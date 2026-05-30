using System;

namespace MiningTruckSim.Vehicle
{
    /// <summary>Modos de câmbio estilo automático: P / R / N / D.</summary>
    public enum GearMode
    {
        Park,
        Reverse,
        Neutral,
        Drive
    }

    /// <summary>
    /// Câmbio automático simples do caminhão (lógica pura, testável em EditMode).
    /// Em <see cref="GearMode.Drive"/> seleciona automaticamente a marcha de avanço
    /// conforme a velocidade. Define o sentido do torque entregue às rodas.
    /// </summary>
    public sealed class Gearbox
    {
        private static readonly GearMode[] Order =
        {
            GearMode.Park, GearMode.Reverse, GearMode.Neutral, GearMode.Drive
        };

        // Velocidade (km/h) em que sobe da marcha i para i+1.
        private readonly float[] _upshiftSpeedsKmh;

        public GearMode Mode { get; private set; } = GearMode.Park;
        public int CurrentForwardGear { get; private set; } = 1; // base 1
        public int ForwardGearCount { get; }

        public Gearbox(float[] upshiftSpeedsKmh = null)
        {
            _upshiftSpeedsKmh = upshiftSpeedsKmh ?? new[] { 12f, 24f, 36f };
            ForwardGearCount = _upshiftSpeedsKmh.Length + 1;
        }

        /// <summary>+1 avança, -1 ré, 0 sem entrega de torque (P/N).</summary>
        public float DirectionSign => Mode switch
        {
            GearMode.Drive => 1f,
            GearMode.Reverse => -1f,
            _ => 0f
        };

        public bool CanDeliverTorque => Mode == GearMode.Drive || Mode == GearMode.Reverse;
        public bool IsParked => Mode == GearMode.Park;

        public void SetMode(GearMode mode)
        {
            Mode = mode;
            if (mode != GearMode.Drive)
            {
                CurrentForwardGear = 1;
            }
        }

        public void ShiftUp() => ShiftBy(+1);
        public void ShiftDown() => ShiftBy(-1);

        private void ShiftBy(int delta)
        {
            int idx = Array.IndexOf(Order, Mode) + delta;
            if (idx < 0)
            {
                idx = 0;
            }
            else if (idx >= Order.Length)
            {
                idx = Order.Length - 1;
            }

            SetMode(Order[idx]);
        }

        /// <summary>Seleciona a marcha de avanço com base na velocidade (só em Drive).</summary>
        public void AutoShift(float speedKmh)
        {
            if (Mode != GearMode.Drive)
            {
                CurrentForwardGear = 1;
                return;
            }

            int gear = 1;
            for (int i = 0; i < _upshiftSpeedsKmh.Length; i++)
            {
                if (speedKmh >= _upshiftSpeedsKmh[i])
                {
                    gear = i + 2;
                }
            }

            CurrentForwardGear = gear;
        }

        /// <summary>Rótulo para o HUD: P, R, N, D1..Dn.</summary>
        public string DisplayGear => Mode switch
        {
            GearMode.Park => "P",
            GearMode.Reverse => "R",
            GearMode.Neutral => "N",
            GearMode.Drive => "D" + CurrentForwardGear,
            _ => "?"
        };
    }
}
