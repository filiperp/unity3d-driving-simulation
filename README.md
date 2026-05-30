# Mining Truck Simulator

POC de simulador de **caminhão de mineração em primeira pessoa** (visão de cabine)
feito em **Unity 6 + URP**, com backend **FastAPI** para usuários, configuração de
operação e persistência de jogadas.

> Status: **Sprint 0 (Fundação)** concluída. Veja [`ROADMAP.md`](ROADMAP.md) para o
> plano completo em sprints e o mapeamento dos critérios de aceitação.

## Estrutura

```
.
├── Assets/MiningTruckSim/      # Código e conteúdo do jogo (Unity)
│   └── Scripts/
│       ├── Runtime/            # Lógica de runtime (asmdef: MiningTruckSim.Runtime)
│       └── Tests/              # Testes EditMode (asmdef: MiningTruckSim.Tests)
├── Packages/manifest.json      # Dependências do Unity (URP, Input System, Cinemachine…)
├── ProjectSettings/            # Configurações do projeto Unity
├── backend/                    # API FastAPI + SQLite
│   ├── app/
│   └── tests/
└── ROADMAP.md
```

## Unity

- Abra a pasta raiz como projeto no **Unity Hub** com **Unity 6000.x** instalado.
- Na primeira abertura, o Unity resolve os pacotes do `Packages/manifest.json`
  (URP, Input System, Cinemachine, AI Navigation, Test Framework) e regenera os
  arquivos derivados (`Library/`, metas faltantes).
- Os testes EditMode ficam em **Window → General → Test Runner**.

> Observação: o conteúdo visual usa **primitivas procedurais** nesta fase. A troca
> pelos modelos comprados está planejada na **Sprint 8**, reaproveitando os slots
> já previstos nos prefabs.

## Backend (FastAPI)

Requer Python 3.11+.

```bash
cd backend
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt

# rodar a API (http://127.0.0.1:8000, docs em /docs)
uvicorn app.main:app --reload

# testes
pytest -q
```

### Endpoints principais

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET`  | `/health` | Healthcheck |
| `GET`  | `/mines` | Lista as 2 minas (fácil/difícil) e seus parâmetros |
| `POST` | `/users` | Cria usuário |
| `GET`  | `/users` | Lista usuários |
| `GET`  | `/users/{id}` | Detalhe de usuário |
| `POST` | `/sessions` | Cria operação (usuário + mina + N ciclos) |
| `GET`  | `/sessions` | Lista operações |
| `GET`  | `/sessions/{id}` | Operação + ciclos |
| `POST` | `/sessions/{id}/cycles` | Registra resultado de um ciclo |
| `POST` | `/sessions/{id}/finish` | Finaliza a operação (agrega score) |
| `GET`  | `/leaderboard` | Ranking por pontuação total |
