# QA — Roteiro de validação (POC Mining Truck Simulator)

Roteiro manual para validar os **9 critérios de aceitação** num Unity 6 real, mais a
verificação automatizada que já roda fora do Editor.

## Pré-requisitos

- Unity **6000.0.32f1** (ou 6000.0.x) com URP.
- Backend rodando (para os itens 7): `cd backend && uvicorn app.main:app`.
- Cena: GameObject vazio com **`MineWorldBootstrap`** (Play). Para o menu de
  configuração/login, um GameObject com `OperationSetupMenu` + `UserLoginMenu`.

## Verificação automatizada (sem Editor)

- [ ] **Backend:** `cd backend && pytest -q` → 7 testes verdes.
- [ ] **CI:** workflow *Backend (FastAPI)* verde no PR.
- [ ] **Lógica de jogo:** Test Runner → EditMode (`MiningTruckSim.Tests`) todos verdes
      (cobrem scoring, ciclo, trilho, alertas, configuração). No CI requer secrets de
      licença Unity (`UNITY_LICENSE`); sem eles o job é pulado.

## Critérios de aceitação

1. **Condução 1ª pessoa (cabine)**
   - [ ] Ao dar Play, a câmera está dentro da cabine; mouse olha em volta dentro dos limites.
   - [ ] `W/S` aceleram/freiam, `A/D` esterçam; o caminhão se move de forma estável.

2. **Loading: ir à escavadeira, estacionar, ver animação de carregamento**
   - [ ] Dirigir até a plataforma **azul**; a escavadeira é visível.
   - [ ] Estacionar alinhado no ponto → escavadeira **anima** e a caçamba **enche** (HUD).

3. **Rota/trilho no mapa aberto + penalização**
   - [ ] A **linha amarela** liga loading→unload; o HUD mostra **progresso**.
   - [ ] Sair da tolerância lateral aciona **⚠ FORA DO TRILHO** e a **penalidade** sobe.

4. **Unload no ponto demarcado**
   - [ ] Carregado, ir à plataforma **laranja**, estacionar e segurar **`B`** (báscula) →
         a carga é **descarregada** e o ciclo conclui.

5. **Interagir com os controles**
   - [ ] `I` ignição, `Q/E` marcha (P/R/N/D), `Espaço` freio de mão, `B` báscula,
         `L` faróis, `H` buzina — todos respondem (HUD reflete o estado).

6. **Pontuação por faixa de operação perfeita**
   - [ ] Manter RPM/velocidade/temperatura/carga na faixa acumula pontos.
   - [ ] Ao concluir o ciclo aparece a **tela de resultado** com **rating S/A/B/C/D**.

7. **Criar/salvar usuários e jogadas**
   - [ ] `UserLoginMenu` cria/seleciona usuário (backend online).
   - [ ] Após a operação, **`LeaderboardScreen`** mostra o ranking; `GET /sessions` e
         `GET /leaderboard` refletem a jogada.
   - [ ] Com a API offline, o jogo segue como *Convidado* sem travar.

8. **Configurar operação: 2 minas + N ciclos**
   - [ ] `OperationSetupMenu` permite escolher **fácil/difícil** e **N**.
   - [ ] A dificuldade muda (tolerância do trilho, frequência de alertas); o loop roda os
         **N ciclos** e mostra o **resumo** com pontuação total agregada.

9. **Alertas aleatórios + procedimentos de conserto distintos**
   - [ ] Surgem alertas (suba `alertsPerMinute` p/ testar): pressão de óleo, filtro,
         fora do trilho, excesso de carga.
   - [ ] Cada um resolve pelo **procedimento próprio** (reduzir RPM / parar e segurar `R` /
         voltar ao trilho / aliviar carga); não resolver **penaliza** a pontuação.

## Polimento (S9, sem áudio nesta fase)

- [ ] Ambiente: céu, **névoa** de profundidade e **sombras** do sol aplicados.
- [ ] **Poeira** procedural sai das rodas ao andar e some ao parar.
- [ ] (Pendente, fase futura) Áudio de motor/alertas/buzina mixado.

## Build

- [ ] Build standalone via menu **MiningTruckSim ▸ Build ▸ Windows64 / Linux64**, ou
      headless: `Unity -batchmode -quit -projectPath . -executeMethod
      MiningTruckSim.Editor.BuildScript.BuildLinux64`.
