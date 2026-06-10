using System.Collections.Generic;
using MiningTruckSim.Scoring;

namespace MiningTruckSim.Config
{
    /// <summary>
    /// Controla o loop de N ciclos de uma operação e agrega a pontuação (critério 8).
    /// Recebe o resultado de cada ciclo (<see cref="CycleScore"/>), avança o índice e
    /// indica quando a operação terminou. Lógica pura (testável em EditMode); a soma dos
    /// scores é o total enviado ao backend ao finalizar (critério 7).
    /// </summary>
    public sealed class OperationProgress
    {
        private readonly List<CycleScore> _results = new List<CycleScore>();

        public OperationConfig Config { get; }
        public int CurrentCycleIndex { get; private set; }

        public OperationProgress(OperationConfig config)
        {
            Config = config.Validated();
        }

        public int TotalCycles => Config.Cycles;
        public IReadOnlyList<CycleScore> Results => _results;
        public bool IsComplete => CurrentCycleIndex >= TotalCycles;
        public int CompletedCycles => _results.Count;

        /// <summary>Soma das pontuações finais de todos os ciclos concluídos.</summary>
        public float TotalScore
        {
            get
            {
                float sum = 0f;
                foreach (CycleScore r in _results)
                {
                    sum += r.Final;
                }

                return sum;
            }
        }

        /// <summary>Média de tempo na faixa perfeita ao longo dos ciclos concluídos.</summary>
        public float AverageBandRatio
        {
            get
            {
                if (_results.Count == 0)
                {
                    return 0f;
                }

                float sum = 0f;
                foreach (CycleScore r in _results)
                {
                    sum += r.BandRatio;
                }

                return sum / _results.Count;
            }
        }

        /// <summary>
        /// Registra o resultado do ciclo atual e avança para o próximo. Ignora chamadas
        /// após a operação já estar completa.
        /// </summary>
        public void CompleteCycle(CycleScore result)
        {
            if (IsComplete)
            {
                return;
            }

            _results.Add(result);
            CurrentCycleIndex++;
        }
    }
}
