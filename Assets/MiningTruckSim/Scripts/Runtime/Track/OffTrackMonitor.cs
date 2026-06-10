namespace MiningTruckSim.Track
{
    /// <summary>
    /// Monitora a aderência ao trilho esperado e penaliza quando o caminhão sai dele
    /// (critério 3). Mantém a noção de "fora do trilho" com base na tolerância lateral
    /// da mina (vinda do backend, critério 8) e acumula penalidade proporcional ao
    /// tempo e à distância fora. Lógica pura (testável em EditMode).
    /// </summary>
    public sealed class OffTrackMonitor
    {
        /// <summary>Distância lateral (m) a partir da qual o caminhão é considerado fora do trilho.</summary>
        public float ToleranceM { get; }

        /// <summary>Penalidade por segundo enquanto fora, por metro de excesso além da tolerância.</summary>
        public float PenaltyPerSecondPerMeter { get; }

        public bool IsOffTrack { get; private set; }
        public float TotalPenalty { get; private set; }
        public float TimeOffTrackSeconds { get; private set; }

        /// <summary>Quantas vezes o caminhão saiu do trilho (transições dentro→fora).</summary>
        public int OffTrackEvents { get; private set; }

        public OffTrackMonitor(float toleranceM, float penaltyPerSecondPerMeter = 2f)
        {
            ToleranceM = toleranceM;
            PenaltyPerSecondPerMeter = penaltyPerSecondPerMeter;
        }

        /// <summary>
        /// Avança o monitor. <paramref name="lateralDistance"/> é a distância atual ao
        /// trilho (ver <see cref="RouteTrack.Sample"/>). Retorna a penalidade aplicada
        /// neste tick (0 se dentro do trilho).
        /// </summary>
        public float Tick(float dt, float lateralDistance)
        {
            if (dt <= 0f)
            {
                return 0f;
            }

            float excess = lateralDistance - ToleranceM;
            if (excess <= 0f)
            {
                IsOffTrack = false;
                return 0f;
            }

            if (!IsOffTrack)
            {
                IsOffTrack = true;
                OffTrackEvents++;
            }

            TimeOffTrackSeconds += dt;
            float penalty = excess * PenaltyPerSecondPerMeter * dt;
            TotalPenalty += penalty;
            return penalty;
        }
    }
}
