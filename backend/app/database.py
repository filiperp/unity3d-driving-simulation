"""Configuração do banco (SQLite via SQLModel)."""
import os
from collections.abc import Iterator

from sqlmodel import Session, SQLModel, create_engine

DATABASE_URL = os.getenv("DATABASE_URL", "sqlite:///./mining_sim.db")

# check_same_thread=False permite uso da mesma conexão entre threads do servidor.
engine = create_engine(
    DATABASE_URL,
    echo=False,
    connect_args={"check_same_thread": False},
)


def init_db() -> None:
    """Cria as tabelas (idempotente)."""
    # Importa os modelos para registrá-los no metadata antes do create_all.
    from app import models  # noqa: F401

    SQLModel.metadata.create_all(engine)


def get_session() -> Iterator[Session]:
    """Dependency do FastAPI: fornece uma sessão por request."""
    with Session(engine) as session:
        yield session
