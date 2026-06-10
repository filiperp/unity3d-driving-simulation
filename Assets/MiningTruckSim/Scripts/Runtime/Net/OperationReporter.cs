using MiningTruckSim.Config;
using MiningTruckSim.Operation;
using MiningTruckSim.Scoring;
using UnityEngine;

namespace MiningTruckSim.Net
{
    /// <summary>
    /// Faz a ponte entre o loop de operação e o backend (critério 7): ao iniciar, cria a
    /// sessão (usuário + mina + N ciclos); a cada ciclo concluído, envia o resultado; ao
    /// fim, finaliza a operação (o backend agrega o score total). Tolerante a falhas de
    /// rede: se a API estiver offline, o jogo continua e apenas registra um aviso.
    /// </summary>
    public sealed class OperationReporter : MonoBehaviour
    {
        public MiningApiClient api;
        public OperationRunner runner;

        public bool IsOnline { get; private set; }
        public int SessionId { get; private set; }
        public string LastError { get; private set; }

        private void Start()
        {
            if (api == null || runner == null)
            {
                return;
            }

            runner.CycleCompleted += OnCycleCompleted;
            runner.OperationCompleted += OnOperationCompleted;

            int userId = OperationContext.ActiveUserId;
            if (userId <= 0)
            {
                // Sem usuário logado: opera apenas localmente (critério 7 é opcional aqui).
                return;
            }

            OperationConfig cfg = OperationContext.Config;
            api.CreateSession(userId, cfg.Mine.Id, cfg.Cycles,
                session =>
                {
                    SessionId = session.id;
                    IsOnline = true;
                },
                err => LastError = err);
        }

        private void OnDestroy()
        {
            if (runner != null)
            {
                runner.CycleCompleted -= OnCycleCompleted;
                runner.OperationCompleted -= OnOperationCompleted;
            }
        }

        private void OnCycleCompleted(CycleScore result)
        {
            if (!IsOnline || SessionId <= 0)
            {
                return;
            }

            var dto = new CycleResultCreateDto
            {
                cycle_index = result.CycleIndex,
                score = result.Final,
                time_in_band_s = result.TimeInBandSeconds,
                off_track_penalties = result.OffTrackEvents,
                alerts_handled = result.AlertsHandled,
                load_tonnes = result.LoadTonnes
            };

            api.AddCycle(SessionId, dto, null, err => LastError = err);
        }

        private void OnOperationCompleted(OperationProgress progress)
        {
            if (!IsOnline || SessionId <= 0)
            {
                return;
            }

            api.FinishSession(SessionId, null, err => LastError = err);
        }
    }
}
