using System;

namespace MiningTruckSim.Alerts
{
    /// <summary>
    /// Um alerta ativo e o progresso do seu conserto (critério 9). A condição de conserto
    /// é avaliada por quem o conhece (o mundo): o jogador precisa sustentá-la pelo tempo
    /// do procedimento. Acumula também a penalidade enquanto não resolvido. Lógica pura.
    /// </summary>
    public sealed class ActiveAlert
    {
        public AlertType Type { get; }
        public AlertCatalog.Definition Definition { get; }

        public bool Resolved { get; private set; }
        public float HoldProgressSeconds { get; private set; }
        public float AccumulatedPenalty { get; private set; }

        public ActiveAlert(AlertType type)
        {
            Type = type;
            Definition = AlertCatalog.Get(type);
        }

        public string Title => Definition.Title;
        public string Instruction => Definition.Repair.Instruction;
        public float RequiredHold => Definition.Repair.HoldSeconds;

        /// <summary>Progresso do conserto em [0,1].</summary>
        public float RepairProgress01 =>
            RequiredHold > 0f ? Math.Min(1f, HoldProgressSeconds / RequiredHold) : 1f;

        /// <summary>
        /// Avança o alerta. <paramref name="repairConditionMet"/> indica se a condição do
        /// procedimento está sendo cumprida neste instante (ex.: RPM baixo, dentro do
        /// trilho, parado segurando [R]). Retorna a penalidade aplicada neste tick.
        /// </summary>
        public float Tick(float dt, bool repairConditionMet)
        {
            if (Resolved || dt <= 0f)
            {
                return 0f;
            }

            if (repairConditionMet)
            {
                HoldProgressSeconds += dt;
                if (HoldProgressSeconds >= RequiredHold)
                {
                    Resolved = true;
                }
            }
            else
            {
                // Deixar de cumprir a condição zera o progresso (precisa sustentar).
                HoldProgressSeconds = 0f;
            }

            if (Resolved)
            {
                return 0f;
            }

            float penalty = Definition.PenaltyPerSecond * dt;
            AccumulatedPenalty += penalty;
            return penalty;
        }
    }
}
