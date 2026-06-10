using MiningTruckSim.Alerts;
using MiningTruckSim.Common;
using MiningTruckSim.Operation;
using MiningTruckSim.Scoring;
using MiningTruckSim.Track;
using MiningTruckSim.UI;
using MiningTruckSim.Vehicle;
using MiningTruckSim.View;
using UnityEngine;

namespace MiningTruckSim.Bootstrap
{
    /// <summary>
    /// Monta a cena jogável da Sprint 2 (critérios 2 e 4): chão, sol, câmera de cabine,
    /// caminhão, área de loading com escavadeira e área de unload, tudo ligado a um
    /// <see cref="CycleDirector"/>. Coloque num GameObject vazio e dê Play.
    /// </summary>
    public sealed class MineWorldBootstrap : MonoBehaviour
    {
        public Vector3 truckSpawn = new Vector3(0f, 1.2f, 0f);
        public Vector3 loadPoint = new Vector3(0f, 0f, 40f);
        public Vector3 unloadPoint = new Vector3(40f, 0f, -30f);

        [Tooltip("Tolerância lateral do trilho (m). Mina fácil ~4, difícil ~2 (critério 8).")]
        public float offTrackToleranceM = 4f;

        [Tooltip("Frequência média de alertas por minuto (vem da mina escolhida, critério 8).")]
        public float alertsPerMinute = 0.3f;

        private void Start()
        {
            EnsureSun();
            CreateGround();

            Camera cam = EnsureCamera();
            GameObject truckGo = ProceduralTruckBuilder.Build(truckSpawn, cam);
            var truck = truckGo.GetComponent<TruckController>();
            var dumpBed = truckGo.GetComponent<DumpBed>();

            // ---- Área de loading + escavadeira ----------------------------------
            OperationZone loadZone = CreateZone("LoadZone", ZoneKind.Load, loadPoint, Vector3.back,
                new Color(0.2f, 0.7f, 1f, 0.25f));
            // Escavadeira ao lado do ponto de carregamento, voltada para ele.
            Vector3 excavatorPos = loadPoint + new Vector3(10f, 0f, 0f);
            Excavator excavator = ProceduralExcavatorBuilder.Build(
                excavatorPos, Quaternion.LookRotation(loadPoint - excavatorPos));

            // ---- Área de unload --------------------------------------------------
            OperationZone unloadZone = CreateZone("UnloadZone", ZoneKind.Unload, unloadPoint,
                Vector3.left, new Color(1f, 0.6f, 0.1f, 0.25f));

            // ---- Trilho esperado loading → unload (critério 3) ------------------
            TrackPath path = CreateTrack(loadPoint, unloadPoint);
            var guide = truckGo.AddComponent<RouteGuide>();
            guide.truck = truck;
            guide.path = path;
            guide.offTrackToleranceM = offTrackToleranceM;

            // ---- Director --------------------------------------------------------
            var director = truckGo.AddComponent<CycleDirector>();
            director.truck = truck;
            director.dumpBed = dumpBed;
            director.loadZone = loadZone;
            director.unloadZone = unloadZone;
            director.excavator = excavator;
            director.fullLoadTonnes = truck.capacityTonnes;

            // HUD do ciclo (mostra fase/carga + estado do trilho).
            var hud = truckGo.AddComponent<CycleHud>();
            hud.director = director;
            hud.guide = guide;

            // ---- Pontuação de performance + tela de resultado (critério 6) ------
            var scorer = truckGo.AddComponent<PerformanceScorer>();
            scorer.truck = truck;
            scorer.routeGuide = guide;

            var resultScreen = truckGo.AddComponent<CycleResultScreen>();

            var presenter = truckGo.AddComponent<CycleResultPresenter>();
            presenter.director = director;
            presenter.scorer = scorer;
            presenter.resultScreen = resultScreen;

            // ---- Alertas aleatórios + procedimentos de conserto (critério 9) ----
            var alertSystem = truckGo.AddComponent<AlertSystem>();
            alertSystem.truck = truck;
            alertSystem.routeGuide = guide;
            alertSystem.scorer = scorer;
            alertSystem.alertsPerMinute = alertsPerMinute;

            var alertHud = truckGo.AddComponent<AlertHud>();
            alertHud.alertSystem = alertSystem;
        }

        private static TrackPath CreateTrack(Vector3 loadPoint, Vector3 unloadPoint)
        {
            var root = new GameObject("ExpectedTrack");
            var path = root.AddComponent<TrackPath>();

            // Trilho com uma curva intermediária entre loading e unload.
            Vector3 mid = Vector3.Lerp(loadPoint, unloadPoint, 0.5f) + new Vector3(-12f, 0f, 6f);
            Vector3[] pts = { loadPoint, mid, unloadPoint };
            foreach (Vector3 p in pts)
            {
                var wp = new GameObject("WP");
                wp.transform.SetParent(root.transform, false);
                wp.transform.position = new Vector3(p.x, 0f, p.z);
                path.waypoints.Add(wp.transform);
            }

            return path;
        }

        private static Camera EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            return cam;
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

        private static OperationZone CreateZone(string name, ZoneKind kind, Vector3 point,
            Vector3 desiredForward, Color color)
        {
            var root = new GameObject(name);
            root.transform.position = point;

            // Gatilho amplo da área.
            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(12f, 4f, 12f);
            trigger.center = new Vector3(0f, 2f, 0f);

            // Marcador visual no chão.
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "Pad";
            pad.transform.SetParent(root.transform, false);
            pad.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            pad.transform.localScale = new Vector3(10f, 0.1f, 10f);
            Object.Destroy(pad.GetComponent<Collider>());
            var mat = MaterialFactory.Create(color);
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            pad.GetComponent<Renderer>().sharedMaterial = mat;

            // Ponto de parada demarcado, com o rumo desejado de estacionamento.
            var parkPoint = new GameObject("ParkPoint");
            parkPoint.transform.SetParent(root.transform, false);
            parkPoint.transform.localRotation = Quaternion.LookRotation(desiredForward);

            var zone = root.AddComponent<OperationZone>();
            zone.kind = kind;
            zone.parkPoint = parkPoint.transform;
            return zone;
        }
    }
}
