"""Modelos de dados (tabelas) e payloads de entrada."""
from datetime import datetime, timezone
from enum import Enum
from typing import Optional

from sqlmodel import Field, SQLModel


def _utcnow() -> datetime:
    return datetime.now(timezone.utc)


class MineId(str, Enum):
    easy = "easy"
    hard = "hard"


class SessionStatus(str, Enum):
    active = "active"
    finished = "finished"
    aborted = "aborted"


# --------------------------------------------------------------------------- #
# Usuários (critério 7)
# --------------------------------------------------------------------------- #
class User(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    username: str = Field(index=True, unique=True)
    display_name: str = ""
    created_at: datetime = Field(default_factory=_utcnow)


class UserCreate(SQLModel):
    username: str
    display_name: str = ""


# --------------------------------------------------------------------------- #
# Operação / sessão de jogo (critérios 7 e 8)
# --------------------------------------------------------------------------- #
class GameSession(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    user_id: int = Field(foreign_key="user.id", index=True)
    mine: MineId
    cycles_planned: int = 1
    status: SessionStatus = Field(default=SessionStatus.active)
    total_score: float = 0.0
    started_at: datetime = Field(default_factory=_utcnow)
    finished_at: Optional[datetime] = None


class SessionCreate(SQLModel):
    user_id: int
    mine: MineId
    cycles_planned: int = 1


# --------------------------------------------------------------------------- #
# Resultado de um ciclo (critérios 3, 6 e 9)
# --------------------------------------------------------------------------- #
class CycleResult(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    session_id: int = Field(foreign_key="gamesession.id", index=True)
    cycle_index: int
    score: float = 0.0
    time_in_band_s: float = 0.0
    off_track_penalties: int = 0
    alerts_handled: int = 0
    load_tonnes: float = 0.0
    created_at: datetime = Field(default_factory=_utcnow)


class CycleResultCreate(SQLModel):
    cycle_index: int
    score: float = 0.0
    time_in_band_s: float = 0.0
    off_track_penalties: int = 0
    alerts_handled: int = 0
    load_tonnes: float = 0.0
