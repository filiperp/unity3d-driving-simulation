using MiningTruckSim.Config;
using MiningTruckSim.Scoring;
using MiningTruckSim.UI;
using MiningTruckSim.Vehicle;
using UnityEngine;

namespace MiningTruckSim.Operation
{
    /// <summary>
    /// Conduz o loop de N ciclos de uma operação (critério 8): a cada ciclo concluído,
    /// registra o <see cref="CycleScore"/> em <see cref="OperationProgress"/>, e então
    /// reinicia o ciclo (reposicionando o caminhão) para o próximo, ou exibe o resumo
    /// final da operação quando todos os ciclos terminam. A pontuação total agregada é
    /// o que será enviado ao backend na Sprint 7 (critério 7).
    /// </summary>
    public sealed class OperationRunner : MonoBehaviour
    {
        public CycleDirector director;
        public PerformanceScorer scorer;
        public TruckController truck;
        public OperationSummaryScreen summaryScreen;

        public OperationProgress Progress { get; private set; }

        private Vector3 _spawnPos;
        private Quaternion _spawnRot;
        private bool _hasSpawnPose;

        private void Start()
        {
            Progress = new OperationProgress(OperationContext.Config);

            if (truck != null)
            {
                _spawnPos = truck.transform.position;
                _spawnRot = truck.transform.rotation;
                _hasSpawnPose = true;
            }

            if (director != null)
            {
                director.Cycle.PhaseChanged += OnPhaseChanged;
            }
        }

        private void OnDestroy()
        {
            if (director != null)
            {
                director.Cycle.PhaseChanged -= OnPhaseChanged;
            }
        }

        private void OnPhaseChanged(CyclePhase previous, CyclePhase next)
        {
            if (next != CyclePhase.Done)
            {
                return;
            }

            int index = Progress.CurrentCycleIndex;
            float loadTonnes = director != null ? director.fullLoadTonnes : 0f;
            CycleScore result = scorer != null
                ? scorer.BuildResult(index, loadTonnes)
                : new CycleScore { CycleIndex = index };

            Progress.CompleteCycle(result);

            if (Progress.IsComplete)
            {
                if (summaryScreen != null)
                {
                    summaryScreen.Show(Progress);
                }
            }
            else
            {
                // Pequeno atraso para o jogador ver a tela de resultado do ciclo.
                Invoke(nameof(StartNextCycle), 0.1f);
            }
        }

        private void StartNextCycle()
        {
            RepositionTruck();
            director?.Cycle.Reset();
            scorer?.ResetForNewCycle();
        }

        private void RepositionTruck()
        {
            if (!_hasSpawnPose || truck == null)
            {
                return;
            }

            var rb = truck.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            truck.transform.SetPositionAndRotation(_spawnPos, _spawnRot);
            truck.currentLoadTonnes = 0f;
        }
    }
}
