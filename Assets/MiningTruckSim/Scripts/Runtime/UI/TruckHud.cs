using MiningTruckSim.Vehicle;
using UnityEngine;

namespace MiningTruckSim.UI
{
    /// <summary>
    /// HUD provisório (IMGUI) com instrumentos e ajuda de controles. Usa OnGUI para
    /// não exigir montagem de Canvas; será substituído por UI definitiva no polimento.
    /// </summary>
    public sealed class TruckHud : MonoBehaviour
    {
        public TruckController truck;
        public DumpBed dumpBed;
        public TruckLights lights;

        private GUIStyle _panel;

        private void OnGUI()
        {
            if (truck == null)
            {
                return;
            }

            _panel ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 15,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 10, 10)
            };

            TruckTelemetry t = truck.Telemetry;
            string dump = dumpBed == null
                ? "-"
                : dumpBed.IsRaised ? "LEVANTADA" : dumpBed.IsLowered ? "abaixada" : "movendo";

            string instruments =
                $"Ignição: {(truck.Engine.Running ? "LIGADO" : "DESLIGADO")}\n" +
                $"Marcha: {truck.Gearbox.DisplayGear}\n" +
                $"Velocidade: {t.SpeedKmh:0} km/h\n" +
                $"RPM: {t.Rpm:0}\n" +
                $"Temp. motor: {t.EngineTempC:0} °C\n" +
                $"Carga: {truck.currentLoadTonnes:0}/{truck.capacityTonnes:0} t\n" +
                $"Freio de mão: {(truck.HandbrakeOn ? "ON" : "off")}\n" +
                (lights != null ? $"Faróis: {(lights.On ? "ON" : "off")}\n" : string.Empty) +
                $"Báscula: {dump}";

            GUI.Box(new Rect(12, 12, 250, 210), instruments, _panel);

            const string help =
                "Controles:\n" +
                "W/S acelerar/frear · A/D direção\n" +
                "Mouse olhar · I ignição · Q/E marcha\n" +
                "Espaço freio de mão · B báscula\n" +
                "L faróis · H buzina";
            GUI.Box(new Rect(12, Screen.height - 110, 360, 98), help, _panel);
        }
    }
}
