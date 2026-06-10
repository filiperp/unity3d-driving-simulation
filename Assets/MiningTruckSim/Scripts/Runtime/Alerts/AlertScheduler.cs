using System;
using System.Collections.Generic;

namespace MiningTruckSim.Alerts
{
    /// <summary>
    /// Agenda alertas aleatórios ao longo do tempo (critério 9). A frequência média
    /// (alertas por minuto) vem da mina escolhida (critério 8). É determinístico por
    /// <see cref="System.Random"/> com seed, o que o torna testável: dado um seed, a
    /// sequência de alertas e intervalos é reproduzível.
    ///
    /// Dispara no máximo um alerta de cada tipo por vez e respeita o conjunto de tipos
    /// "elegíveis" no momento (ex.: só emitir Overload quando há carga). Lógica pura.
    /// </summary>
    public sealed class AlertScheduler
    {
        private readonly Random _rng;
        private readonly float _alertsPerMinute;
        private float _timeToNext;

        public AlertScheduler(float alertsPerMinute, int seed = 0)
        {
            _alertsPerMinute = Math.Max(0f, alertsPerMinute);
            _rng = new Random(seed);
            _timeToNext = SampleInterval();
        }

        /// <summary>Segundos até o próximo disparo agendado.</summary>
        public float TimeToNext => _timeToNext;

        /// <summary>
        /// Avança o tempo. Quando o cronômetro vence, escolhe aleatoriamente um tipo
        /// dentre <paramref name="eligible"/> que ainda não esteja ativo e o retorna;
        /// senão retorna null. Reagenda sempre que vence.
        /// </summary>
        public AlertType? Tick(float dt, IReadOnlyCollection<AlertType> eligible,
            ISet<AlertType> active)
        {
            if (_alertsPerMinute <= 0f || dt <= 0f)
            {
                return null;
            }

            _timeToNext -= dt;
            if (_timeToNext > 0f)
            {
                return null;
            }

            _timeToNext += SampleInterval();

            // Filtra os tipos elegíveis que não estão ativos.
            var candidates = new List<AlertType>();
            foreach (AlertType t in eligible)
            {
                if (!active.Contains(t))
                {
                    candidates.Add(t);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates[_rng.Next(candidates.Count)];
        }

        /// <summary>
        /// Intervalo até o próximo alerta, em segundos. Média = 60/alertsPerMinute, com
        /// distribuição exponencial (processo de Poisson) para parecer aleatório natural.
        /// </summary>
        private float SampleInterval()
        {
            float meanSeconds = 60f / _alertsPerMinute;
            // u em (0,1]; -ln(u) ~ exponencial(1).
            double u = 1.0 - _rng.NextDouble();
            return (float)(-Math.Log(u) * meanSeconds);
        }
    }
}
