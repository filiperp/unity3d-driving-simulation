using MiningTruckSim.Vehicle;
using UnityEngine;

namespace MiningTruckSim.Operation
{
    /// <summary>
    /// Orquestra o ciclo de operação na cena (critérios 2 e 4): conecta o caminhão, as
    /// zonas de load/unload e a escavadeira à máquina de estados <see cref="OperationCycle"/>.
    /// Lê presença/estacionamento das zonas, dirige a animação da escavadeira e o
    /// enchimento da caçamba (em toneladas) e aciona o unload pela báscula.
    /// </summary>
    public sealed class CycleDirector : MonoBehaviour
    {
        public TruckController truck;
        public DumpBed dumpBed;
        public OperationZone loadZone;
        public OperationZone unloadZone;
        public Excavator excavator;

        [Tooltip("Toneladas correspondentes à caçamba cheia (LoadFill = 1).")]
        public float fullLoadTonnes = 220f;

        public ParkingCheck parking = ParkingCheck.Default;
        public OperationCycle Cycle { get; } = new OperationCycle();

        private void Awake()
        {
            parking.PositionToleranceM = parking.PositionToleranceM <= 0f
                ? ParkingCheck.Default.PositionToleranceM
                : parking.PositionToleranceM;
        }

        private void Update()
        {
            UpdateZonePresence();
            UpdateParking();

            bool bedRaised = dumpBed != null && dumpBed.IsRaised;
            Cycle.Tick(Time.deltaTime, bedRaised);

            if (excavator != null)
            {
                excavator.Loading = Cycle.IsLoading;
            }

            if (truck != null)
            {
                truck.currentLoadTonnes = Cycle.LoadFill * fullLoadTonnes;
            }
        }

        private void UpdateZonePresence()
        {
            if (loadZone != null)
            {
                if (loadZone.TruckInside)
                {
                    Cycle.EnterLoadZone();
                }
                else
                {
                    Cycle.ExitLoadZone();
                }
            }

            if (unloadZone != null)
            {
                if (unloadZone.TruckInside)
                {
                    Cycle.EnterUnloadZone();
                }
                else
                {
                    Cycle.ExitUnloadZone();
                }
            }
        }

        private void UpdateParking()
        {
            if (truck == null)
            {
                return;
            }

            OperationZone active = Cycle.Phase switch
            {
                CyclePhase.ApproachingLoad or CyclePhase.Loading => loadZone,
                CyclePhase.ApproachingUnload or CyclePhase.Unloading => unloadZone,
                _ => null
            };

            if (active == null)
            {
                return;
            }

            bool parked = parking.IsParked(
                truck.transform.position,
                truck.transform.forward,
                active.PointPosition,
                active.PointForward,
                truck.SpeedKmh);

            Cycle.SetParkedAtPoint(parked);
        }
    }
}
