using UnityEngine;

namespace MiningTruckSim.View
{
    /// <summary>
    /// Câmera em primeira pessoa na cabine (critério 1). Acompanha um ponto de
    /// ancoragem dentro da cabine e permite olhar em volta com o mouse, dentro de
    /// limites de ângulo, mantendo a orientação base do caminhão.
    /// </summary>
    public sealed class CabinCamera : MonoBehaviour
    {
        public Transform target;
        public float sensitivity = 2f;
        public float minPitch = -35f;
        public float maxPitch = 45f;
        public float maxYaw = 100f;

        private float _yaw;
        private float _pitch;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            _yaw += Input.GetAxis("Mouse X") * sensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            _yaw = Mathf.Clamp(_yaw, -maxYaw, maxYaw);
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            transform.position = target.position;
            transform.rotation = target.rotation * Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}
