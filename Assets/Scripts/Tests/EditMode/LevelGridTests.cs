using Game.Core.Levels;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelGridTests
    {
        private const string Cube = "Cube";
        private const string Prizm = "Prizm";

        private static readonly CellSpan Wide3 = new CellSpan(3, 1, 1);
        private static readonly CellSpan Tall2 = new CellSpan(1, 2, 1);

        private LevelGrid grid;

        [SetUp]
        public void SetUp()
        {
            grid = new LevelGrid(6, 4, 6);
        }

        private static CellIndex At(int x, int y, int z) => new CellIndex(x, y, z);

        [Test]
        public void NewGrid_IsEmpty()
        {
            Assert.AreEqual(0, grid.Count);
            Assert.IsFalse(grid.TryGetAt(At(0, 0, 0), out _));
        }

        [Test]
        public void Placing_RecordsTheObject()
        {
            Assert.IsTrue(grid.TryPlace(Cube, At(1, 0, 2), CellSpan.Single, 0f));

            Assert.AreEqual(1, grid.Count);
            Assert.IsTrue(grid.TryGetAt(At(1, 0, 2), out GridPlacement placed));
            Assert.AreEqual(Cube, placed.Type);
        }

        [Test]
        public void MultiCellObject_OccupiesEveryCellItCovers()
        {
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);

            Assert.IsTrue(grid.TryGetAt(At(1, 0, 0), out _));
            Assert.IsTrue(grid.TryGetAt(At(2, 0, 0), out _));
            Assert.IsTrue(grid.TryGetAt(At(3, 0, 0), out _));
            Assert.IsFalse(grid.TryGetAt(At(4, 0, 0), out _));
        }

        [Test]
        public void MultiCellObject_IsFoundFromAnyOfItsCells()
        {
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);

            Assert.IsTrue(grid.TryGetAt(At(3, 0, 0), out GridPlacement found));

            Assert.AreEqual(Prizm, found.Type);
            Assert.AreEqual(At(1, 0, 0), found.Origin);
        }

        [Test]
        public void CannotPlaceOnAnOccupiedCell()
        {
            grid.TryPlace(Cube, At(2, 0, 2), CellSpan.Single, 0f);

            Assert.IsFalse(grid.TryPlace(Cube, At(2, 0, 2), CellSpan.Single, 0f));
            Assert.AreEqual(1, grid.Count);
        }

        [Test]
        public void CannotPlaceOverlappingAMultiCellObject()
        {
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);

            Assert.IsFalse(grid.TryPlace(Cube, At(2, 0, 0), CellSpan.Single, 0f));
            Assert.IsFalse(grid.TryPlace(Cube, At(3, 0, 0), CellSpan.Single, 0f));
            Assert.IsTrue(grid.TryPlace(Cube, At(4, 0, 0), CellSpan.Single, 0f));
        }

        [Test]
        public void CannotPlaceOutsideTheGrid()
        {
            Assert.IsFalse(grid.TryPlace(Cube, At(-1, 0, 0), CellSpan.Single, 0f));
            Assert.IsFalse(grid.TryPlace(Cube, At(6, 0, 0), CellSpan.Single, 0f));
            Assert.IsFalse(grid.TryPlace(Cube, At(0, 4, 0), CellSpan.Single, 0f));
            Assert.IsFalse(grid.TryPlace(Cube, At(0, 0, 6), CellSpan.Single, 0f));
        }

        [Test]
        public void CannotPlaceWhenTheSpanRunsOffTheEdge()
        {
            Assert.IsFalse(grid.TryPlace(Prizm, At(4, 0, 0), Wide3, 0f));
            Assert.IsTrue(grid.TryPlace(Prizm, At(3, 0, 0), Wide3, 0f));
        }

        [Test]
        public void SameCellOnDifferentLevels_DoesNotCollide()
        {
            Assert.IsTrue(grid.TryPlace(Cube, At(2, 0, 2), CellSpan.Single, 0f));
            Assert.IsTrue(grid.TryPlace(Cube, At(2, 1, 2), CellSpan.Single, 0f));

            Assert.AreEqual(2, grid.Count);
        }

        [Test]
        public void TallObject_BlocksTheLevelAbove()
        {
            grid.TryPlace(Cube, At(2, 0, 2), Tall2, 0f);

            Assert.IsFalse(grid.TryPlace(Cube, At(2, 1, 2), CellSpan.Single, 0f));
            Assert.IsTrue(grid.TryPlace(Cube, At(2, 2, 2), CellSpan.Single, 0f));
        }

        [Test]
        public void EmptyType_IsRejected()
        {
            Assert.IsFalse(grid.TryPlace(string.Empty, At(0, 0, 0), CellSpan.Single, 0f));
            Assert.IsFalse(grid.TryPlace(null, At(0, 0, 0), CellSpan.Single, 0f));
            Assert.AreEqual(0, grid.Count);
        }

        [Test]
        public void Removing_FreesEveryCellOfTheSpan()
        {
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);

            Assert.IsTrue(grid.TryRemoveAt(At(1, 0, 0)));

            Assert.AreEqual(0, grid.Count);
            Assert.IsTrue(grid.TryPlace(Cube, At(2, 0, 0), CellSpan.Single, 0f));
        }

        [Test]
        public void Removing_WorksFromAnyCellOfTheSpan()
        {
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);

            Assert.IsTrue(grid.TryRemoveAt(At(3, 0, 0)));

            Assert.AreEqual(0, grid.Count);
        }

        [Test]
        public void RemovingFromAnEmptyCell_ReturnsFalse()
        {
            Assert.IsFalse(grid.TryRemoveAt(At(0, 0, 0)));
            Assert.IsFalse(grid.TryRemoveAt(At(99, 0, 0)));
        }

        [Test]
        public void RemovingOne_LeavesTheOthersFindable()
        {
            grid.TryPlace(Cube, At(0, 0, 0), CellSpan.Single, 0f);
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);
            grid.TryPlace(Cube, At(5, 0, 0), CellSpan.Single, 0f);

            Assert.IsTrue(grid.TryRemoveAt(At(0, 0, 0)));

            Assert.AreEqual(2, grid.Count);

            Assert.IsTrue(grid.TryGetAt(At(2, 0, 0), out GridPlacement prizm));
            Assert.AreEqual(Prizm, prizm.Type);
            Assert.AreEqual(At(1, 0, 0), prizm.Origin);

            Assert.IsTrue(grid.TryGetAt(At(5, 0, 0), out GridPlacement cube));
            Assert.AreEqual(Cube, cube.Type);
        }

        [Test]
        public void RemovingRepeatedly_KeepsTheGridConsistent()
        {
            grid.TryPlace(Cube, At(0, 0, 0), CellSpan.Single, 0f);
            grid.TryPlace(Cube, At(1, 0, 0), CellSpan.Single, 0f);
            grid.TryPlace(Cube, At(2, 0, 0), CellSpan.Single, 0f);

            Assert.IsTrue(grid.TryRemoveAt(At(1, 0, 0)));
            Assert.IsTrue(grid.TryRemoveAt(At(0, 0, 0)));

            Assert.AreEqual(1, grid.Count);
            Assert.IsTrue(grid.TryGetAt(At(2, 0, 0), out GridPlacement remaining));
            Assert.AreEqual(At(2, 0, 0), remaining.Origin);
        }

        [Test]
        public void Placing_PreservesYaw()
        {
            grid.TryPlace(Cube, At(0, 0, 0), CellSpan.Single, 90f);

            Assert.IsTrue(grid.TryGetAt(At(0, 0, 0), out GridPlacement placed));
            Assert.AreEqual(90f, placed.Yaw, 0.001f);
        }

        [Test]
        public void Rotating_ASingleCellObject_ChangesOnlyItsYaw()
        {
            grid.TryPlace(Cube, At(2, 0, 2), CellSpan.Single, 0f);

            Assert.IsTrue(grid.TryRotateAt(At(2, 0, 2), CellSpan.Single, 90f));

            Assert.IsTrue(grid.TryGetAt(At(2, 0, 2), out GridPlacement rotated));
            Assert.AreEqual(90f, rotated.Yaw, 0.001f);
            Assert.AreEqual(1, grid.Count);
        }

        [Test]
        public void Rotating_AWideObject_RespansItDeep()
        {
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);

            Assert.IsTrue(grid.TryRotateAt(At(1, 0, 0), new CellSpan(1, 1, 3), 90f));

            Assert.IsTrue(grid.TryGetAt(At(1, 0, 2), out GridPlacement rotated));
            Assert.AreEqual(Prizm, rotated.Type);
            Assert.IsFalse(grid.TryGetAt(At(2, 0, 0), out _));
        }

        [Test]
        public void Rotating_WorksFromAnyCellOfTheSpan()
        {
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);

            Assert.IsTrue(grid.TryRotateAt(At(3, 0, 0), new CellSpan(1, 1, 3), 90f));

            Assert.IsTrue(grid.TryGetAt(At(1, 0, 0), out GridPlacement rotated));
            Assert.AreEqual(90f, rotated.Yaw, 0.001f);
        }

        [Test]
        public void Rotating_IsRejectedWhenTheNewSpanRunsOffTheEdge()
        {
            grid.TryPlace(Prizm, At(0, 0, 4), Wide3, 0f);

            Assert.IsFalse(grid.TryRotateAt(At(0, 0, 4), new CellSpan(1, 1, 3), 90f));

            Assert.IsTrue(grid.TryGetAt(At(2, 0, 4), out GridPlacement unchanged));
            Assert.AreEqual(Wide3, unchanged.Span);
            Assert.AreEqual(0f, unchanged.Yaw, 0.001f);
        }

        [Test]
        public void Rotating_IsRejectedWhenTheNewSpanWouldCollide()
        {
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);
            grid.TryPlace(Cube, At(1, 0, 1), CellSpan.Single, 0f);

            Assert.IsFalse(grid.TryRotateAt(At(1, 0, 0), new CellSpan(1, 1, 3), 90f));

            Assert.AreEqual(2, grid.Count);
            Assert.IsTrue(grid.TryGetAt(At(3, 0, 0), out GridPlacement prizm));
            Assert.AreEqual(Wide3, prizm.Span);
            Assert.IsTrue(grid.TryGetAt(At(1, 0, 1), out GridPlacement cube));
            Assert.AreEqual(Cube, cube.Type);
        }

        [Test]
        public void Rotating_AnEmptyCell_ReturnsFalse()
        {
            Assert.IsFalse(grid.TryRotateAt(At(0, 0, 0), CellSpan.Single, 90f));
            Assert.IsFalse(grid.TryRotateAt(At(99, 0, 0), CellSpan.Single, 90f));
        }

        [Test]
        public void Clear_EmptiesEverything()
        {
            grid.TryPlace(Cube, At(0, 0, 0), CellSpan.Single, 0f);
            grid.TryPlace(Prizm, At(1, 0, 0), Wide3, 0f);

            grid.Clear();

            Assert.AreEqual(0, grid.Count);
            Assert.IsTrue(grid.TryPlace(Cube, At(1, 0, 0), CellSpan.Single, 0f));
        }
    }
}
