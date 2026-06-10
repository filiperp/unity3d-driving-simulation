using MiningTruckSim.Config;
using MiningTruckSim.Scoring;
using NUnit.Framework;

namespace MiningTruckSim.Tests
{
    public class MineCatalogTests
    {
        [Test]
        public void HasTwoMines_EasyAndHard()
        {
            Assert.AreEqual(2, MineCatalog.All.Count);
            Assert.IsFalse(MineCatalog.Easy.IsHard);
            Assert.IsTrue(MineCatalog.Hard.IsHard);
        }

        [Test]
        public void Hard_IsTougherThanEasy()
        {
            Assert.Greater(MineCatalog.Hard.RouteLengthM, MineCatalog.Easy.RouteLengthM);
            Assert.Greater(MineCatalog.Hard.AlertsPerMinute, MineCatalog.Easy.AlertsPerMinute);
            // Tolerância menor = mais difícil ficar no trilho.
            Assert.Less(MineCatalog.Hard.OffTrackToleranceM, MineCatalog.Easy.OffTrackToleranceM);
        }

        [Test]
        public void ById_IsCaseInsensitive_DefaultsToEasy()
        {
            Assert.AreEqual("hard", MineCatalog.ById("HARD").Id);
            Assert.AreEqual("easy", MineCatalog.ById("easy").Id);
            Assert.AreEqual("easy", MineCatalog.ById("inexistente").Id);
        }
    }

    public class OperationConfigTests
    {
        [Test]
        public void Validated_ClampsCyclesToAtLeastOne()
        {
            var cfg = new OperationConfig(MineCatalog.Easy, 0).Validated();
            Assert.AreEqual(1, cfg.Cycles);

            var big = new OperationConfig(MineCatalog.Hard, 999).Validated();
            Assert.AreEqual(20, big.Cycles);
        }
    }

    public class OperationProgressTests
    {
        private static CycleScore Score(int idx, float final, float band) => new CycleScore
        {
            CycleIndex = idx,
            BaseScore = final,
            Penalties = 0f,
            BandRatio = band
        };

        [Test]
        public void RunsNCycles_AggregatesScore_ThenCompletes()
        {
            var progress = new OperationProgress(new OperationConfig(MineCatalog.Hard, 3));
            Assert.AreEqual(3, progress.TotalCycles);
            Assert.IsFalse(progress.IsComplete);

            progress.CompleteCycle(Score(0, 100f, 0.8f));
            progress.CompleteCycle(Score(1, 50f, 0.6f));
            Assert.IsFalse(progress.IsComplete);
            Assert.AreEqual(2, progress.CurrentCycleIndex);
            Assert.AreEqual(2, progress.CompletedCycles);

            progress.CompleteCycle(Score(2, 30f, 0.4f));
            Assert.IsTrue(progress.IsComplete);
            Assert.AreEqual(180f, progress.TotalScore, 1e-3f);
            Assert.AreEqual(0.6f, progress.AverageBandRatio, 1e-3f);
            Assert.AreEqual(3, progress.CompletedCycles);
        }

        [Test]
        public void IgnoresExtraCycles_AfterComplete()
        {
            var progress = new OperationProgress(new OperationConfig(MineCatalog.Easy, 1));
            progress.CompleteCycle(Score(0, 100f, 1f));
            Assert.IsTrue(progress.IsComplete);

            progress.CompleteCycle(Score(1, 999f, 1f)); // deve ser ignorado
            Assert.AreEqual(100f, progress.TotalScore, 1e-3f);
            Assert.AreEqual(1, progress.CompletedCycles);
        }
    }
}
