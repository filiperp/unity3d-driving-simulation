using MiningTruckSim.Common;
using MiningTruckSim.Operation;
using UnityEngine;

namespace MiningTruckSim.Bootstrap
{
    /// <summary>
    /// Monta uma escavadeira de mineração a partir de primitivas (procedural-first),
    /// com articulações de giro (swing) e concha (boom) ligadas ao <see cref="Excavator"/>.
    /// Geometria sob "Body_ProceduralSlot" para troca pelos modelos comprados na S8.
    /// </summary>
    public static class ProceduralExcavatorBuilder
    {
        public static Excavator Build(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("Excavator");
            root.transform.SetPositionAndRotation(position, rotation);

            var slot = new GameObject("Body_ProceduralSlot");
            slot.transform.SetParent(root.transform, false);

            var trackColor = new Color(0.15f, 0.15f, 0.17f);
            var bodyColor = new Color(0.90f, 0.75f, 0.05f);

            // Esteiras + base.
            CreateBox("TrackL", slot.transform, new Vector3(-1.6f, 0.5f, 0f),
                new Vector3(1.0f, 1.0f, 6.0f), trackColor);
            CreateBox("TrackR", slot.transform, new Vector3(1.6f, 0.5f, 0f),
                new Vector3(1.0f, 1.0f, 6.0f), trackColor);
            CreateBox("Base", slot.transform, new Vector3(0f, 1.1f, 0f),
                new Vector3(3.4f, 0.4f, 5.0f), bodyColor);

            // Giro (cabine + lança) — pivô no centro da base.
            var swing = new GameObject("Swing");
            swing.transform.SetParent(slot.transform, false);
            swing.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            CreateBox("Cab", swing.transform, new Vector3(-0.8f, 0.9f, -0.5f),
                new Vector3(1.6f, 1.8f, 2.4f), bodyColor);
            CreateBox("CounterWeight", swing.transform, new Vector3(0f, 0.6f, -2.2f),
                new Vector3(2.6f, 1.2f, 1.2f), trackColor);

            // Concha (boom) — articula no eixo X, à frente da cabine.
            var boom = new GameObject("Boom");
            boom.transform.SetParent(swing.transform, false);
            boom.transform.localPosition = new Vector3(0.6f, 0.8f, 1.0f);
            CreateBox("Arm", boom.transform, new Vector3(0f, 0.2f, 2.2f),
                new Vector3(0.6f, 0.6f, 4.0f), bodyColor);
            CreateBox("Bucket", boom.transform, new Vector3(0f, -0.4f, 4.2f),
                new Vector3(1.6f, 1.2f, 1.2f), trackColor);

            var excavator = root.AddComponent<Excavator>();
            excavator.swing = swing.transform;
            excavator.boom = boom.transform;
            return excavator;
        }

        private static void CreateBox(string name, Transform parent, Vector3 localPos,
            Vector3 size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color);
            Object.Destroy(go.GetComponent<Collider>());
        }
    }
}
