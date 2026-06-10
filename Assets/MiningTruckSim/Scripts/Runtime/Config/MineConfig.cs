using System;

namespace MiningTruckSim.Config
{
    /// <summary>
    /// Parâmetros de uma mina (critério 8). Espelha o catálogo do backend (`/mines`):
    /// distância da rota, curvas, inclinação, tolerância lateral do trilho e frequência
    /// de alertas. Estes valores configuram a dificuldade da operação no cliente.
    /// Tipo puro/serializável.
    /// </summary>
    [Serializable]
    public struct MineConfig
    {
        /// <summary>Identificador estável usado pelo backend ("easy" / "hard").</summary>
        public string Id;
        public string Name;
        public bool IsHard;

        public float RouteLengthM;
        public int CurveCount;
        public float MaxGradePct;

        /// <summary>Tolerância lateral (m) antes de penalizar saída do trilho (critério 3).</summary>
        public float OffTrackToleranceM;

        /// <summary>Frequência média de alertas por minuto (critério 9).</summary>
        public float AlertsPerMinute;

        /// <summary>Quantidade de ciclos recomendada por padrão.</summary>
        public int RecommendedCycles;
    }
}
