using MiningTruckSim.Operation;
using NUnit.Framework;
using UnityEngine;

namespace MiningTruckSim.Tests
{
    public class OperationCycleTests
    {
        [Test]
        public void HappyPath_RunsThroughAllPhases()
        {
            var cycle = new OperationCycle { LoadRatePerSec = 0.5f, UnloadRatePerSec = 1f };
            Assert.AreEqual(CyclePhase.Idle, cycle.Phase);

            cycle.EnterLoadZone();
            Assert.AreEqual(CyclePhase.ApproachingLoad, cycle.Phase);

            cycle.SetParkedAtPoint(true);
            Assert.AreEqual(CyclePhase.Loading, cycle.Phase);

            // 2s a 0.5/s enche a caçamba
            cycle.Tick(1f, false);
            cycle.Tick(1f, false);
            Assert.AreEqual(CyclePhase.Loaded, cycle.Phase);
            Assert.AreEqual(1f, cycle.LoadFill, 1e-3f);

            cycle.EnterUnloadZone();
            Assert.AreEqual(CyclePhase.ApproachingUnload, cycle.Phase);

            // báscula levantada inicia o unload e esvazia
            cycle.Tick(0.5f, true);
            Assert.AreEqual(CyclePhase.Unloading, cycle.Phase);
            cycle.Tick(1f, true);
            cycle.Tick(1f, true);
            Assert.AreEqual(CyclePhase.Done, cycle.Phase);
            Assert.AreEqual(0f, cycle.LoadFill, 1e-3f);
            Assert.IsTrue(cycle.IsComplete);
        }

        [Test]
        public void LeavingPoint_DuringLoading_GoesBackToApproaching()
        {
            var cycle = new OperationCycle();
            cycle.EnterLoadZone();
            cycle.SetParkedAtPoint(true);
            Assert.AreEqual(CyclePhase.Loading, cycle.Phase);

            cycle.SetParkedAtPoint(false);
            Assert.AreEqual(CyclePhase.ApproachingLoad, cycle.Phase);
        }

        [Test]
        public void Unload_RequiresRaisedBed()
        {
            var cycle = new OperationCycle { LoadRatePerSec = 1f, UnloadRatePerSec = 1f };
            cycle.EnterLoadZone();
            cycle.SetParkedAtPoint(true);
            cycle.Tick(1f, false); // carrega
            cycle.EnterUnloadZone();

            // báscula abaixada: não descarrega
            cycle.Tick(2f, false);
            Assert.AreEqual(CyclePhase.ApproachingUnload, cycle.Phase);
            Assert.AreEqual(1f, cycle.LoadFill, 1e-3f);

            // levanta a báscula: descarrega
            cycle.Tick(2f, true);
            Assert.AreEqual(CyclePhase.Done, cycle.Phase);
        }

        [Test]
        public void PhaseChanged_FiresOnTransitions()
        {
            var cycle = new OperationCycle();
            int count = 0;
            CyclePhase last = CyclePhase.Idle;
            cycle.PhaseChanged += (_, next) => { count++; last = next; };

            cycle.EnterLoadZone();
            cycle.SetParkedAtPoint(true);

            Assert.AreEqual(2, count);
            Assert.AreEqual(CyclePhase.Loading, last);
        }
    }

    public class ParkingCheckTests
    {
        [Test]
        public void Parked_WhenClose_Aligned_AndSlow()
        {
            var check = ParkingCheck.Default;
            bool parked = check.IsParked(
                truckPos: new Vector3(1f, 0f, 0f),
                truckForward: Vector3.forward,
                pointPos: Vector3.zero,
                desiredForward: Vector3.forward,
                speedKmh: 1f);

            Assert.IsTrue(parked);
        }

        [Test]
        public void NotParked_WhenTooFar()
        {
            var check = ParkingCheck.Default;
            bool parked = check.IsParked(
                new Vector3(10f, 0f, 0f), Vector3.forward,
                Vector3.zero, Vector3.forward, 0f);
            Assert.IsFalse(parked);
        }

        [Test]
        public void NotParked_WhenMisaligned()
        {
            var check = ParkingCheck.Default;
            bool parked = check.IsParked(
                Vector3.zero, Vector3.right,
                Vector3.zero, Vector3.forward, 0f);
            Assert.IsFalse(parked);
        }

        [Test]
        public void NotParked_WhenMoving()
        {
            var check = ParkingCheck.Default;
            bool parked = check.IsParked(
                Vector3.zero, Vector3.forward,
                Vector3.zero, Vector3.forward, speedKmh: 20f);
            Assert.IsFalse(parked);
        }
    }
}
