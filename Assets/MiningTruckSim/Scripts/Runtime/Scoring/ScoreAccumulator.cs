namespace MiningTruckSim.Scoring
{
    /// <summary>
    /// Acumula pontuação ao longo de um ciclo: pontos proporcionais ao tempo em que o
    /// caminhão é mantido na faixa de operação perfeita (critério 6), descontando
    /// penalidades (ex.: saída do trilho — critério 3, alertas não resolvidos — critério 9).
    /// Lógica pura, avançada por chamadas de <see cref="Tick"/> com o delta de tempo.
    /// </summary>
    public sealed class ScoreAccumulator
    {
        private readonly OperationBand _band;
        private readonly float _pointsPerSecondInBand;

        public float Score { get; private set; }
        public float TimeInBandSeconds { get; private set; }
        public float TotalTimeSeconds { get; private set; }
        public int PenaltyCount { get; private set; }

        public ScoreAccumulator(OperationBand band, float pointsPerSecondInBand = 10f)
        {
            _band = band;
            _pointsPerSecondInBand = pointsPerSecondInBand;
        }

        /// <summary>
        /// Avança o acumulador. <paramref name="deltaSeconds"/> é o tempo decorrido e
        /// <paramref name="telemetry"/> o estado atual do caminhão.
        /// </summary>
        public void Tick(float deltaSeconds, TruckTelemetry telemetry)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            TotalTimeSeconds += deltaSeconds;
            float quality = _band.Quality(telemetry);
            Score += quality * _pointsPerSecondInBand * deltaSeconds;

            if (_band.IsInBand(telemetry))
            {
                TimeInBandSeconds += deltaSeconds;
            }
        }

        /// <summary>Aplica uma penalidade pontual (ex.: saída do trilho, excesso de carga).</summary>
        public void ApplyPenalty(float points)
        {
            if (points <= 0f)
            {
                return;
            }

            Score -= points;
            if (Score < 0f)
            {
                Score = 0f;
            }

            PenaltyCount++;
        }

        /// <summary>Fração do ciclo mantida na faixa perfeita, em [0,1].</summary>
        public float BandRatio => TotalTimeSeconds > 0f ? TimeInBandSeconds / TotalTimeSeconds : 0f;
    }
}
