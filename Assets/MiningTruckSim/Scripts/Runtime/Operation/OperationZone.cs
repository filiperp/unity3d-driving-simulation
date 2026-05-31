using UnityEngine;

namespace MiningTruckSim.Operation
{
    public enum ZoneKind
    {
        Load,
        Unload
    }

    /// <summary>
    /// Área demarcada de loading ou unload (critérios 2 e 4). Combina um gatilho amplo
    /// (entrar/sair da área) com um ponto de parada (posição + rumo desejado) usado pelo
    /// <see cref="ParkingCheck"/>. Detecta o caminhão pela presença de um Rigidbody no alvo.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class OperationZone : MonoBehaviour
    {
        public ZoneKind kind = ZoneKind.Load;

        [Tooltip("Transform do ponto de parada demarcado; usa este objeto se vazio.")]
        public Transform parkPoint;

        public bool TruckInside { get; private set; }
        public Transform TrackedTruck { get; private set; }

        public Vector3 PointPosition => parkPoint != null ? parkPoint.position : transform.position;
        public Vector3 PointForward => parkPoint != null ? parkPoint.forward : transform.forward;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null)
            {
                return;
            }

            TruckInside = true;
            TrackedTruck = rb.transform;
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null && rb.transform == TrackedTruck)
            {
                TruckInside = false;
                TrackedTruck = null;
            }
        }
    }
}
