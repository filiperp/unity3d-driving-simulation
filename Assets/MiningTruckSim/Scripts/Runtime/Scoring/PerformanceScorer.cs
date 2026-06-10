using MiningTruckSim.Track;
using UnityEngine;

namespace MiningTruckSim.Scoring
{
    /// <summary>
    /// Avalia a performance do caminhão durante um ciclo (critério 6): a cada frame
    /// alimenta o <see cref="ScoreAccumulator"/> com a telemetria atual (pontuando o
    /// tempo na "faixa de operação perfeita") e absorve as penalidades do trilho
    /// (critério 3) e de alertas (critério 9). Produz um <see cref="CycleScore"/>.
    /// </summary>
    public sealed class PerformanceScorer : MonoBehaviour
    {
        public Vehicle.TruckController truck;
        public RouteGuide routeGuide;

        [Tooltip("Pontos por segundo quando a operação está 100% na faixa perfeita.")]
        public float pointsPerSecondInBand = 10f;

        public OperationBand band = OperationBand.Default;

        public ScoreAccumulator Accumulator { get; private set; }
        public int AlertsHandled { get; private set; }

        /// <summary>Soma de todas as penalidades descontadas (trilho + alertas).</summary>
        public float TotalPenalties { get; private set; }

        private float _absorbedTrackPenalty;

        private void Awake()
        {
            Accumulator = new ScoreAccumulator(band, pointsPerSecondInBand);
        }

        private void Update()
        {
            if (truck == null)
            {
                return;
            }

            Accumulator.Tick(Time.deltaTime, truck.Telemetry);
            AbsorbTrackPenalty();
        }

        /// <summary>
        /// Transfere o incremento de penalidade do trilho (acumulada em RouteGuide) para
        /// o score, evitando contar o mesmo total mais de uma vez.
        /// </summary>
        private void AbsorbTrackPenalty()
        {
            if (routeGuide == null)
            {
                return;
            }

            float total = routeGuide.TotalPenalty;
            float delta = total - _absorbedTrackPenalty;
            if (delta > 0f)
            {
                Accumulator.ApplyPenalty(delta);
                _absorbedTrackPenalty = total;
                TotalPenalties += delta;
            }
        }

        /// <summary>Registra a penalidade de um alerta resolvido (critério 9, Sprint 5).</summary>
        public void RegisterAlertPenalty(float points)
        {
            if (points > 0f)
            {
                Accumulator.ApplyPenalty(points);
                TotalPenalties += points;
            }

            AlertsHandled++;
        }

        /// <summary>Fecha a contabilização e devolve o resultado do ciclo.</summary>
        public CycleScore BuildResult(int cycleIndex, float loadTonnes)
        {
            int offTrackEvents = routeGuide != null && routeGuide.Monitor != null
                ? routeGuide.Monitor.OffTrackEvents
                : 0;

            // Accumulator.Score já é líquido das penalidades; reconstruímos o bruto.
            return new CycleScore
            {
                CycleIndex = cycleIndex,
                BaseScore = Accumulator.Score + TotalPenalties,
                Penalties = TotalPenalties,
                BandRatio = Accumulator.BandRatio,
                OffTrackEvents = offTrackEvents,
                AlertsHandled = AlertsHandled,
                LoadTonnes = loadTonnes
            };
        }
    }
}
