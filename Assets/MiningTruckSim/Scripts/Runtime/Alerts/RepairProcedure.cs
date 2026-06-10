namespace MiningTruckSim.Alerts
{
    /// <summary>
    /// Como um alerta é resolvido (critério 9): cada tipo tem um procedimento distinto.
    /// Alguns são condições contínuas do mundo (reduzir RPM, voltar ao trilho, aliviar
    /// carga) e outros exigem uma ação explícita do jogador segurando um botão por um
    /// tempo (troca de filtro). Tipo puro, testável.
    /// </summary>
    public enum RepairKind
    {
        /// <summary>Manter uma condição do mundo verdadeira por <see cref="RepairProcedure.HoldSeconds"/>.</summary>
        HoldCondition,

        /// <summary>Segurar a tecla de conserto por <see cref="RepairProcedure.HoldSeconds"/> com o caminhão parado.</summary>
        ManualHoldAction
    }

    /// <summary>Descrição do procedimento de conserto de um alerta.</summary>
    public readonly struct RepairProcedure
    {
        public readonly RepairKind Kind;

        /// <summary>Tempo (s) que a condição/ação precisa ser sustentada para resolver.</summary>
        public readonly float HoldSeconds;

        /// <summary>Instrução exibida ao jogador.</summary>
        public readonly string Instruction;

        public RepairProcedure(RepairKind kind, float holdSeconds, string instruction)
        {
            Kind = kind;
            HoldSeconds = holdSeconds;
            Instruction = instruction;
        }
    }
}
