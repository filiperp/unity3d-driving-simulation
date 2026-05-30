using MiningTruckSim.Common;
using UnityEngine;

namespace MiningTruckSim.Bootstrap
{
    /// <summary>
    /// Ponto de entrada da cena de teste da Sprint 1. Coloque este componente em um
    /// GameObject vazio numa cena nova e dê Play: ele cria o chão, a iluminação, a
    /// câmera e o caminhão jogável proceduralmente — sem montagem manual.
    /// </summary>
    public sealed class TruckSimBootstrap : MonoBehaviour
    {
        public bool buildGround = true;
        public Vector3 truckSpawn = new Vector3(0f, 1.2f, 0f);

        private void Start()
        {
            EnsureSun();

            if (buildGround)
            {
                CreateGround();
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            ProceduralTruckBuilder.Build(truckSpawn, cam);
        }

        private static void EnsureSun()
        {
            foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                {
                    return;
                }
            }

            var sunGo = new GameObject("Sun");
            sunGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            sun.color = new Color(1f, 0.97f, 0.9f);
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(60f, 1f, 60f); // ~600 x 600 m
            ground.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.45f, 0.40f, 0.33f), smoothness: 0.05f);
        }
    }
}
