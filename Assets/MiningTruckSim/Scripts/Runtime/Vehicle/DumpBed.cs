using UnityEngine;

namespace MiningTruckSim.Vehicle
{
    /// <summary>
    /// Componente visual da caçamba basculante (báscula). Aciona com a tecla [B] e
    /// gira o <see cref="pivot"/> usando o <see cref="DumpBedMotor"/>. A lógica de
    /// unload (despejo no ponto demarcado) consome <see cref="IsRaised"/> na S2.
    /// </summary>
    public sealed class DumpBed : MonoBehaviour
    {
        public Transform pivot;
        public Vector3 hingeAxis = Vector3.right;
        public float maxAngleDeg = 50f;
        public float speedDegPerSec = 25f;
        public KeyCode toggleKey = KeyCode.B;

        public DumpBedMotor Motor { get; } = new DumpBedMotor();

        private Quaternion _restRotation = Quaternion.identity;

        private void Awake()
        {
            Motor.MaxAngleDeg = maxAngleDeg;
            Motor.SpeedDegPerSec = speedDegPerSec;
            if (pivot != null)
            {
                _restRotation = pivot.localRotation;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                Motor.Toggle();
            }

            Motor.Step(Time.deltaTime);

            if (pivot != null)
            {
                pivot.localRotation = _restRotation * Quaternion.AngleAxis(-Motor.AngleDeg, hingeAxis);
            }
        }

        public bool IsRaised => Motor.IsFullyRaised;
        public bool IsLowered => Motor.IsFullyLowered;

        /// <summary>Pede para levantar/abaixar (usado por gatilhos de unload).</summary>
        public void SetRaised(bool raised) => Motor.RaiseRequested = raised;
    }
}
