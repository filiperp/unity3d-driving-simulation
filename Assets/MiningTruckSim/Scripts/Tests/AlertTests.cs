using System.Collections.Generic;
using MiningTruckSim.Alerts;
using NUnit.Framework;

namespace MiningTruckSim.Tests
{
    public class AlertCatalogTests
    {
        [Test]
        public void EveryType_HasDistinctProcedure()
        {
            var instructions = new HashSet<string>();
            foreach (AlertType type in AlertCatalog.AllTypes)
            {
                AlertCatalog.Definition def = AlertCatalog.Get(type);
                Assert.IsNotEmpty(def.Title);
                Assert.IsNotEmpty(def.Repair.Instruction);
                Assert.Greater(def.PenaltyPerSecond, 0f);
                // Procedimentos de conserto são distintos entre os tipos (critério 9).
                Assert.IsTrue(instructions.Add(def.Repair.Instruction),
                    $"Procedimento duplicado para {type}");
            }
        }

        [Test]
        public void Filter_UsesManualAction_OthersVary()
        {
            Assert.AreEqual(RepairKind.ManualHoldAction,
                AlertCatalog.Get(AlertType.CloggedFilter).Repair.Kind);
            Assert.AreEqual(RepairKind.HoldCondition,
                AlertCatalog.Get(AlertType.OffTrack).Repair.Kind);
        }
    }

    public class ActiveAlertTests
    {
        [Test]
        public void Resolves_WhenConditionHeldLongEnough()
        {
            var alert = new ActiveAlert(AlertType.OffTrack); // hold 1.5s
            alert.Tick(0.5f, true);
            Assert.IsFalse(alert.Resolved);
            alert.Tick(0.5f, true);
            alert.Tick(0.6f, true);
            Assert.IsTrue(alert.Resolved);
            Assert.AreEqual(1f, alert.RepairProgress01, 1e-3f);
        }

        [Test]
        public void Progress_ResetsWhenConditionBreaks()
        {
            var alert = new ActiveAlert(AlertType.OffTrack);
            alert.Tick(1.0f, true);
            Assert.Greater(alert.RepairProgress01, 0f);

            alert.Tick(0.5f, false); // quebrou a condição
            Assert.AreEqual(0f, alert.RepairProgress01, 1e-3f);
            Assert.IsFalse(alert.Resolved);
        }

        [Test]
        public void AccumulatesPenalty_WhileUnresolved()
        {
            var alert = new ActiveAlert(AlertType.Overload); // 5/s
            alert.Tick(1f, false);
            alert.Tick(1f, false);
            Assert.AreEqual(10f, alert.AccumulatedPenalty, 1e-3f);
        }
    }

    public class AlertSchedulerTests
    {
        private static readonly AlertType[] AllEligible =
        {
            AlertType.HighOilPressure, AlertType.CloggedFilter,
            AlertType.OffTrack, AlertType.Overload
        };

        [Test]
        public void Deterministic_ForSameSeed()
        {
            var a = new AlertScheduler(alertsPerMinute: 30f, seed: 7);
            var b = new AlertScheduler(alertsPerMinute: 30f, seed: 7);

            var seqA = new List<AlertType?>();
            var seqB = new List<AlertType?>();
            var emptyActive = new HashSet<AlertType>();

            for (int i = 0; i < 200; i++)
            {
                seqA.Add(a.Tick(0.1f, AllEligible, emptyActive));
                seqB.Add(b.Tick(0.1f, AllEligible, emptyActive));
            }

            CollectionAssert.AreEqual(seqA, seqB);
        }

        [Test]
        public void EventuallySpawns_AtReasonableRate()
        {
            var scheduler = new AlertScheduler(alertsPerMinute: 60f, seed: 1);
            var active = new HashSet<AlertType>();
            int count = 0;

            // 5 minutos simulados a 60/min => ordem de grandeza de centenas.
            for (int i = 0; i < 3000; i++)
            {
                AlertType? t = scheduler.Tick(0.1f, AllEligible, active);
                if (t.HasValue)
                {
                    count++;
                }
            }

            Assert.Greater(count, 50);
        }

        [Test]
        public void NeverSpawns_TypeAlreadyActive()
        {
            var scheduler = new AlertScheduler(alertsPerMinute: 600f, seed: 3);
            var active = new HashSet<AlertType> { AlertType.HighOilPressure };
            var onlyOil = new[] { AlertType.HighOilPressure };

            for (int i = 0; i < 500; i++)
            {
                AlertType? t = scheduler.Tick(0.1f, onlyOil, active);
                Assert.IsNull(t); // único tipo elegível já está ativo
            }
        }

        [Test]
        public void NeverSpawns_WhenFrequencyZero()
        {
            var scheduler = new AlertScheduler(alertsPerMinute: 0f, seed: 1);
            var active = new HashSet<AlertType>();
            for (int i = 0; i < 1000; i++)
            {
                Assert.IsNull(scheduler.Tick(0.1f, AllEligible, active));
            }
        }
    }
}
