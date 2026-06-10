using System;

namespace MiningTruckSim.Config
{
    /// <summary>
    /// Configuração escolhida pelo jogador para uma operação (critério 8): qual mina e
    /// quantos ciclos (N, configurável). Tipo puro, validado e passado do menu para a cena.
    /// </summary>
    [Serializable]
    public struct OperationConfig
    {
        public MineConfig Mine;
        public int Cycles;

        public OperationConfig(MineConfig mine, int cycles)
        {
            Mine = mine;
            Cycles = cycles;
        }

        /// <summary>Garante pelo menos 1 ciclo (e um teto sensato para o POC).</summary>
        public OperationConfig Validated()
        {
            int c = Math.Clamp(Cycles, 1, 20);
            return new OperationConfig(Mine, c);
        }

        public static OperationConfig Default =>
            new OperationConfig(MineCatalog.Easy, MineCatalog.Easy.RecommendedCycles);
    }
}
