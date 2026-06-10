# Roadmap — Mining Truck Simulator (Unity 6 + URP / FastAPI)

POC de simulador de caminhão de mineração em primeira pessoa (visão de cabine),
com ciclo completo de operação (loading → rota/trilho → unload → pontuação),
alertas com procedimentos de conserto, configuração de operação e persistência
de usuários/jogadas via backend FastAPI.

## Princípios

- **Sprints pequenas e verificáveis.** Cada sprint entrega algo concreto + testes.
- **Lógica pura testável.** Regras de jogo (scoring, faixa de operação, alertas,
  detecção de trilho) ficam em C# puro/`EditMode`-testável, isoladas de
  `MonoBehaviour`/render. O backend é testado com `pytest` neste ambiente.
- **Procedural-first.** Tudo é montado com primitivas (cubos/cilindros) e código.
  Os prefabs já nascem com "slots" para troca por modelos comprados (Sprint 8).
- **Interações operacionais desde cedo.** Báscula (dump), botões, ignição, luzes,
  buzina, marchas — tudo funcional mesmo com a arte provisória.

## Convenções

- Render: **Universal Render Pipeline (URP)** / Unity **6000.x**.
- Input: **Input System** (novo).
- Câmera: **Cinemachine 3**.
- Navegação/IA: **AI Navigation (NavMesh)** para a escavadeira/tráfego.
- Namespaces: `MiningTruckSim.*` (Runtime, Tests).
- Backend: **FastAPI + SQLModel + SQLite** (`backend/`).

---

## Mapa Critérios de Aceitação → Sprints

| # | Critério | Sprint |
|---|----------|--------|
| 1 | Condução 1ª pessoa, visão de cabine | S1 |
| 5 | Interagir com os controles do caminhão | S1 |
| 2 | Ir ao loading, ver escavadeira, estacionar, animação de carregamento | S2 |
| 4 | Unload no ponto demarcado (báscula) | S2 |
| 3 | Dirigir até unload seguindo o trilho, penalizar fora do trilho, mapa aberto | S3 |
| 6 | Pontuação por manter o caminhão em faixa de operação perfeita | S4 |
| 9 | Alertas aleatórios com procedimentos de conserto distintos | S5 |
| 8 | Configurar operação: 2 minas (fácil/difícil), N ciclos | S6 |
| 7 | Criar e salvar usuários e jogadas | S7 (backend já em S0) |

---

## Sprints

### Sprint 0 — Fundação ✅ (esta entrega)
- Estrutura do projeto Unity 6 + URP (`Packages/manifest.json`, `ProjectSettings`,
  pastas, **assembly definitions** Runtime/Tests).
- Esqueleto do backend **FastAPI** (usuários, sessões, ciclos, minas, leaderboard)
  com **SQLite** e **testes pytest** passando.
- Primeira lógica de domínio testável: `OperationBand` (base do critério 6).
- `ROADMAP.md`, `README.md`, `.editorconfig`.
- **Testes:** `pytest` (backend) verde; `OperationBandTests` (EditMode) prontos.

### Sprint 1 — Veículo & Cabine (1ª pessoa) ✅ → critérios 1, 5
- `TruckController` com física (`WheelCollider`), câmbio automático (P/R/N/D), ré,
  freio de serviço e freio de mão.
- `CabinCamera` em 1ª pessoa (look around com o mouse, ancorada na cabine).
- Controles interativos: ignição, acelerador/freio, marcha, **báscula (`DumpBed`)**,
  **buzina** (áudio procedural), **faróis** (`TruckLights`), freio de estacionamento.
- HUD (IMGUI) com velocímetro, marcha, RPM, temperatura, carga e ajuda de controles.
- `ProceduralTruckBuilder` + `TruckSimBootstrap`: monta caminhão/cena de primitivas,
  com geometria sob `Body_ProceduralSlot` (slot p/ troca de modelo na S8).
- **Testes:** `Gearbox`, `EngineModel`, `DumpBedMotor` (EditMode) — lógica pura.

### Sprint 2 — Mundo & Ciclo de Loading ✅ → critérios 2, 4
- Cena de mina procedural (`MineWorldBootstrap`): chão, áreas de **loading** e **unload**.
- `Excavator` procedural com **animação de carregamento** (giro + concha).
- `OperationZone` (gatilho da área + ponto de parada) e `ParkingCheck`
  (alinhamento/estacionamento).
- Enchimento progressivo da caçamba (toneladas) e **unload** via báscula no ponto
  demarcado, com HUD de ciclo (`CycleHud`).
- **Testes:** `OperationCycle` (idle→loading→loaded→unloading→done) e `ParkingCheck`.

### Sprint 3 — Rota, Trilho & Penalização → critério 3
- Trilho via **spline**/waypoints entre loading e unload; **mapa aberto**.
- Detecção de saída do trilho (distância lateral) + **penalização** acumulada.
- Minimapa/seta de orientação.
- **Testes:** cálculo de desvio lateral e regra de penalização (lógica pura).

### Sprint 4 — Performance & Pontuação → critério 6
- "Faixa de operação perfeita": RPM/velocidade/temperatura/carga ideais.
- Acúmulo de score por tempo dentro da faixa; combinação com penalidades.
- Tela de resultado de ciclo/operação.
- **Testes:** `OperationBand` + `ScoreCalculator` (lógica pura).

### Sprint 5 — Alertas & Procedimentos → critério 9
- Motor de eventos aleatórios: **pressão de óleo alta, filtro entupido, saída do
  trilho, excesso de carga** (extensível).
- Cada alerta com **procedimento de conserto distinto** (mini-interação/checklist).
- Impacto na performance enquanto não resolvido.
- **Testes:** agendador de alertas (determinístico via seed) + resolução.

### Sprint 6 — Configuração de Operação & 2 Minas → critério 8
- Menu: escolher **mina fácil/difícil** e **N ciclos** (configurável).
- Dois layouts de mina (parâmetros: distância, curvas, inclinação, frequência de
  alertas) já espelhados no backend (`/mines`).
- Loop de N ciclos com agregação de pontuação.
- **Testes:** progressão de ciclos e agregação.

### Sprint 7 — Usuários & Persistência (integração Unity ↔ API) → critério 7
- Cliente Unity para a API (`UnityWebRequest`): login/seleção de usuário,
  criar operação, enviar resultado de cada ciclo, finalizar sessão.
- Histórico de jogadas e leaderboard na UI.
- **Testes:** integração contra a API (já coberta por pytest no backend).

### Sprint 8 — Troca de Assets (modelos comprados)
- Substituir primitivas pelos modelos reais (caminhão, escavadeira, cenário)
  reaproveitando os slots/prefab variants criados desde a S1.
- Ajuste de escala, colisores, pontos de articulação (báscula), animações.

### Sprint 9 — Polimento, Build & QA
- Áudio (motor, alertas), partículas (poeira), iluminação/skybox URP.
- Build settings, otimização, configuração de qualidade.
- QA final contra os 9 critérios de aceitação.

---

## Estado atual

- [x] Sprint 0 — Fundação
- [x] Sprint 1 — Veículo & Cabine
- [x] Sprint 2 — Mundo & Loading
- [ ] Sprint 3 — Rota & Trilho
- [ ] Sprint 4 — Performance & Pontuação
- [ ] Sprint 5 — Alertas & Procedimentos
- [ ] Sprint 6 — Configuração & 2 Minas
- [ ] Sprint 7 — Usuários & Persistência (integração)
- [ ] Sprint 8 — Troca de Assets
- [ ] Sprint 9 — Polimento & QA
