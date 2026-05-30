using MiningTruckSim.Common;
using MiningTruckSim.UI;
using MiningTruckSim.Vehicle;
using MiningTruckSim.View;
using UnityEngine;

namespace MiningTruckSim.Bootstrap
{
    /// <summary>
    /// Monta um caminhão de mineração jogável a partir de primitivas (procedural-first).
    /// A geometria visual fica sob "Body_ProceduralSlot" para troca futura pelos modelos
    /// comprados (Sprint 8), sem mexer na física/lógica. Configura WheelColliders,
    /// TruckController, câmera de cabine, báscula, faróis, buzina e HUD.
    /// </summary>
    public static class ProceduralTruckBuilder
    {
        private const float WheelRadius = 0.7f;

        public static GameObject Build(Vector3 position, Camera cam = null)
        {
            var root = new GameObject("MiningTruck");
            root.transform.position = position;

            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 9000f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.5f;

            // ---- Visual procedural (slot trocável) -------------------------------
            var bodySlot = new GameObject("Body_ProceduralSlot");
            bodySlot.transform.SetParent(root.transform, false);

            CreateBox("Chassis", bodySlot.transform, new Vector3(0f, 0.7f, 0f),
                new Vector3(3f, 0.7f, 8f), new Color(0.20f, 0.22f, 0.26f), keepCollider: true);
            CreateBox("Cab", bodySlot.transform, new Vector3(0f, 1.9f, 2.6f),
                new Vector3(2.6f, 1.6f, 2.2f), new Color(0.85f, 0.65f, 0.10f), keepCollider: false);
            CreateBox("EngineHood", bodySlot.transform, new Vector3(0f, 1.2f, 3.9f),
                new Vector3(2.6f, 1.0f, 1.4f), new Color(0.85f, 0.65f, 0.10f), keepCollider: false);

            // Caçamba basculante com pivô na traseira.
            var dumpPivot = new GameObject("DumpBedPivot");
            dumpPivot.transform.SetParent(bodySlot.transform, false);
            dumpPivot.transform.localPosition = new Vector3(0f, 1.2f, -3.6f);
            CreateBox("DumpBox", dumpPivot.transform, new Vector3(0f, 0.6f, 3.0f),
                new Vector3(3.0f, 1.2f, 6.5f), new Color(0.35f, 0.36f, 0.40f), keepCollider: false);

            // ---- Faróis ----------------------------------------------------------
            Light headLeft = CreateHeadlight(bodySlot.transform, new Vector3(-0.9f, 1.1f, 4.6f));
            Light headRight = CreateHeadlight(bodySlot.transform, new Vector3(0.9f, 1.1f, 4.6f));

            // ---- Ancoragem da câmera de cabine ----------------------------------
            var cabinAnchor = new GameObject("CabinCameraAnchor");
            cabinAnchor.transform.SetParent(root.transform, false);
            cabinAnchor.transform.localPosition = new Vector3(0f, 2.4f, 3.0f);

            // ---- Rodas (colliders + malhas) -------------------------------------
            const float halfTrack = 1.45f;
            const float frontZ = 2.6f;
            const float rearZ = -2.6f;
            const float axleY = -0.1f;

            var fl = CreateWheel("WheelFL", root.transform, new Vector3(-halfTrack, axleY, frontZ));
            var fr = CreateWheel("WheelFR", root.transform, new Vector3(halfTrack, axleY, frontZ));
            var rl = CreateWheel("WheelRL", root.transform, new Vector3(-halfTrack, axleY, rearZ));
            var rr = CreateWheel("WheelRR", root.transform, new Vector3(halfTrack, axleY, rearZ));

            // ---- Componentes -----------------------------------------------------
            var truck = root.AddComponent<TruckController>();
            truck.frontLeft = fl.collider;
            truck.frontRight = fr.collider;
            truck.rearLeft = rl.collider;
            truck.rearRight = rr.collider;
            truck.frontLeftMesh = fl.mesh;
            truck.frontRightMesh = fr.mesh;
            truck.rearLeftMesh = rl.mesh;
            truck.rearRightMesh = rr.mesh;

            var dumpBed = root.AddComponent<DumpBed>();
            dumpBed.pivot = dumpPivot.transform;

            var lights = root.AddComponent<TruckLights>();
            lights.headlights.Add(headLeft);
            lights.headlights.Add(headRight);

            root.AddComponent<TruckHorn>();

            var hud = root.AddComponent<TruckHud>();
            hud.truck = truck;
            hud.dumpBed = dumpBed;
            hud.lights = lights;

            // ---- Câmera de cabine ------------------------------------------------
            if (cam != null)
            {
                var cabinCam = cam.GetComponent<CabinCamera>();
                if (cabinCam == null)
                {
                    cabinCam = cam.gameObject.AddComponent<CabinCamera>();
                }

                cabinCam.target = cabinAnchor.transform;
                cam.transform.SetPositionAndRotation(cabinAnchor.transform.position,
                    cabinAnchor.transform.rotation);
            }

            return root;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 localPos,
            Vector3 size, Color color, bool keepCollider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color);

            if (!keepCollider)
            {
                Object.Destroy(go.GetComponent<Collider>());
            }

            return go;
        }

        private static Light CreateHeadlight(Transform parent, Vector3 localPos)
        {
            var go = new GameObject("Headlight");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity; // aponta para +Z (frente)

            var light = go.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 70f;
            light.spotAngle = 75f;
            light.intensity = 5f;
            light.color = new Color(1f, 0.96f, 0.85f);
            light.enabled = false;
            return light;
        }

        private static (WheelCollider collider, Transform mesh) CreateWheel(string name,
            Transform root, Vector3 localPos)
        {
            var colliderGo = new GameObject(name + "_Collider");
            colliderGo.transform.SetParent(root, false);
            colliderGo.transform.localPosition = localPos;

            var wc = colliderGo.AddComponent<WheelCollider>();
            wc.radius = WheelRadius;
            wc.mass = 80f;
            wc.suspensionDistance = 0.3f;
            wc.forceAppPointDistance = 0f;

            JointSpring spring = wc.suspensionSpring;
            spring.spring = 120000f;
            spring.damper = 9000f;
            spring.targetPosition = 0.5f;
            wc.suspensionSpring = spring;

            WheelFrictionCurve fwd = wc.forwardFriction;
            fwd.stiffness = 2.2f;
            wc.forwardFriction = fwd;

            WheelFrictionCurve side = wc.sidewaysFriction;
            side.stiffness = 2.6f;
            wc.sidewaysFriction = side;

            // Âncora da malha (recebe pose do WheelCollider a cada frame).
            var meshAnchor = new GameObject(name + "_Mesh");
            meshAnchor.transform.SetParent(root, false);
            meshAnchor.transform.localPosition = localPos;

            var tire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tire.name = "Tire";
            tire.transform.SetParent(meshAnchor.transform, false);
            tire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // eixo do cilindro -> X
            float diameter = WheelRadius * 2f;
            tire.transform.localScale = new Vector3(diameter, 0.25f, diameter);
            tire.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.08f, 0.08f, 0.08f));
            Object.Destroy(tire.GetComponent<Collider>());

            return (wc, meshAnchor.transform);
        }
    }
}
