"""Testes de fumaça/integração da API."""


def test_health(client):
    r = client.get("/health")
    assert r.status_code == 200
    assert r.json()["status"] == "ok"


def test_mines_lists_two(client):
    r = client.get("/mines")
    assert r.status_code == 200
    mines = r.json()
    ids = {m["id"] for m in mines}
    assert ids == {"easy", "hard"}


def test_user_crud_and_unique_username(client):
    r = client.post("/users", json={"username": "operador1", "display_name": "Ana"})
    assert r.status_code == 201
    user = r.json()
    assert user["id"] > 0

    # username duplicado deve falhar
    r2 = client.post("/users", json={"username": "operador1"})
    assert r2.status_code == 409

    r3 = client.get(f"/users/{user['id']}")
    assert r3.status_code == 200
    assert r3.json()["display_name"] == "Ana"


def test_session_requires_existing_user(client):
    r = client.post("/sessions", json={"user_id": 999, "mine": "easy", "cycles_planned": 2})
    assert r.status_code == 404


def test_full_operation_flow_and_leaderboard(client):
    user = client.post("/users", json={"username": "driver", "display_name": "Bru"}).json()

    session = client.post(
        "/sessions",
        json={"user_id": user["id"], "mine": "hard", "cycles_planned": 2},
    ).json()
    sid = session["id"]
    assert session["status"] == "active"

    # registra dois ciclos
    client.post(
        f"/sessions/{sid}/cycles",
        json={"cycle_index": 0, "score": 120.5, "time_in_band_s": 90, "load_tonnes": 200},
    )
    client.post(
        f"/sessions/{sid}/cycles",
        json={"cycle_index": 1, "score": 80.0, "off_track_penalties": 2},
    )

    detail = client.get(f"/sessions/{sid}").json()
    assert len(detail["cycles"]) == 2

    finished = client.post(f"/sessions/{sid}/finish").json()
    assert finished["status"] == "finished"
    assert finished["total_score"] == 200.5
    assert finished["finished_at"] is not None

    # não pode adicionar ciclo após finalizar
    r = client.post(f"/sessions/{sid}/cycles", json={"cycle_index": 2})
    assert r.status_code == 409

    lb = client.get("/leaderboard").json()
    assert lb[0]["username"] == "driver"
    assert lb[0]["total_score"] == 200.5
