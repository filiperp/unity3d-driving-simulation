using MiningTruckSim.Scoring;
using NUnit.Framework;

namespace MiningTruckSim.Tests
{
    public class CycleScoreTests
    {
        [Test]
        public void Final_IsBaseMinusPenalties_NeverNegative()
        {
            var s = new CycleScore { BaseScore = 100f, Penalties = 30f };
            Assert.AreEqual(70f, s.Final, 1e-3f);

            var drained = new CycleScore { BaseScore = 20f, Penalties = 50f };
            Assert.AreEqual(0f, drained.Final, 1e-3f);
        }

        [Test]
        public void Rating_S_RequiresCleanRunAndHighBand()
        {
            var s = new CycleScore { BandRatio = 0.95f, OffTrackEvents = 0 };
            Assert.AreEqual("S", s.Rating);

            // mesma banda mas com saída do trilho => não é S
            var withOffTrack = new CycleScore { BandRatio = 0.95f, OffTrackEvents = 1 };
            Assert.AreEqual("A", withOffTrack.Rating);
        }

        [Test]
        public void Rating_DegradesWithBandRatio()
        {
            Assert.AreEqual("A", new CycleScore { BandRatio = 0.8f }.Rating);
            Assert.AreEqual("B", new CycleScore { BandRatio = 0.6f }.Rating);
            Assert.AreEqual("C", new CycleScore { BandRatio = 0.3f }.Rating);
            Assert.AreEqual("D", new CycleScore { BandRatio = 0.1f }.Rating);
        }
    }
}
