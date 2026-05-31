using MiningTruckSim.Scoring;
using UnityEngine;

namespace MiningTruckSim.Vehicle
{
    /// <summary>
    /// Direção do caminhão em física (WheelCollider) com ignição, câmbio automático,
    /// ré, freio de serviço e freio de mão (critérios 1 e 5). Expõe a telemetria usada
    /// por scoring (critério 6), alertas (S5) e HUD.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TruckController : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        public WheelCollider frontLeft;
        public WheelCollider frontRight;
        public WheelCollider rearLeft;
        public WheelCollider rearRight;

        [Header("Wheel Meshes (visual)")]
        public Transform frontLeftMesh;
        public Transform frontRightMesh;
        public Transform rearLeftMesh;
        public Transform rearRightMesh;

        [Header("Tuning")]
        public float maxMotorTorque = 6000f;
        public float maxBrakeTorque = 12000f;
        public float maxSteerAngle = 28f;
        public float handbrakeTorque = 20000f;

        [Header("Load")]
        public float capacityTonnes = 220f;
        public float currentLoadTonnes = 0f;

        public EngineModel Engine { get; } = new EngineModel();
        public Gearbox Gearbox { get; } = new Gearbox();
        public TruckInputReader Input { get; } = new TruckInputReader();
        public TruckTelemetry Telemetry { get; private set; }
        public bool HandbrakeOn { get; private set; } = true;
        public float SpeedKmh => Telemetry.SpeedKmh;
        public float LoadRatio => capacityTonnes > 0f ? currentLoadTonnes / capacityTonnes : 0f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass = new Vector3(0f, -0.6f, 0f);
        }

        private void Update()
        {
            Input.Sample();

            if (Input.IgnitionPressed)
            {
                Engine.Toggle();
            }

            if (Input.GearUpPressed)
            {
                Gearbox.ShiftUp();
            }

            if (Input.GearDownPressed)
            {
                Gearbox.ShiftDown();
            }

            if (Input.HandbrakePressed)
            {
                HandbrakeOn = !HandbrakeOn;
            }

            UpdateWheelMeshes();
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            float speedKmh = _rb.linearVelocity.magnitude * 3.6f;
            float loadRatio = LoadRatio;

            Engine.Step(dt, Input.Throttle, loadRatio);
            Gearbox.AutoShift(speedKmh);

            ApplyDrive();
            ApplySteer();
            ApplyBrakes();

            Telemetry = new TruckTelemetry(Engine.Rpm, speedKmh, Engine.TempC, loadRatio);
        }

        private void ApplyDrive()
        {
            float torque = 0f;
            if (Engine.Running && Gearbox.CanDeliverTorque && !HandbrakeOn)
            {
                torque = Input.Throttle * maxMotorTorque * Gearbox.DirectionSign;
            }

            SetTorque(rearLeft, torque);
            SetTorque(rearRight, torque);
        }

        private void ApplySteer()
        {
            float angle = Input.Steer * maxSteerAngle;
            SetSteer(frontLeft, angle);
            SetSteer(frontRight, angle);
        }

        private void ApplyBrakes()
        {
            float service = Input.Brake * maxBrakeTorque;
            float parking = (HandbrakeOn || Gearbox.IsParked) ? handbrakeTorque : 0f;

            SetBrake(frontLeft, service);
            SetBrake(frontRight, service);
            SetBrake(rearLeft, service + parking);
            SetBrake(rearRight, service + parking);
        }

        private void UpdateWheelMeshes()
        {
            SyncMesh(frontLeft, frontLeftMesh);
            SyncMesh(frontRight, frontRightMesh);
            SyncMesh(rearLeft, rearLeftMesh);
            SyncMesh(rearRight, rearRightMesh);
        }

        private static void SetTorque(WheelCollider wc, float value)
        {
            if (wc != null)
            {
                wc.motorTorque = value;
            }
        }

        private static void SetSteer(WheelCollider wc, float value)
        {
            if (wc != null)
            {
                wc.steerAngle = value;
            }
        }

        private static void SetBrake(WheelCollider wc, float value)
        {
            if (wc != null)
            {
                wc.brakeTorque = value;
            }
        }

        private static void SyncMesh(WheelCollider wc, Transform mesh)
        {
            if (wc == null || mesh == null)
            {
                return;
            }

            wc.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.SetPositionAndRotation(pos, rot);
        }
    }
}
