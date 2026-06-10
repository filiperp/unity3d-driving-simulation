using System;
using UnityEngine;

namespace MiningTruckSim.Operation
{
    /// <summary>
    /// Verifica se o caminhão está "estacionado no ponto de carregamento": dentro de um
    /// raio do ponto, alinhado a um rumo desejado e praticamente parado (critério 2).
    /// Lógica isolada para ser testável e reutilizada nos pontos de load e unload.
    /// </summary>
    [Serializable]
    public struct ParkingCheck
    {
        public float PositionToleranceM;
        public float HeadingToleranceDeg;
        public float MaxSpeedKmh;

        public static ParkingCheck Default => new ParkingCheck
        {
            PositionToleranceM = 4f,
            HeadingToleranceDeg = 35f,
            MaxSpeedKmh = 3f
        };

        public bool IsParked(Vector3 truckPos, Vector3 truckForward,
            Vector3 pointPos, Vector3 desiredForward, float speedKmh)
        {
            float planarDist = Vector2.Distance(
                new Vector2(truckPos.x, truckPos.z),
                new Vector2(pointPos.x, pointPos.z));
            if (planarDist > PositionToleranceM)
            {
                return false;
            }

            if (speedKmh > MaxSpeedKmh)
            {
                return false;
            }

            Vector3 a = Flatten(truckForward);
            Vector3 b = Flatten(desiredForward);
            if (a.sqrMagnitude < 1e-4f || b.sqrMagnitude < 1e-4f)
            {
                return true; // sem direção significativa para comparar
            }

            float angle = Vector3.Angle(a, b);
            return angle <= HeadingToleranceDeg;
        }

        private static Vector3 Flatten(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
