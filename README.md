# Mining Truck Simulator

POC de simulador de **caminhão de mineração em primeira pessoa** (visão de cabine)
feito em **Unity 6 + URP**, com backend **FastAPI** para usuários, configuração de
operação e persistência de jogadas.

> Status: **Sprints 0–7 + Sprint 9 (Polimento/Build/QA, sem áudio)** concluídas — os
> **9 critérios de aceitação** estão cobertos. Veja [`ROADMAP.md`](ROADMAP.md) para o
> plano e [`QA_CHECKLIST.md`](QA_CHECKLIST.md) para o roteiro de validação. Falta apenas
> a **Sprint 8** (troca das primitivas pelos modelos comprados) e, como pendência futura,
> o **áudio**.

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

A cena da Sprint 2 (`MineWorldBootstrap`) já inclui o **trilho esperado** (linha amarela)
entre loading e unload, adicionado na Sprint 3:

- siga a linha amarela; o HUD mostra o **progresso** ao longo da rota;
- se afastar além da tolerância lateral (mina fácil ~4 m, difícil ~2 m), o HUD acusa
  **⚠ FORA DO TRILHO** e a **penalidade** acumula proporcionalmente ao desvio e ao tempo.

Ao **concluir o ciclo** (descarga completa), a Sprint 4 mostra a **tela de resultado**
com a pontuação de performance: pontos por tempo na **faixa de operação perfeita**
(RPM/velocidade/temperatura/carga ideais) menos as penalidades, com **rating S/A/B/C/D**.

Durante a operação, a Sprint 5 dispara **alertas aleatórios** com **procedimentos de
conserto distintos** (canto esquerdo, com barra de progresso do reparo):

| Alerta | Como consertar |
|--------|----------------|
| Pressão de óleo alta | Reduza o RPM (solte o acelerador) e mantenha até normalizar |
| Filtro de ar entupido | Pare o caminhão e segure **`R`** para trocar o filtro |
| Fora do trilho | Retorne ao trilho esperado |
| Excesso de carga | Alivie a carga (descarregue) até a faixa nominal |

Cada alerta não resolvido **penaliza a pontuação** por segundo. A frequência de alertas
depende da mina escolhida (critério 8).

### Configurar a operação (Sprint 6 — critério 8)

- O componente **`OperationSetupMenu`** (num GameObject de um menu) deixa escolher entre
  as **2 minas** — *Vale Verde (fácil)* e *Serra Negra (difícil)* — e o **número de ciclos
  (N)**, gravando em `OperationContext`.
- A cena `MineWorldBootstrap` lê essa configuração (`useOperationContext = true`) e aplica
  a dificuldade da mina (tolerância do trilho, frequência de alertas).
- O **`OperationRunner`** roda o **loop de N ciclos**: a cada ciclo concluído reposiciona o
  caminhão e reinicia; ao fim dos N, mostra o **resumo da operação** com a pontuação total
  agregada e a média de tempo na faixa perfeita.

> Para testar rápido sem montar o menu, deixe `useOperationContext = false` no
> `MineWorldBootstrap` (usa mina fácil + 3 ciclos).

### Usuários, persistência e leaderboard (Sprint 7 — critério 7)

Com o backend rodando (`uvicorn app.main:app`), a cena integra-se à API:

1. Suba a API (`cd backend && uvicorn app.main:app`).
2. No `MineWorldBootstrap`, mantenha `enableBackend = true` e ajuste `apiBaseUrl` se
   necessário (padrão `http://127.0.0.1:8000`).
3. Use **`UserLoginMenu`** para criar/selecionar um usuário (gravado em `OperationContext`).
4. Jogue a operação: o **`OperationReporter`** cria a sessão, **envia o resultado de cada
   ciclo** e finaliza a operação (o backend agrega o score total).
5. **`LeaderboardScreen`** mostra o ranking por pontuação total.

> Tudo é tolerante a API offline: sem usuário/servidor, o jogo roda localmente como
> *Convidado* e nada é persistido. O cliente usa `UnityWebRequest` (coroutines, sem
> bloquear o jogo) e o backend já tem **CORS** liberado para o player Unity.

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
