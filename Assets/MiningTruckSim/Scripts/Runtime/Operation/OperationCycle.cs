using System;

namespace MiningTruckSim.Operation
{
    /// <summary>
    /// Máquina de estados do ciclo de operação (lógica pura, testável em EditMode).
    /// Recebe eventos do mundo (entrou na área, estacionou, báscula levantada) e o
    /// progresso de carga/descarga, e avança entre as fases do <see cref="CyclePhase"/>.
    ///
    /// Sequência: Idle → ApproachingLoad → Loading → Loaded → ApproachingUnload →
    /// Unloading → Done. Mantida independente de Unity para ser testada e reaproveitada
    /// pelo loop de N ciclos da Sprint 6.
    /// </summary>
    public sealed class OperationCycle
    {
        /// <summary>Quanto a caçamba enche por segundo enquanto carrega (fração de 0..1).</summary>
        public float LoadRatePerSec = 0.2f;

        /// <summary>Quanto a caçamba esvazia por segundo enquanto descarrega.</summary>
        public float UnloadRatePerSec = 0.5f;

        public CyclePhase Phase { get; private set; } = CyclePhase.Idle;

        /// <summary>Carga atual da caçamba em [0,1] (1 = cheia).</summary>
        public float LoadFill { get; private set; }

        /// <summary>Disparado a cada transição de fase (fase anterior, nova fase).</summary>
        public event Action<CyclePhase, CyclePhase> PhaseChanged;

        public bool IsLoading => Phase == CyclePhase.Loading;
        public bool IsUnloading => Phase == CyclePhase.Unloading;
        public bool IsComplete => Phase == CyclePhase.Done;

        /// <summary>O caminhão entrou na área de loading.</summary>
        public void EnterLoadZone()
        {
            if (Phase == CyclePhase.Idle)
            {
                SetPhase(CyclePhase.ApproachingLoad);
            }
        }

        /// <summary>O caminhão saiu da área de loading sem estar carregando.</summary>
        public void ExitLoadZone()
        {
            if (Phase == CyclePhase.ApproachingLoad)
            {
                SetPhase(CyclePhase.Idle);
            }
        }

        /// <summary>O caminhão entrou na área de unload (já carregado).</summary>
        public void EnterUnloadZone()
        {
            if (Phase == CyclePhase.Loaded)
            {
                SetPhase(CyclePhase.ApproachingUnload);
            }
        }

        public void ExitUnloadZone()
        {
            if (Phase == CyclePhase.ApproachingUnload)
            {
                SetPhase(CyclePhase.Loaded);
            }
        }

        /// <summary>
        /// Informa se o caminhão está estacionado/alinhado no ponto demarcado da fase atual.
        /// Em ApproachingLoad inicia o carregamento; em ApproachingUnload habilita o unload.
        /// </summary>
        public void SetParkedAtPoint(bool parked)
        {
            switch (Phase)
            {
                case CyclePhase.ApproachingLoad when parked:
                    SetPhase(CyclePhase.Loading);
                    break;
                case CyclePhase.Loading when !parked:
                    // Saiu do ponto durante o carregamento: volta a aproximar.
                    SetPhase(CyclePhase.ApproachingLoad);
                    break;
            }
        }

        /// <summary>
        /// Avança o ciclo no tempo. <paramref name="bedRaised"/> indica se a báscula está
        /// levantada (necessário para descarregar no ponto de unload).
        /// </summary>
        public void Tick(float dt, bool bedRaised)
        {
            if (dt <= 0f)
            {
                return;
            }

            // Levantar a báscula no ponto de unload inicia a descarga já neste tick.
            if (Phase == CyclePhase.ApproachingUnload && bedRaised)
            {
                SetPhase(CyclePhase.Unloading);
            }

            switch (Phase)
            {
                case CyclePhase.Loading:
                    LoadFill = Math.Min(1f, LoadFill + LoadRatePerSec * dt);
                    if (LoadFill >= 1f)
                    {
                        SetPhase(CyclePhase.Loaded);
                    }

                    break;

                case CyclePhase.Unloading:
                    if (bedRaised)
                    {
                        LoadFill = Math.Max(0f, LoadFill - UnloadRatePerSec * dt);
                        if (LoadFill <= 0f)
                        {
                            SetPhase(CyclePhase.Done);
                        }
                    }

                    break;
            }
        }

        private void SetPhase(CyclePhase next)
        {
            if (next == Phase)
            {
                return;
            }

            CyclePhase prev = Phase;
            Phase = next;
            PhaseChanged?.Invoke(prev, next);
        }
    }
}
