using System.Collections.Generic;
using UnityEngine;

namespace MiningTruckSim.Track
{
    /// <summary>
    /// Trilho esperado entre a área de loading e a de unload, representado como uma
    /// polilinha de waypoints no plano XZ (critério 3). Calcula, para uma posição do
    /// caminhão, o ponto mais próximo sobre o trilho, o desvio lateral e o progresso
    /// (0..1) ao longo da rota. Lógica pura (testável em EditMode).
    /// </summary>
    public sealed class RouteTrack
    {
        private readonly List<Vector3> _points = new List<Vector3>();
        private readonly List<float> _cumLength = new List<float>(); // comprimento acumulado até cada ponto

        public RouteTrack(IEnumerable<Vector3> waypoints)
        {
            foreach (Vector3 p in waypoints)
            {
                _points.Add(new Vector3(p.x, 0f, p.z));
            }

            RecomputeLengths();
        }

        public int PointCount => _points.Count;
        public float TotalLength => _cumLength.Count > 0 ? _cumLength[_cumLength.Count - 1] : 0f;
        public IReadOnlyList<Vector3> Points => _points;

        private void RecomputeLengths()
        {
            _cumLength.Clear();
            if (_points.Count == 0)
            {
                return;
            }

            float acc = 0f;
            _cumLength.Add(0f);
            for (int i = 1; i < _points.Count; i++)
            {
                acc += Vector3.Distance(_points[i - 1], _points[i]);
                _cumLength.Add(acc);
            }
        }

        /// <summary>
        /// Projeta <paramref name="worldPos"/> (XZ) sobre o trilho e devolve o desvio
        /// lateral (distância ao trilho), o progresso normalizado [0,1] e a direção
        /// (tangente) do trilho no ponto projetado.
        /// </summary>
        public TrackSample Sample(Vector3 worldPos)
        {
            var pos = new Vector3(worldPos.x, 0f, worldPos.z);

            if (_points.Count == 0)
            {
                return new TrackSample(pos, 0f, 0f, Vector3.forward);
            }

            if (_points.Count == 1)
            {
                return new TrackSample(_points[0], Vector3.Distance(pos, _points[0]), 0f, Vector3.forward);
            }

            float bestDist = float.MaxValue;
            Vector3 bestPoint = _points[0];
            Vector3 bestDir = Vector3.forward;
            float bestDistanceAlong = 0f;

            for (int i = 0; i < _points.Count - 1; i++)
            {
                Vector3 a = _points[i];
                Vector3 b = _points[i + 1];
                Vector3 ab = b - a;
                float abLenSq = ab.sqrMagnitude;
                float t = abLenSq > 1e-6f ? Mathf.Clamp01(Vector3.Dot(pos - a, ab) / abLenSq) : 0f;
                Vector3 proj = a + ab * t;
                float dist = Vector3.Distance(pos, proj);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPoint = proj;
                    bestDir = abLenSq > 1e-6f ? ab.normalized : Vector3.forward;
                    bestDistanceAlong = _cumLength[i] + Mathf.Sqrt(abLenSq) * t;
                }
            }

            float progress = TotalLength > 1e-6f ? Mathf.Clamp01(bestDistanceAlong / TotalLength) : 0f;
            return new TrackSample(bestPoint, bestDist, progress, bestDir);
        }
    }

    /// <summary>Resultado da projeção de uma posição sobre o trilho.</summary>
    public readonly struct TrackSample
    {
        /// <summary>Ponto mais próximo sobre o trilho (XZ).</summary>
        public readonly Vector3 ClosestPoint;

        /// <summary>Distância lateral ao trilho (m).</summary>
        public readonly float LateralDistance;

        /// <summary>Progresso ao longo do trilho em [0,1].</summary>
        public readonly float Progress;

        /// <summary>Tangente (direção de avanço) do trilho no ponto projetado.</summary>
        public readonly Vector3 Direction;

        public TrackSample(Vector3 closestPoint, float lateralDistance, float progress, Vector3 direction)
        {
            ClosestPoint = closestPoint;
            LateralDistance = lateralDistance;
            Progress = progress;
            Direction = direction;
        }
    }
}
