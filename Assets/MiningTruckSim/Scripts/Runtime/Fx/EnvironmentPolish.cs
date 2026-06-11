using UnityEngine;
using UnityEngine.Rendering;

namespace MiningTruckSim.Fx
{
    /// <summary>
    /// Ambientação da cena de mineração (polimento visual, S9): céu, luz ambiente, névoa
    /// (poeira atmosférica) e sombras do sol. Aplica via <see cref="RenderSettings"/> e
    /// <see cref="QualitySettings"/> em runtime, para deixar a cena apresentável sem
    /// depender de assets de skybox/lighting. Idempotente; chame uma vez no bootstrap.
    /// </summary>
    public static class EnvironmentPolish
    {
        public static void Apply(Light sun = null)
        {
            // Céu/horizonte por cor sólida quando não há skybox dedicado.
            RenderSettings.skybox = null;
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.62f, 0.70f, 0.80f); // céu poente empoeirado
                cam.farClipPlane = Mathf.Max(cam.farClipPlane, 1200f);
            }

            // Luz ambiente quente e suave.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.70f, 0.75f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.52f, 0.46f);
            RenderSettings.ambientGroundColor = new Color(0.32f, 0.28f, 0.23f);

            // Névoa de poeira para dar profundidade ao mapa aberto.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.66f, 0.66f, 0.62f);
            RenderSettings.fogDensity = 0.0018f;

            if (sun != null)
            {
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.7f;
            }
        }
    }
}
