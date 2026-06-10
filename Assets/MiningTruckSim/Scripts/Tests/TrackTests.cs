using MiningTruckSim.Track;
using NUnit.Framework;
using UnityEngine;

namespace MiningTruckSim.Tests
{
    public class RouteTrackTests
    {
        private static RouteTrack StraightTrack()
        {
            // Linha reta de (0,0,0) a (0,0,100) ao longo de +Z.
            return new RouteTrack(new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 50f),
                new Vector3(0f, 0f, 100f),
            });
        }

        [Test]
        public void TotalLength_IsSumOfSegments()
        {
            var track = StraightTrack();
            Assert.AreEqual(100f, track.TotalLength, 1e-3f);
        }

        [Test]
        public void Sample_OnTrack_ZeroLateral_AndProgress()
        {
            var track = StraightTrack();
            TrackSample s = track.Sample(new Vector3(0f, 5f, 50f));

            Assert.AreEqual(0f, s.LateralDistance, 1e-3f);
            Assert.AreEqual(0.5f, s.Progress, 1e-3f);
            Assert.AreEqual(Vector3.forward, s.Direction);
        }

        [Test]
        public void Sample_OffToSide_ReturnsLateralDistance()
        {
            var track = StraightTrack();
            TrackSample s = track.Sample(new Vector3(8f, 0f, 25f));

            Assert.AreEqual(8f, s.LateralDistance, 1e-3f);
            Assert.AreEqual(0.25f, s.Progress, 1e-3f);
        }

        [Test]
        public void Sample_BeforeStart_ClampsToStart()
        {
            var track = StraightTrack();
            TrackSample s = track.Sample(new Vector3(0f, 0f, -20f));

            Assert.AreEqual(0f, s.Progress, 1e-3f);
            Assert.AreEqual(20f, s.LateralDistance, 1e-3f);
        }

        [Test]
        public void Sample_PastEnd_ClampsToEnd()
        {
            var track = StraightTrack();
            TrackSample s = track.Sample(new Vector3(0f, 0f, 130f));

            Assert.AreEqual(1f, s.Progress, 1e-3f);
            Assert.AreEqual(30f, s.LateralDistance, 1e-3f);
        }
    }

    public class OffTrackMonitorTests
    {
        [Test]
        public void WithinTolerance_NoPenalty()
        {
            var m = new OffTrackMonitor(toleranceM: 4f);
            float p = m.Tick(1f, lateralDistance: 3f);

            Assert.AreEqual(0f, p, 1e-3f);
            Assert.IsFalse(m.IsOffTrack);
            Assert.AreEqual(0, m.OffTrackEvents);
        }

        [Test]
        public void BeyondTolerance_AccumulatesPenaltyByExcess()
        {
            var m = new OffTrackMonitor(toleranceM: 4f, penaltyPerSecondPerMeter: 2f);
            // 6m lateral => 2m de excesso => 2m * 2 * 1s = 4 por tick
            float p = m.Tick(1f, 6f);

            Assert.AreEqual(4f, p, 1e-3f);
            Assert.IsTrue(m.IsOffTrack);
            Assert.AreEqual(1, m.OffTrackEvents);
            Assert.AreEqual(1f, m.TimeOffTrackSeconds, 1e-3f);
        }

        [Test]
        public void CountsDistinctOffTrackEvents()
        {
            var m = new OffTrackMonitor(toleranceM: 4f);
            m.Tick(1f, 6f); // sai (evento 1)
            m.Tick(1f, 7f); // continua fora (ainda evento 1)
            m.Tick(1f, 2f); // volta
            m.Tick(1f, 6f); // sai de novo (evento 2)

            Assert.AreEqual(2, m.OffTrackEvents);
            Assert.Greater(m.TotalPenalty, 0f);
        }
    }
}
