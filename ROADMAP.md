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

### Sprint 3 — Rota, Trilho & Penalização ✅ → critério 3
- Trilho via waypoints entre loading e unload (`TrackPath`/`RouteTrack`), desenhado
  com `LineRenderer` no **mapa aberto**.
- Detecção de saída do trilho por desvio lateral (`RouteTrack.Sample`) + **penalização**
  acumulada proporcional ao excesso e ao tempo (`OffTrackMonitor`/`RouteGuide`).
- Progresso ao longo da rota e alerta de "fora do trilho" no HUD.
- **Testes:** `RouteTrack` (projeção/desvio/progresso) e `OffTrackMonitor` (penalização).

### Sprint 4 — Performance & Pontuação ✅ → critério 6
- `PerformanceScorer`: pontua o tempo na "faixa de operação perfeita"
  (`OperationBand` + `ScoreAccumulator`) e absorve as penalidades do trilho (S3) e
  de alertas (S5).
- `CycleScore`: resultado consolidado (final = base − penalidades) com **rating S/A/B/C/D**.
- `CycleResultScreen` + `CycleResultPresenter`: tela de resultado ao concluir o ciclo.
- **Testes:** `CycleScore` (final/rating) somando aos de `OperationBand`/`ScoreAccumulator`.

### Sprint 5 — Alertas & Procedimentos ✅ → critério 9
- `AlertScheduler`: motor de eventos aleatórios (Poisson, determinístico por seed),
  frequência vinda da mina (critério 8); tipos: **pressão de óleo alta, filtro
  entupido, saída do trilho, excesso de carga** (extensível via `AlertCatalog`).
- Cada alerta com **procedimento de conserto distinto** (`RepairProcedure`): condição
  sustentada (reduzir RPM / voltar ao trilho / aliviar carga) ou ação manual (segurar
  [R] parado p/ trocar filtro), com barra de progresso no HUD.
- **Penalidade por segundo** enquanto não resolvido, descontada do scoring (S4).
- **Testes:** `AlertScheduler` (determinismo/seed, taxa, sem duplicar tipo),
  `ActiveAlert` (resolução/penalidade) e `AlertCatalog` (procedimentos distintos).

### Sprint 6 — Configuração de Operação & 2 Minas ✅ → critério 8
- `OperationSetupMenu`: escolher **mina fácil/difícil** e **N ciclos** (configurável),
  gravando em `OperationContext` lido pela cena.
- `MineCatalog` (2 minas) espelha o backend (`/mines`): distância, curvas, inclinação,
  tolerância do trilho e frequência de alertas → dificuldade aplicada na cena.
- `OperationRunner` + `OperationProgress`: **loop de N ciclos** com reinício automático
  do ciclo e **agregação de pontuação**; `OperationSummaryScreen` com o resumo final.
- **Testes:** `MineCatalog`, `OperationConfig` (validação de N) e `OperationProgress`
  (progressão e agregação dos N ciclos).

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
- [x] Sprint 3 — Rota & Trilho
- [x] Sprint 4 — Performance & Pontuação
- [x] Sprint 5 — Alertas & Procedimentos
- [x] Sprint 6 — Configuração & 2 Minas
- [ ] Sprint 7 — Usuários & Persistência (integração)
- [ ] Sprint 8 — Troca de Assets
- [ ] Sprint 9 — Polimento & QA
