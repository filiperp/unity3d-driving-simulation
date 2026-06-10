using System.Collections.Generic;

namespace MiningTruckSim.Alerts
{
    /// <summary>
    /// Catálogo estático dos alertas (critério 9): para cada <see cref="AlertType"/>,
    /// o procedimento de conserto distinto, o título exibido e a penalidade por segundo
    /// enquanto o alerta fica ativo sem ser resolvido.
    /// </summary>
    public static class AlertCatalog
    {
        public readonly struct Definition
        {
            public readonly string Title;
            public readonly RepairProcedure Repair;
            public readonly float PenaltyPerSecond;

            public Definition(string title, RepairProcedure repair, float penaltyPerSecond)
            {
                Title = title;
                Repair = repair;
                PenaltyPerSecond = penaltyPerSecond;
            }
        }

        private static readonly Dictionary<AlertType, Definition> Map = new()
        {
            [AlertType.HighOilPressure] = new Definition(
                "Pressão de óleo ALTA",
                new RepairProcedure(RepairKind.HoldCondition, 3f,
                    "Reduza o RPM (solte o acelerador) e mantenha até normalizar."),
                penaltyPerSecond: 3f),

            [AlertType.CloggedFilter] = new Definition(
                "Filtro de ar ENTUPIDO",
                new RepairProcedure(RepairKind.ManualHoldAction, 4f,
                    "Pare o caminhão e segure [R] para trocar o filtro."),
                penaltyPerSecond: 2f),

            [AlertType.OffTrack] = new Definition(
                "Fora do TRILHO",
                new RepairProcedure(RepairKind.HoldCondition, 1.5f,
                    "Retorne ao trilho esperado e siga a rota."),
                penaltyPerSecond: 4f),

            [AlertType.Overload] = new Definition(
                "Excesso de CARGA",
                new RepairProcedure(RepairKind.HoldCondition, 2f,
                    "Alivie a carga (descarregue) até a faixa nominal."),
                penaltyPerSecond: 5f),
        };

        public static Definition Get(AlertType type) => Map[type];

        public static IEnumerable<AlertType> AllTypes => Map.Keys;
    }
}
