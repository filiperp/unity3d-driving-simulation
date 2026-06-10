using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiningTruckSim.Track
{
    /// <summary>
    /// Componente de cena que define o trilho esperado por uma lista de waypoints
    /// (Transforms) e expõe um <see cref="RouteTrack"/> para a lógica de penalização.
    /// Também desenha o trilho no chão com um LineRenderer (mapa aberto, critério 3).
    /// </summary>
    public sealed class TrackPath : MonoBehaviour
    {
        [Tooltip("Waypoints na ordem loading → unload. Se vazio, usa os filhos diretos.")]
        public List<Transform> waypoints = new List<Transform>();

        public float lineWidth = 3f;
        public Color lineColor = new Color(1f, 0.85f, 0.2f, 0.9f);

        private RouteTrack _route;
        private LineRenderer _line;

        public RouteTrack Route => _route ??= BuildRoute();

        private void Awake()
        {
            CollectChildrenIfEmpty();
            _route = BuildRoute();
            BuildLine();
        }

        private void CollectChildrenIfEmpty()
        {
            if (waypoints.Count == 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    waypoints.Add(transform.GetChild(i));
                }
            }
        }

        private RouteTrack BuildRoute()
        {
            IEnumerable<Vector3> pts = waypoints.Where(w => w != null).Select(w => w.position);
            return new RouteTrack(pts);
        }

        private void BuildLine()
        {
            if (waypoints.Count < 2)
            {
                return;
            }

            _line = gameObject.GetComponent<LineRenderer>();
            if (_line == null)
            {
                _line = gameObject.AddComponent<LineRenderer>();
            }

            _line.useWorldSpace = true;
            _line.widthMultiplier = lineWidth;
            _line.numCornerVertices = 4;
            _line.material = new Material(Shader.Find("Universal Render Pipeline/Unlit")
                                          ?? Shader.Find("Sprites/Default"));
            _line.startColor = _line.endColor = lineColor;

            Vector3[] positions = waypoints.Where(w => w != null)
                .Select(w => new Vector3(w.position.x, 0.1f, w.position.z))
                .ToArray();
            _line.positionCount = positions.Length;
            _line.SetPositions(positions);
        }
    }
}
