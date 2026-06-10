using System;

namespace MiningTruckSim.Scoring
{
    /// <summary>
    /// Resultado consolidado de um ciclo de operação (critério 6). Agrega o score bruto
    /// por tempo na faixa perfeita, as penalidades (trilho, alertas) e métricas de
    /// desempenho, produzindo a pontuação final e uma classificação. É um tipo puro,
    /// serializável e enviado ao backend ao fim do ciclo (critério 7).
    /// </summary>
    [Serializable]
    public struct CycleScore
    {
        public int CycleIndex;

        /// <summary>Pontos acumulados por qualidade/tempo na faixa de operação.</summary>
        public float BaseScore;

        /// <summary>Total de penalidades descontadas (saída do trilho, alertas etc.).</summary>
        public float Penalties;

        /// <summary>Fração do ciclo mantida na faixa perfeita, em [0,1].</summary>
        public float BandRatio;

        public int OffTrackEvents;
        public int AlertsHandled;
        public float LoadTonnes;

        /// <summary>Pontuação final do ciclo, nunca negativa.</summary>
        public float Final => Math.Max(0f, BaseScore - Penalties);

        /// <summary>Classificação simples por qualidade da operação (critério 6).</summary>
        public string Rating
        {
            get
            {
                float r = BandRatio;
                if (OffTrackEvents == 0 && r >= 0.9f)
                {
                    return "S";
                }

                if (r >= 0.75f)
                {
                    return "A";
                }

                if (r >= 0.5f)
                {
                    return "B";
                }

                return r >= 0.25f ? "C" : "D";
            }
        }
    }
}
