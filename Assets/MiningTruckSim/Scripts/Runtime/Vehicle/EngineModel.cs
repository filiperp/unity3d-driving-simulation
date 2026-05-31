using System;

namespace MiningTruckSim.Vehicle
{
    /// <summary>
    /// Modelo simplificado do motor (lógica pura, testável). Liga/desliga (ignição),
    /// aproxima o RPM de um alvo função do acelerador e simula aquecimento, que cresce
    /// com acelerador e excesso de carga (insumo para alertas de temperatura na S5).
    /// </summary>
    public sealed class EngineModel
    {
        public float IdleRpm = 700f;
        public float MaxRpm = 2200f;
        public float WarmTempC = 70f;
        public float MaxTempC = 120f;
        public float AmbientTempC = 25f;

        public bool Running { get; private set; }
        public float Rpm { get; private set; }
        public float TempC { get; private set; } = 25f;

        public void Start() => Running = true;
        public void Stop() => Running = false;
        public void Toggle() => Running = !Running;

        /// <param name="throttle01">Acelerador em [0,1].</param>
        /// <param name="loadRatio">Carga atual / capacidade (1 = cheio).</param>
        public void Step(float dt, float throttle01, float loadRatio)
        {
            if (dt <= 0f)
            {
                return;
            }

            throttle01 = Clamp01(throttle01);

            float targetRpm = Running ? IdleRpm + throttle01 * (MaxRpm - IdleRpm) : 0f;
            float rpmBlend = 1f - (float)Math.Exp(-dt * 3f);
            Rpm += (targetRpm - Rpm) * rpmBlend;
            if (!Running && Rpm < 1f)
            {
                Rpm = 0f;
            }

            float overload = Math.Max(0f, loadRatio - 1f);
            float targetTemp = Running
                ? WarmTempC + throttle01 * 25f + overload * 30f
                : AmbientTempC;
            float tempBlend = 1f - (float)Math.Exp(-dt * 0.05f);
            TempC += (targetTemp - TempC) * tempBlend;
            if (TempC > MaxTempC)
            {
                TempC = MaxTempC;
            }
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
