using MiningTruckSim.Scoring;
using NUnit.Framework;

namespace MiningTruckSim.Tests
{
    public class OperationBandTests
    {
        [Test]
        public void IsInBand_TrueForNominalTelemetry()
        {
            var band = OperationBand.Default;
            var t = new TruckTelemetry(rpm: 1500f, speedKmh: 25f, engineTempC: 90f, loadRatio: 0.9f);

            Assert.IsTrue(band.IsInBand(t));
            Assert.AreEqual(1f, band.Quality(t), 1e-4f);
        }

        [Test]
        public void IsInBand_FalseWhenOverloaded()
        {
            var band = OperationBand.Default;
            var t = new TruckTelemetry(rpm: 1500f, speedKmh: 25f, engineTempC: 90f, loadRatio: 1.3f);

            Assert.IsFalse(band.IsInBand(t));
            Assert.Less(band.Quality(t), 1f);
        }

        [Test]
        public void IsInBand_FalseWhenOverRpmAndOverheated()
        {
            var band = OperationBand.Default;
            var t = new TruckTelemetry(rpm: 2200f, speedKmh: 25f, engineTempC: 130f, loadRatio: 0.5f);

            Assert.IsFalse(band.IsInBand(t));
        }

        [Test]
        public void Quality_DecaysOutsideRange()
        {
            var band = OperationBand.Default;
            var slightlyOff = new TruckTelemetry(2000f, 25f, 90f, 0.5f); // 100 rpm acima
            var farOff = new TruckTelemetry(3000f, 25f, 90f, 0.5f);

            Assert.Greater(band.Quality(slightlyOff), band.Quality(farOff));
        }
    }

    public class ScoreAccumulatorTests
    {
        [Test]
        public void Tick_AccumulatesScoreAndBandTime_WhenInBand()
        {
            var acc = new ScoreAccumulator(OperationBand.Default, pointsPerSecondInBand: 10f);
            var t = new TruckTelemetry(1500f, 25f, 90f, 0.9f);

            for (int i = 0; i < 10; i++)
            {
                acc.Tick(1f, t); // 10 segundos na faixa
            }

            Assert.AreEqual(100f, acc.Score, 1e-3f);
            Assert.AreEqual(10f, acc.TimeInBandSeconds, 1e-3f);
            Assert.AreEqual(1f, acc.BandRatio, 1e-3f);
        }

        [Test]
        public void ApplyPenalty_ReducesScoreAndCounts_NeverBelowZero()
        {
            var acc = new ScoreAccumulator(OperationBand.Default);
            var t = new TruckTelemetry(1500f, 25f, 90f, 0.9f);
            acc.Tick(1f, t); // +10

            acc.ApplyPenalty(5f);
            Assert.AreEqual(5f, acc.Score, 1e-3f);
            Assert.AreEqual(1, acc.PenaltyCount);

            acc.ApplyPenalty(1000f);
            Assert.AreEqual(0f, acc.Score, 1e-3f);
        }

        [Test]
        public void Tick_OutOfBand_NoBandTimeButPartialScore()
        {
            var acc = new ScoreAccumulator(OperationBand.Default, pointsPerSecondInBand: 10f);
            var overloaded = new TruckTelemetry(1500f, 25f, 90f, 1.25f);

            acc.Tick(1f, overloaded);

            Assert.AreEqual(0f, acc.TimeInBandSeconds, 1e-3f);
            Assert.Greater(acc.Score, 0f);
            Assert.Less(acc.Score, 10f);
        }
    }
}
