using System;

namespace MiningTruckSim.Scoring
{
    /// <summary>
    /// Amostra instantânea do estado do caminhão usada por scoring, alertas e HUD.
    /// Mantida como tipo puro para ser facilmente testável e serializável.
    /// </summary>
    [Serializable]
    public struct TruckTelemetry
    {
        public float Rpm;
        public float SpeedKmh;
        public float EngineTempC;
        /// <summary>Carga atual / capacidade nominal (1.0 = cheio; >1.0 = excesso).</summary>
        public float LoadRatio;

        public TruckTelemetry(float rpm, float speedKmh, float engineTempC, float loadRatio)
        {
            Rpm = rpm;
            SpeedKmh = speedKmh;
            EngineTempC = engineTempC;
            LoadRatio = loadRatio;
        }
    }
}
