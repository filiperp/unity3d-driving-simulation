using MiningTruckSim.Vehicle;
using NUnit.Framework;

namespace MiningTruckSim.Tests
{
    public class GearboxTests
    {
        [Test]
        public void Starts_InPark_NoTorque()
        {
            var gb = new Gearbox();
            Assert.AreEqual(GearMode.Park, gb.Mode);
            Assert.AreEqual(0f, gb.DirectionSign);
            Assert.IsTrue(gb.IsParked);
            Assert.IsFalse(gb.CanDeliverTorque);
            Assert.AreEqual("P", gb.DisplayGear);
        }

        [Test]
        public void ShiftUp_CyclesParkReverseNeutralDrive_AndClamps()
        {
            var gb = new Gearbox();
            gb.ShiftUp();
            Assert.AreEqual(GearMode.Reverse, gb.Mode);
            Assert.AreEqual(-1f, gb.DirectionSign);

            gb.ShiftUp();
            Assert.AreEqual(GearMode.Neutral, gb.Mode);

            gb.ShiftUp();
            Assert.AreEqual(GearMode.Drive, gb.Mode);
            Assert.AreEqual(1f, gb.DirectionSign);

            gb.ShiftUp(); // clamp em Drive
            Assert.AreEqual(GearMode.Drive, gb.Mode);
        }

        [Test]
        public void ShiftDown_ClampsAtPark()
        {
            var gb = new Gearbox();
            gb.ShiftDown();
            Assert.AreEqual(GearMode.Park, gb.Mode);
        }

        [Test]
        public void AutoShift_PicksForwardGearBySpeed_OnlyInDrive()
        {
            var gb = new Gearbox(new[] { 12f, 24f, 36f });

            gb.SetMode(GearMode.Drive);
            gb.AutoShift(0f);
            Assert.AreEqual(1, gb.CurrentForwardGear);
            gb.AutoShift(20f);
            Assert.AreEqual(2, gb.CurrentForwardGear);
            gb.AutoShift(40f);
            Assert.AreEqual(4, gb.CurrentForwardGear);
            Assert.AreEqual("D4", gb.DisplayGear);

            gb.SetMode(GearMode.Neutral);
            gb.AutoShift(40f);
            Assert.AreEqual(1, gb.CurrentForwardGear);
        }
    }

    public class EngineModelTests
    {
        [Test]
        public void Stopped_StaysAtZeroRpm()
        {
            var e = new EngineModel();
            for (int i = 0; i < 50; i++)
            {
                e.Step(0.1f, 1f, 0f);
            }

            Assert.IsFalse(e.Running);
            Assert.AreEqual(0f, e.Rpm, 1e-2f);
        }

        [Test]
        public void Running_ReachesIdle_ThenRisesWithThrottle()
        {
            var e = new EngineModel();
            e.Start();
            for (int i = 0; i < 100; i++)
            {
                e.Step(0.1f, 0f, 0f);
            }

            Assert.AreEqual(e.IdleRpm, e.Rpm, 5f);

            for (int i = 0; i < 200; i++)
            {
                e.Step(0.1f, 1f, 0f);
            }

            Assert.Greater(e.Rpm, e.IdleRpm + 100f);
            Assert.LessOrEqual(e.Rpm, e.MaxRpm + 1f);
        }

        [Test]
        public void Overload_RaisesTemperature()
        {
            var normal = new EngineModel();
            var overloaded = new EngineModel();
            normal.Start();
            overloaded.Start();

            for (int i = 0; i < 600; i++)
            {
                normal.Step(0.1f, 0.5f, 0.9f);
                overloaded.Step(0.1f, 0.5f, 1.5f);
            }

            Assert.Greater(overloaded.TempC, normal.TempC);
            Assert.LessOrEqual(overloaded.TempC, overloaded.MaxTempC);
        }
    }

    public class DumpBedMotorTests
    {
        [Test]
        public void Raises_ToMax_AndClamps()
        {
            var m = new DumpBedMotor { MaxAngleDeg = 50f, SpeedDegPerSec = 25f, RaiseRequested = true };
            for (int i = 0; i < 10; i++)
            {
                m.Step(0.5f); // 5s * 25 = 125 deg solicitado, deve travar em 50
            }

            Assert.AreEqual(50f, m.AngleDeg, 1e-3f);
            Assert.IsTrue(m.IsFullyRaised);
            Assert.IsFalse(m.IsFullyLowered);
        }

        [Test]
        public void Lowers_BackToZero()
        {
            var m = new DumpBedMotor { MaxAngleDeg = 50f, SpeedDegPerSec = 25f, RaiseRequested = true };
            for (int i = 0; i < 10; i++)
            {
                m.Step(0.5f);
            }

            m.RaiseRequested = false;
            for (int i = 0; i < 10; i++)
            {
                m.Step(0.5f);
            }

            Assert.AreEqual(0f, m.AngleDeg, 1e-3f);
            Assert.IsTrue(m.IsFullyLowered);
        }
    }
}
