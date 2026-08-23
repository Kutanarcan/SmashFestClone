using Game.Core.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class GridSpaceTests
    {
        private const float Tolerance = 0.001f;

        private static readonly Vector3 Unit = new Vector3(2.1f, 2.1f, 2.1f);

        [Test]
        public void SpanFor_AUnitSizedObject_IsOneCell()
        {
            CellSpan span = GridSpace.SpanFor(new Vector3(2.1f, 2.1f, 2.1f), Unit);

            Assert.AreEqual(CellSpan.Single, span);
        }

        [Test]
        public void SpanFor_ATripleWidthObject_IsThreeCellsWide()
        {
            CellSpan span = GridSpace.SpanFor(new Vector3(6.3f, 2.1f, 2.1f), Unit);

            Assert.AreEqual(3, span.Width);
            Assert.AreEqual(1, span.Height);
            Assert.AreEqual(1, span.Depth);
        }

        [Test]
        public void SpanFor_RoundsToTheNearestCell()
        {
            CellSpan span = GridSpace.SpanFor(new Vector3(6.0f, 2.1f, 2.1f), Unit);

            Assert.AreEqual(3, span.Width);
        }

        [Test]
        public void SpanFor_IsNeverZero()
        {
            CellSpan span = GridSpace.SpanFor(new Vector3(0.01f, 0.01f, 0.01f), Unit);

            Assert.AreEqual(CellSpan.Single, span);
        }

        [Test]
        public void SpanFor_AZeroUnit_FallsBackToOneCell()
        {
            CellSpan span = GridSpace.SpanFor(new Vector3(6.3f, 2.1f, 2.1f), Vector3.zero);

            Assert.AreEqual(CellSpan.Single, span);
        }

        [Test]
        public void CellToWorldCenter_OfTheFirstCell_IsHalfAUnitIn()
        {
            Vector3 center = GridSpace.CellToWorldCenter(
                new CellIndex(0, 0, 0), CellSpan.Single, Unit, Vector3.zero);

            Assert.AreEqual(1.05f, center.x, Tolerance);
            Assert.AreEqual(1.05f, center.y, Tolerance);
            Assert.AreEqual(1.05f, center.z, Tolerance);
        }

        [Test]
        public void CellToWorldCenter_StepsOneUnitPerCell()
        {
            Vector3 center = GridSpace.CellToWorldCenter(
                new CellIndex(2, 1, 3), CellSpan.Single, Unit, Vector3.zero);

            Assert.AreEqual(2f * 2.1f + 1.05f, center.x, Tolerance);
            Assert.AreEqual(1f * 2.1f + 1.05f, center.y, Tolerance);
            Assert.AreEqual(3f * 2.1f + 1.05f, center.z, Tolerance);
        }

        [Test]
        public void CellToWorldCenter_OfAMultiCellObject_CentresOnTheWholeBlock()
        {
            Vector3 center = GridSpace.CellToWorldCenter(
                new CellIndex(0, 0, 0), new CellSpan(3, 1, 1), Unit, Vector3.zero);

            Assert.AreEqual(3f * 2.1f * 0.5f, center.x, Tolerance);
            Assert.AreEqual(1.05f, center.z, Tolerance);
        }

        [Test]
        public void CellToWorldCenter_RespectsTheGridOrigin()
        {
            var origin = new Vector3(-10f, 5f, 20f);

            Vector3 center = GridSpace.CellToWorldCenter(
                new CellIndex(0, 0, 0), CellSpan.Single, Unit, origin);

            Assert.AreEqual(-10f + 1.05f, center.x, Tolerance);
            Assert.AreEqual(5f + 1.05f, center.y, Tolerance);
            Assert.AreEqual(20f + 1.05f, center.z, Tolerance);
        }

        [Test]
        public void WorldToCell_RoundTripsThroughCellToWorld()
        {
            var origin = new Vector3(-10f, 0f, 20f);
            var cell = new CellIndex(4, 2, 1);

            Vector3 center = GridSpace.CellToWorldCenter(cell, CellSpan.Single, Unit, origin);
            CellIndex back = GridSpace.WorldToCell(center, Unit, origin);

            Assert.AreEqual(cell, back);
        }

        [Test]
        public void WorldToCell_FloorsSoAnyPointInACellMapsToIt()
        {
            CellIndex low = GridSpace.WorldToCell(new Vector3(0.01f, 0.01f, 0.01f), Unit, Vector3.zero);
            CellIndex high = GridSpace.WorldToCell(new Vector3(2.09f, 2.09f, 2.09f), Unit, Vector3.zero);

            Assert.AreEqual(new CellIndex(0, 0, 0), low);
            Assert.AreEqual(new CellIndex(0, 0, 0), high);
        }

        [Test]
        public void WorldAlignedSize_IdentityRotation_IsUnchanged()
        {
            var size = new Vector3(6.3f, 2.1f, 2.1f);

            Assert.AreEqual(size, GridSpace.WorldAlignedSize(size, Quaternion.identity));
        }

        [Test]
        public void WorldAlignedSize_NinetyAboutY_SwapsWidthAndDepth()
        {
            var size = new Vector3(6.3f, 2.1f, 2.1f);

            Vector3 rotated = GridSpace.WorldAlignedSize(size, Quaternion.Euler(0f, 90f, 0f));

            Assert.AreEqual(2.1f, rotated.x, Tolerance);
            Assert.AreEqual(2.1f, rotated.y, Tolerance);
            Assert.AreEqual(6.3f, rotated.z, Tolerance);
        }

        [Test]
        public void WorldAlignedSize_IsNeverNegative()
        {
            Vector3 rotated = GridSpace.WorldAlignedSize(
                new Vector3(6.3f, 2.1f, 2.1f), Quaternion.Euler(0f, 270f, 0f));

            Assert.GreaterOrEqual(rotated.x, 0f);
            Assert.GreaterOrEqual(rotated.z, 0f);
        }

        [Test]
        public void RotatedPrizm_SpansThreeCellsDeepInsteadOfWide()
        {
            var size = new Vector3(6.3f, 2.1f, 2.1f);

            CellSpan unrotated = GridSpace.SpanFor(size, Unit);
            CellSpan turned = GridSpace.SpanFor(
                GridSpace.WorldAlignedSize(size, Quaternion.Euler(0f, 90f, 0f)), Unit);

            Assert.AreEqual(3, unrotated.Width);
            Assert.AreEqual(1, unrotated.Depth);

            Assert.AreEqual(1, turned.Width);
            Assert.AreEqual(3, turned.Depth);
        }

        [Test]
        public void GridSizeFor_FloorsThePlatformToWholeCells()
        {
            Vector2Int size = GridSpace.GridSizeFor(new Vector2(8f, 6f), new Vector2(2.1f, 2.1f));

            Assert.AreEqual(3, size.x);
            Assert.AreEqual(2, size.y);
        }

        [Test]
        public void GridSizeFor_IsNeverZero()
        {
            Vector2Int size = GridSpace.GridSizeFor(new Vector2(0.5f, 0.5f), new Vector2(2.1f, 2.1f));

            Assert.AreEqual(1, size.x);
            Assert.AreEqual(1, size.y);
        }
    }
}
