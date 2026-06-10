"""API FastAPI do Mining Truck Simulator.

Cobre persistência de usuários e jogadas (critério 7) e a configuração de
operação por mina/ciclos (critério 8). A pontuação calculada no cliente Unity
(critérios 3, 6 e 9) é registrada por ciclo e agregada ao finalizar a operação.
"""
from contextlib import asynccontextmanager
from datetime import datetime, timezone

from fastapi import Depends, FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.exc import IntegrityError
from sqlmodel import Session, select

from app import mines as mines_catalog
from app.database import get_session, init_db
from app.models import (
    CycleResult,
    CycleResultCreate,
    GameSession,
    MineId,
    SessionCreate,
    SessionStatus,
    User,
    UserCreate,
)

@asynccontextmanager
async def lifespan(_: FastAPI):
    init_db()
    yield


app = FastAPI(
    title="Mining Truck Simulator API",
    version="0.1.0",
    description="Backend de usuários, operações e pontuação do simulador.",
    lifespan=lifespan,
)

# CORS liberado para o cliente Unity (Editor/Player/WebGL) acessar a API (critério 7).
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health", tags=["meta"])
def health() -> dict:
    return {"status": "ok", "version": app.version}


# --------------------------------------------------------------------------- #
# Minas (critério 8)
# --------------------------------------------------------------------------- #
@app.get("/mines", tags=["mines"])
def list_mines() -> list[dict]:
    return mines_catalog.list_mines()


@app.get("/mines/{mine_id}", tags=["mines"])
def get_mine(mine_id: MineId) -> dict:
    mine = mines_catalog.get_mine(mine_id)
    if mine is None:
        raise HTTPException(status_code=404, detail="Mina não encontrada")
    return mine


# --------------------------------------------------------------------------- #
# Usuários (critério 7)
# --------------------------------------------------------------------------- #
@app.post("/users", response_model=User, status_code=201, tags=["users"])
def create_user(payload: UserCreate, db: Session = Depends(get_session)) -> User:
    user = User(username=payload.username, display_name=payload.display_name)
    db.add(user)
    try:
        db.commit()
    except IntegrityError:
        db.rollback()
        raise HTTPException(status_code=409, detail="username já existe")
    db.refresh(user)
    return user


@app.get("/users", response_model=list[User], tags=["users"])
def list_users(db: Session = Depends(get_session)) -> list[User]:
    return list(db.exec(select(User)).all())


@app.get("/users/{user_id}", response_model=User, tags=["users"])
def get_user(user_id: int, db: Session = Depends(get_session)) -> User:
    user = db.get(User, user_id)
    if user is None:
        raise HTTPException(status_code=404, detail="Usuário não encontrado")
    return user


# --------------------------------------------------------------------------- #
# Operações / sessões (critérios 7 e 8)
# --------------------------------------------------------------------------- #
@app.post("/sessions", response_model=GameSession, status_code=201, tags=["sessions"])
def create_session(payload: SessionCreate, db: Session = Depends(get_session)) -> GameSession:
    if db.get(User, payload.user_id) is None:
        raise HTTPException(status_code=404, detail="Usuário não encontrado")
    if payload.cycles_planned < 1:
        raise HTTPException(status_code=422, detail="cycles_planned deve ser >= 1")

    game = GameSession(
        user_id=payload.user_id,
        mine=payload.mine,
        cycles_planned=payload.cycles_planned,
    )
    db.add(game)
    db.commit()
    db.refresh(game)
    return game


@app.get("/sessions", response_model=list[GameSession], tags=["sessions"])
def list_sessions(db: Session = Depends(get_session)) -> list[GameSession]:
    return list(db.exec(select(GameSession)).all())


@app.get("/sessions/{session_id}", tags=["sessions"])
def get_session_detail(session_id: int, db: Session = Depends(get_session)) -> dict:
    game = db.get(GameSession, session_id)
    if game is None:
        raise HTTPException(status_code=404, detail="Operação não encontrada")
    cycles = db.exec(
        select(CycleResult).where(CycleResult.session_id == session_id)
    ).all()
    return {"session": game, "cycles": list(cycles)}


@app.post(
    "/sessions/{session_id}/cycles",
    response_model=CycleResult,
    status_code=201,
    tags=["sessions"],
)
def add_cycle(
    session_id: int,
    payload: CycleResultCreate,
    db: Session = Depends(get_session),
) -> CycleResult:
    game = db.get(GameSession, session_id)
    if game is None:
        raise HTTPException(status_code=404, detail="Operação não encontrada")
    if game.status != SessionStatus.active:
        raise HTTPException(status_code=409, detail="Operação não está ativa")

    cycle = CycleResult(session_id=session_id, **payload.model_dump())
    db.add(cycle)
    db.commit()
    db.refresh(cycle)
    return cycle


@app.post("/sessions/{session_id}/finish", response_model=GameSession, tags=["sessions"])
def finish_session(session_id: int, db: Session = Depends(get_session)) -> GameSession:
    game = db.get(GameSession, session_id)
    if game is None:
        raise HTTPException(status_code=404, detail="Operação não encontrada")

    cycles = db.exec(
        select(CycleResult).where(CycleResult.session_id == session_id)
    ).all()
    game.total_score = sum(c.score for c in cycles)
    game.status = SessionStatus.finished
    game.finished_at = datetime.now(timezone.utc)
    db.add(game)
    db.commit()
    db.refresh(game)
    return game


# --------------------------------------------------------------------------- #
# Leaderboard (critério 6/7)
# --------------------------------------------------------------------------- #
@app.get("/leaderboard", tags=["meta"])
def leaderboard(limit: int = 10, db: Session = Depends(get_session)) -> list[dict]:
    stmt = (
        select(GameSession, User)
        .join(User, User.id == GameSession.user_id)
        .where(GameSession.status == SessionStatus.finished)
        .order_by(GameSession.total_score.desc())
        .limit(limit)
    )
    rows = db.exec(stmt).all()
    return [
        {
            "session_id": s.id,
            "username": u.username,
            "display_name": u.display_name,
            "mine": s.mine.value,
            "cycles_planned": s.cycles_planned,
            "total_score": s.total_score,
            "finished_at": s.finished_at,
        }
        for s, u in rows
    ]
