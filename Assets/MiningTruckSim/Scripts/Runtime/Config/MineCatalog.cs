using System;
using System.Collections.Generic;

namespace MiningTruckSim.Config
{
    /// <summary>
    /// Catálogo das 2 minas do POC (critério 8): uma fácil e uma difícil. Os valores
    /// espelham o backend (`app/mines.py`) para que cliente e servidor concordem sobre a
    /// configuração da operação. Mantido como fonte estática e pura no cliente.
    /// </summary>
    public static class MineCatalog
    {
        public static readonly MineConfig Easy = new MineConfig
        {
            Id = "easy",
            Name = "Mina Vale Verde (Fácil)",
            IsHard = false,
            RouteLengthM = 600f,
            CurveCount = 3,
            MaxGradePct = 6f,
            OffTrackToleranceM = 4f,
            AlertsPerMinute = 0.15f,
            RecommendedCycles = 3
        };

        public static readonly MineConfig Hard = new MineConfig
        {
            Id = "hard",
            Name = "Mina Serra Negra (Difícil)",
            IsHard = true,
            RouteLengthM = 1400f,
            CurveCount = 8,
            MaxGradePct = 12f,
            OffTrackToleranceM = 2f,
            AlertsPerMinute = 0.45f,
            RecommendedCycles = 5
        };

        public static IReadOnlyList<MineConfig> All => new[] { Easy, Hard };

        public static MineConfig ById(string id)
        {
            if (string.Equals(id, Hard.Id, StringComparison.OrdinalIgnoreCase))
            {
                return Hard;
            }

            return Easy;
        }
    }
}
