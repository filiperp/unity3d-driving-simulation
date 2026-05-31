# Mining Truck Simulator

POC de simulador de **caminhão de mineração em primeira pessoa** (visão de cabine)
feito em **Unity 6 + URP**, com backend **FastAPI** para usuários, configuração de
operação e persistência de jogadas.

> Status: **Sprint 2 (Mundo & Ciclo de Loading)** concluída. Veja [`ROADMAP.md`](ROADMAP.md)
> para o plano completo em sprints e o mapeamento dos critérios de aceitação.

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

### Testar a cena da Sprint 1 (caminhão jogável)

1. Crie uma cena nova (`File → New Scene`, modelo Basic/URP) e salve em
   `Assets/MiningTruckSim/Scenes/Sandbox.unity`.
2. Crie um GameObject vazio e adicione o componente **`TruckSimBootstrap`**.
3. Dê **Play**. O `TruckSimBootstrap` gera chão, sol, câmera de cabine e o caminhão
   procedural automaticamente — sem montagem manual.

> A geração é via código (`ProceduralTruckBuilder`), então a cena pode ficar
> praticamente vazia. Quando os modelos comprados chegarem (S8), basta substituir
> o conteúdo de `Body_ProceduralSlot`.

#### Controles (critério 5)

| Tecla | Ação |
|-------|------|
| `W` / `S` | Acelerador / freio |
| `A` / `D` | Direção |
| Mouse | Olhar em volta (cabine) |
| `I` | Ignição (liga/desliga o motor) |
| `Q` / `E` | Trocar marcha (P → R → N → D) |
| `Espaço` | Freio de mão |
| `B` | Báscula (levanta/abaixa a caçamba) |
| `L` | Faróis |
| `H` | Buzina |

Os testes EditMode da lógica de veículo ficam em **Window → General → Test Runner**
(`MiningTruckSim.Tests`).

### Testar a cena da Sprint 2 (ciclo de loading/unload)

1. Cena nova → GameObject vazio → componente **`MineWorldBootstrap`** → **Play**.
2. O bootstrap gera o caminhão, a **plataforma azul de loading** (com escavadeira) e a
   **plataforma laranja de unload**, ligados a um `CycleDirector`.
3. Fluxo do ciclo (HUD no canto direito guia cada passo):
   - dirija até a plataforma **azul** e **estacione alinhado** no ponto → a escavadeira
     começa a **animação de carregamento** e a caçamba enche;
   - quando cheia, dirija até a plataforma **laranja**, estacione e pressione **`B`**
     para erguer a báscula → a carga é **descarregada** e o ciclo conclui.

> A escavadeira também usa `Body_ProceduralSlot` (troca de modelo na S8).

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
