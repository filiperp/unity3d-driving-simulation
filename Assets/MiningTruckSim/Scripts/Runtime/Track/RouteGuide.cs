using MiningTruckSim.Vehicle;
using UnityEngine;

namespace MiningTruckSim.Track
{
    /// <summary>
    /// Acompanha a posição do caminhão sobre o <see cref="TrackPath"/>, aplica a
    /// penalização por sair do trilho (critério 3) e expõe o estado para o HUD e para
    /// o scoring. A tolerância lateral vem da configuração da mina (critério 8).
    /// </summary>
    public sealed class RouteGuide : MonoBehaviour
    {
        public TruckController truck;
        public TrackPath path;

        [Tooltip("Tolerância lateral (m) antes de penalizar. Definida pela mina escolhida.")]
        public float offTrackToleranceM = 4f;

        public float penaltyPerSecondPerMeter = 2f;

        public OffTrackMonitor Monitor { get; private set; }
        public TrackSample LastSample { get; private set; }

        private void Awake()
        {
            Monitor = new OffTrackMonitor(offTrackToleranceM, penaltyPerSecondPerMeter);
        }

        private void Update()
        {
            if (truck == null || path == null)
            {
                return;
            }

            LastSample = path.Route.Sample(truck.transform.position);
            Monitor.Tick(Time.deltaTime, LastSample.LateralDistance);
        }

        public bool IsOffTrack => Monitor != null && Monitor.IsOffTrack;
        public float Progress => LastSample.Progress;
        public float TotalPenalty => Monitor != null ? Monitor.TotalPenalty : 0f;
    }
}
