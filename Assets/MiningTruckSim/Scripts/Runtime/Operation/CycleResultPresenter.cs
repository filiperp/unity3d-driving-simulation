using MiningTruckSim.Scoring;
using MiningTruckSim.UI;
using UnityEngine;

namespace MiningTruckSim.Operation
{
    /// <summary>
    /// Liga o fim do ciclo (transição para <see cref="CyclePhase.Done"/>) ao scoring e à
    /// tela de resultado (critério 6). Ao concluir, monta o <see cref="CycleScore"/> a
    /// partir do <see cref="PerformanceScorer"/> e o apresenta na <see cref="CycleResultScreen"/>.
    /// </summary>
    public sealed class CycleResultPresenter : MonoBehaviour
    {
        public CycleDirector director;
        public PerformanceScorer scorer;
        public CycleResultScreen resultScreen;

        [Tooltip("Índice do ciclo atual (0-based). Será controlado pelo loop de N ciclos na S6.")]
        public int cycleIndex;

        public CycleScore LastResult { get; private set; }
        public bool HasResult { get; private set; }

        private void OnEnable()
        {
            if (director != null)
            {
                director.Cycle.PhaseChanged += OnPhaseChanged;
            }
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.Cycle.PhaseChanged -= OnPhaseChanged;
            }
        }

        private void OnPhaseChanged(CyclePhase previous, CyclePhase next)
        {
            if (next != CyclePhase.Done || scorer == null)
            {
                return;
            }

            float loadTonnes = director != null ? director.fullLoadTonnes : 0f;
            LastResult = scorer.BuildResult(cycleIndex, loadTonnes);
            HasResult = true;

            if (resultScreen != null)
            {
                resultScreen.Show(LastResult);
            }
        }
    }
}
