using UnityEngine;

namespace MiningTruckSim.Vehicle
{
    /// <summary>
    /// Leitura dos controles do caminhão via Input Manager (legado), que funciona sem
    /// configuração extra do projeto. Mapeamento (critério 5):
    /// W/S = acelerador/freio, A/D = direção, mouse = olhar na cabine,
    /// I = ignição, Q/E = trocar marcha, Espaço = freio de mão,
    /// B = báscula, L = faróis, H = buzina.
    /// </summary>
    public sealed class TruckInputReader
    {
        public float Throttle;   // 0..1
        public float Brake;      // 0..1
        public float Steer;      // -1..1
        public Vector2 Look;

        public bool IgnitionPressed;
        public bool GearUpPressed;
        public bool GearDownPressed;
        public bool HandbrakePressed;
        public bool DumpPressed;
        public bool LightsPressed;
        public bool HornHeld;

        public void Sample()
        {
            float v = Input.GetAxis("Vertical");
            Throttle = Mathf.Clamp01(v);
            Brake = Mathf.Clamp01(-v);
            Steer = Input.GetAxis("Horizontal");
            Look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            IgnitionPressed = Input.GetKeyDown(KeyCode.I);
            GearUpPressed = Input.GetKeyDown(KeyCode.E);
            GearDownPressed = Input.GetKeyDown(KeyCode.Q);
            HandbrakePressed = Input.GetKeyDown(KeyCode.Space);
            DumpPressed = Input.GetKeyDown(KeyCode.B);
            LightsPressed = Input.GetKeyDown(KeyCode.L);
            HornHeld = Input.GetKey(KeyCode.H);
        }
    }
}
