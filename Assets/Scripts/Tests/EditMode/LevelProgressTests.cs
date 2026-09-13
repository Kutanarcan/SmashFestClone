using Game.Core.Levels;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelProgressTests
    {
        [Test]
        public void StartsOnTheFirstLevel()
        {
            var progress = new LevelProgress(3);

            Assert.AreEqual(0, progress.CurrentIndex);
            Assert.AreEqual(3, progress.Count);
            Assert.IsTrue(progress.HasLevels);
        }

        [Test]
        public void AdvancingWalksThroughEveryLevel()
        {
            var progress = new LevelProgress(3);

            Assert.IsTrue(progress.TryAdvance());
            Assert.AreEqual(1, progress.CurrentIndex);

            Assert.IsTrue(progress.TryAdvance());
            Assert.AreEqual(2, progress.CurrentIndex);
        }

        [Test]
        public void AdvancingPastTheLastLevel_Fails_AndStaysPut()
        {
            var progress = new LevelProgress(2);
            progress.TryAdvance();

            Assert.IsFalse(progress.TryAdvance());
            Assert.AreEqual(1, progress.CurrentIndex);
        }

        [Test]
        public void IsOnLastLevel_IsTrueOnlyAtTheEnd()
        {
            var progress = new LevelProgress(2);

            Assert.IsFalse(progress.IsOnLastLevel);

            progress.TryAdvance();

            Assert.IsTrue(progress.IsOnLastLevel);
        }

        [Test]
        public void GoTo_ClampsToTheAvailableRange()
        {
            var progress = new LevelProgress(3);

            progress.GoTo(99);
            Assert.AreEqual(2, progress.CurrentIndex);

            progress.GoTo(-5);
            Assert.AreEqual(0, progress.CurrentIndex);
        }

        [Test]
        public void Reset_ReturnsToTheFirstLevel()
        {
            var progress = new LevelProgress(3);
            progress.GoTo(2);

            progress.Reset();

            Assert.AreEqual(0, progress.CurrentIndex);
        }

        [Test]
        public void AnEmptyProgress_HasNoLevelsAndCannotAdvance()
        {
            var progress = new LevelProgress(0);

            Assert.IsFalse(progress.HasLevels);
            Assert.IsFalse(progress.HasNext);
            Assert.IsFalse(progress.IsOnLastLevel);
            Assert.IsFalse(progress.TryAdvance());
            Assert.AreEqual(0, progress.CurrentIndex);
        }

        [Test]
        public void ANegativeCount_IsTreatedAsEmpty()
        {
            var progress = new LevelProgress(-4);

            Assert.AreEqual(0, progress.Count);
            Assert.IsFalse(progress.HasLevels);
        }

        [Test]
        public void ASingleLevel_IsImmediatelyTheLast()
        {
            var progress = new LevelProgress(1);

            Assert.IsTrue(progress.IsOnLastLevel);
            Assert.IsFalse(progress.HasNext);
        }
    }
}
