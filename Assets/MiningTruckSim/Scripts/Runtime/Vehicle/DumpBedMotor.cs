using System;

namespace MiningTruckSim.Vehicle
{
    /// <summary>
    /// Cinemática da caçamba basculante (lógica pura, testável). Move o ângulo entre
    /// 0 (abaixada) e <see cref="MaxAngleDeg"/> (levantada) a uma velocidade constante.
    /// Usado tanto pelo componente visual quanto pela lógica de unload (S2).
    /// </summary>
    public sealed class DumpBedMotor
    {
        public float MaxAngleDeg = 50f;
        public float SpeedDegPerSec = 25f;

        public bool RaiseRequested { get; set; }
        public float AngleDeg { get; private set; }

        public bool IsFullyRaised => AngleDeg >= MaxAngleDeg - 0.001f;
        public bool IsFullyLowered => AngleDeg <= 0.001f;

        public void Toggle() => RaiseRequested = !RaiseRequested;

        public void Step(float dt)
        {
            if (dt <= 0f)
            {
                return;
            }

            float target = RaiseRequested ? MaxAngleDeg : 0f;
            float step = SpeedDegPerSec * dt;

            if (AngleDeg < target)
            {
                AngleDeg = Math.Min(target, AngleDeg + step);
            }
            else if (AngleDeg > target)
            {
                AngleDeg = Math.Max(target, AngleDeg - step);
            }
        }
    }
}
