"""Catálogo estático das 2 minas (critério 8).

Os parâmetros são espelhados pelo cliente Unity para configurar a dificuldade da
operação (distância da rota, curvas, inclinação e frequência de alertas).
"""
from app.models import MineId

MINES = {
    MineId.easy: {
        "id": MineId.easy.value,
        "name": "Mina Vale Verde (Fácil)",
        "difficulty": "easy",
        "route_length_m": 600,
        "curve_count": 3,
        "max_grade_pct": 6,
        "alert_frequency": 0.15,   # alertas por minuto (aprox.)
        "off_track_tolerance_m": 4.0,
        "recommended_cycles": 3,
    },
    MineId.hard: {
        "id": MineId.hard.value,
        "name": "Mina Serra Negra (Difícil)",
        "difficulty": "hard",
        "route_length_m": 1400,
        "curve_count": 8,
        "max_grade_pct": 12,
        "alert_frequency": 0.45,
        "off_track_tolerance_m": 2.0,
        "recommended_cycles": 5,
    },
}


def list_mines() -> list[dict]:
    return list(MINES.values())


def get_mine(mine_id: MineId) -> dict | None:
    return MINES.get(mine_id)
