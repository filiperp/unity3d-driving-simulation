using System.Collections.Generic;
using MiningTruckSim.Scoring;
using MiningTruckSim.Track;
using MiningTruckSim.Vehicle;
using UnityEngine;

namespace MiningTruckSim.Alerts
{
    /// <summary>
    /// Executa o sistema de alertas na cena (critério 9): usa o <see cref="AlertScheduler"/>
    /// para disparar alertas aleatórios conforme a frequência da mina, avalia o
    /// procedimento de conserto de cada um contra o estado do caminhão/mundo e repassa as
    /// penalidades ao <see cref="PerformanceScorer"/>. Expõe os alertas ativos ao HUD.
    /// </summary>
    public sealed class AlertSystem : MonoBehaviour
    {
        public TruckController truck;
        public RouteGuide routeGuide;
        public PerformanceScorer scorer;

        [Tooltip("Frequência média de alertas por minuto (vem da mina escolhida).")]
        public float alertsPerMinute = 0.3f;

        [Tooltip("Seed do gerador. Mude para variar a sequência; fixe para reproduzir.")]
        public int seed = 12345;

        [Tooltip("Tecla de ação manual de conserto (ex.: troca de filtro).")]
        public KeyCode repairKey = KeyCode.R;

        public IReadOnlyList<ActiveAlert> ActiveAlerts => _active;

        private readonly List<ActiveAlert> _active = new List<ActiveAlert>();
        private readonly HashSet<AlertType> _activeTypes = new HashSet<AlertType>();
        private AlertScheduler _scheduler;

        private void Awake()
        {
            _scheduler = new AlertScheduler(alertsPerMinute, seed);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            TrySpawn(dt);
            UpdateActive(dt);
        }

        private void TrySpawn(float dt)
        {
            AlertType? spawned = _scheduler.Tick(dt, GetEligibleTypes(), _activeTypes);
            if (spawned.HasValue)
            {
                _active.Add(new ActiveAlert(spawned.Value));
                _activeTypes.Add(spawned.Value);
            }
        }

        private void UpdateActive(float dt)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ActiveAlert alert = _active[i];
                bool conditionMet = IsRepairConditionMet(alert.Type);
                float penalty = alert.Tick(dt, conditionMet);

                if (penalty > 0f && scorer != null)
                {
                    scorer.Accumulator.ApplyPenalty(penalty);
                }

                if (alert.Resolved)
                {
                    if (scorer != null)
                    {
                        // Contabiliza como alerta resolvido (sem nova penalidade aqui).
                        scorer.RegisterAlertPenalty(0f);
                    }

                    _active.RemoveAt(i);
                    _activeTypes.Remove(alert.Type);
                }
            }
        }

        /// <summary>Tipos que fazem sentido disparar agora, dado o estado do caminhão.</summary>
        private IReadOnlyCollection<AlertType> GetEligibleTypes()
        {
            var eligible = new List<AlertType>
            {
                AlertType.HighOilPressure,
                AlertType.CloggedFilter
            };

            // Overload só faz sentido com carga relevante a bordo.
            if (truck != null && truck.LoadRatio > 0.25f)
            {
                eligible.Add(AlertType.Overload);
            }

            // OffTrack só quando há trilho para sair dele.
            if (routeGuide != null)
            {
                eligible.Add(AlertType.OffTrack);
            }

            return eligible;
        }

        /// <summary>Avalia se o procedimento de conserto do alerta está sendo cumprido.</summary>
        private bool IsRepairConditionMet(AlertType type)
        {
            switch (type)
            {
                case AlertType.HighOilPressure:
                    // Reduzir RPM: aproximar da marcha lenta.
                    return truck != null && truck.Engine.Rpm <= truck.Engine.IdleRpm + 250f;

                case AlertType.CloggedFilter:
                    // Parado e segurando a tecla de conserto.
                    return truck != null && truck.SpeedKmh < 2f && Input.GetKey(repairKey);

                case AlertType.OffTrack:
                    // De volta para dentro da tolerância do trilho.
                    return routeGuide != null && !routeGuide.IsOffTrack;

                case AlertType.Overload:
                    // Carga aliviada (descarregar parte) abaixo da faixa de excesso.
                    return truck != null && truck.LoadRatio < 0.95f;

                default:
                    return false;
            }
        }
    }
}
