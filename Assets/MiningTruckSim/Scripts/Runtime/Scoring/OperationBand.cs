using System;

namespace MiningTruckSim.Scoring
{
    /// <summary>
    /// Define a "faixa de operação perfeita" do caminhão e avalia, a cada instante,
    /// se a operação está dentro dela. É lógica pura (sem dependência de Unity), o que
    /// permite testá-la em EditMode e reutilizá-la no cálculo de pontuação (critério 6).
    /// </summary>
    [Serializable]
    public struct OperationBand
    {
        public float MinRpm;
        public float MaxRpm;
        public float MinSpeedKmh;
        public float MaxSpeedKmh;
        public float MaxEngineTempC;
        /// <summary>Razão de carga máxima aceitável (1.0 = capacidade nominal).</summary>
        public float MaxLoadRatio;

        /// <summary>Faixa padrão de referência para um caminhão de mineração no POC.</summary>
        public static OperationBand Default => new OperationBand
        {
            MinRpm = 1200f,
            MaxRpm = 1900f,
            MinSpeedKmh = 10f,
            MaxSpeedKmh = 45f,
            MaxEngineTempC = 105f,
            MaxLoadRatio = 1.0f
        };

        /// <summary>
        /// Retorna true quando todos os parâmetros estão dentro da faixa ideal.
        /// </summary>
        public bool IsInBand(TruckTelemetry t)
        {
            return t.Rpm >= MinRpm && t.Rpm <= MaxRpm
                && t.SpeedKmh >= MinSpeedKmh && t.SpeedKmh <= MaxSpeedKmh
                && t.EngineTempC <= MaxEngineTempC
                && t.LoadRatio <= MaxLoadRatio;
        }

        /// <summary>
        /// Qualidade da operação em [0,1]: 1 dentro da faixa, decaindo conforme se afasta.
        /// Útil para um score contínuo em vez de binário.
        /// </summary>
        public float Quality(TruckTelemetry t)
        {
            float rpm = RangeQuality(t.Rpm, MinRpm, MaxRpm);
            float speed = RangeQuality(t.SpeedKmh, MinSpeedKmh, MaxSpeedKmh);
            float temp = t.EngineTempC <= MaxEngineTempC
                ? 1f
                : Clamp01(1f - (t.EngineTempC - MaxEngineTempC) / 30f);
            float load = t.LoadRatio <= MaxLoadRatio
                ? 1f
                : Clamp01(1f - (t.LoadRatio - MaxLoadRatio) / 0.5f);

            // O elo mais fraco define a qualidade geral da operação.
            return Math.Min(Math.Min(rpm, speed), Math.Min(temp, load));
        }

        private static float RangeQuality(float value, float min, float max)
        {
            if (value >= min && value <= max)
            {
                return 1f;
            }

            float span = Math.Max(max - min, 0.0001f);
            float distance = value < min ? (min - value) : (value - max);
            // Zera quando ultrapassa meia-faixa para fora dos limites.
            return Clamp01(1f - distance / (span * 0.5f));
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
