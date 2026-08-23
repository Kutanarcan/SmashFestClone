using Game.Core.Levels;
using Game.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class GridLevelConverterTests
    {
        private const string Cube = "Cube";
        private const string Prizm = "Prizm";
        private const string Jar = "Jar";
        private const float Tolerance = 0.001f;

        private static readonly Vector3 Unit = new Vector3(2.1f, 2.1f, 2.1f);
        private static readonly Vector3 Origin = new Vector3(-6.3f, 0f, -6.3f);

        private static readonly Vector3 JarOffset = new Vector3(0.4f, 0.9f, -0.3f);

        private CellLayout layout;
        private FakeLevelObjectSizes sizes;

        [SetUp]
        public void SetUp()
        {
            layout = new CellLayout(Unit, Origin);

            sizes = new FakeLevelObjectSizes()
                .Add(Cube, new Vector3(2.1f, 2.1f, 2.1f))
                .Add(Prizm, new Vector3(6.3f, 2.1f, 2.1f))
                .Add(Jar, new Vector3(2.1f, 2.1f, 2.1f), JarOffset);
        }

        private static LevelGrid NewGrid() => new LevelGrid(6, 4, 6);

        private static CellIndex At(int x, int y, int z) => new CellIndex(x, y, z);

        private LevelDefinition Convert(LevelGrid grid) =>
            GridLevelConverter.ToDefinition(grid, layout, sizes, "level_01", 5, null);

        [Test]
        public void ToDefinition_CarriesIdAndBallCount()
        {
            LevelDefinition level = Convert(NewGrid());

            Assert.AreEqual("level_01", level.id);
            Assert.AreEqual(5, level.ballCount);
            Assert.AreEqual(0, level.objects.Length);
        }

        [Test]
        public void ToDefinition_PositionsAnObjectAtItsCellCentre()
        {
            LevelGrid grid = NewGrid();
            grid.TryPlace(Cube, At(0, 0, 0), CellSpan.Single, 0f);

            LevelDefinition level = Convert(grid);

            Assert.AreEqual(1, level.objects.Length);
            Assert.AreEqual(Origin.x + 1.05f, level.objects[0].position.x, Tolerance);
            Assert.AreEqual(1.05f, level.objects[0].position.y, Tolerance);
            Assert.AreEqual(Origin.z + 1.05f, level.objects[0].position.z, Tolerance);
        }

        [Test]
        public void ToDefinition_CentresAWideObjectOnItsWholeBlock()
        {
            LevelGrid grid = NewGrid();
            grid.TryPlace(Prizm, At(0, 0, 0), new CellSpan(3, 1, 1), 0f);

            LevelDefinition level = Convert(grid);

            Assert.AreEqual(Origin.x + 3f * 2.1f * 0.5f, level.objects[0].position.x, Tolerance);
        }

        [Test]
        public void ToDefinition_SubtractsTheColliderCentreOffset()
        {
            LevelGrid grid = NewGrid();
            grid.TryPlace(Jar, At(0, 0, 0), CellSpan.Single, 0f);

            LevelDefinition level = Convert(grid);

            Assert.AreEqual(Origin.x + 1.05f - JarOffset.x, level.objects[0].position.x, Tolerance);
            Assert.AreEqual(1.05f - JarOffset.y, level.objects[0].position.y, Tolerance);
            Assert.AreEqual(Origin.z + 1.05f - JarOffset.z, level.objects[0].position.z, Tolerance);
        }

        [Test]
        public void ToDefinition_WritesYawAsRotation()
        {
            LevelGrid grid = NewGrid();
            grid.TryPlace(Cube, At(1, 0, 1), CellSpan.Single, 90f);

            LevelDefinition level = Convert(grid);

            Assert.AreEqual(90f, level.objects[0].rotation.y, Tolerance);
        }

        [Test]
        public void ToDefinition_SkipsTypesTheCatalogDoesNotKnow()
        {
            LevelGrid grid = NewGrid();
            grid.TryPlace(Cube, At(0, 0, 0), CellSpan.Single, 0f);
            grid.TryPlace("Mystery", At(2, 0, 0), CellSpan.Single, 0f);

            LevelDefinition level = Convert(grid);

            Assert.AreEqual(1, level.objects.Length);
            Assert.AreEqual(Cube, level.objects[0].type);
        }

        [Test]
        public void RoundTrip_PreservesCellsTypesAndSpans()
        {
            LevelGrid source = NewGrid();
            source.TryPlace(Cube, At(0, 0, 0), CellSpan.Single, 0f);
            source.TryPlace(Prizm, At(1, 0, 2), new CellSpan(3, 1, 1), 0f);
            source.TryPlace(Cube, At(0, 1, 0), CellSpan.Single, 0f);

            LevelDefinition level = Convert(source);

            LevelGrid loaded = NewGrid();
            int count = GridLevelConverter.Fill(loaded, level, layout, sizes);

            Assert.AreEqual(3, count);

            Assert.IsTrue(loaded.TryGetAt(At(0, 0, 0), out GridPlacement cube));
            Assert.AreEqual(Cube, cube.Type);

            Assert.IsTrue(loaded.TryGetAt(At(3, 0, 2), out GridPlacement prizm));
            Assert.AreEqual(Prizm, prizm.Type);
            Assert.AreEqual(At(1, 0, 2), prizm.Origin);
            Assert.AreEqual(new CellSpan(3, 1, 1), prizm.Span);

            Assert.IsTrue(loaded.TryGetAt(At(0, 1, 0), out GridPlacement upstairs));
            Assert.AreEqual(Cube, upstairs.Type);
        }

        [Test]
        public void RoundTrip_PreservesOffsetColliders()
        {
            LevelGrid source = NewGrid();
            source.TryPlace(Jar, At(2, 1, 3), CellSpan.Single, 0f);

            LevelGrid loaded = NewGrid();
            GridLevelConverter.Fill(loaded, Convert(source), layout, sizes);

            Assert.IsTrue(loaded.TryGetAt(At(2, 1, 3), out GridPlacement jar));
            Assert.AreEqual(Jar, jar.Type);
        }

        [Test]
        public void RoundTrip_PreservesARotatedWideObject()
        {
            LevelGrid source = NewGrid();
            source.TryPlace(Prizm, At(1, 0, 0), new CellSpan(1, 1, 3), 90f);

            LevelGrid loaded = NewGrid();
            GridLevelConverter.Fill(loaded, Convert(source), layout, sizes);

            Assert.IsTrue(loaded.TryGetAt(At(1, 0, 0), out GridPlacement prizm));
            Assert.AreEqual(new CellSpan(1, 1, 3), prizm.Span);
            Assert.AreEqual(90f, prizm.Yaw, Tolerance);
        }

        [Test]
        public void RoundTrip_SurvivesJsonSerialisation()
        {
            LevelGrid source = NewGrid();
            source.TryPlace(Prizm, At(1, 0, 2), new CellSpan(3, 1, 1), 0f);
            source.TryPlace(Jar, At(0, 0, 0), CellSpan.Single, 0f);

            string json = LevelSerializer.ToJson(Convert(source));

            Assert.IsTrue(LevelSerializer.TryFromJson(json, out LevelDefinition parsed, out string error), error);

            LevelGrid loaded = NewGrid();
            Assert.AreEqual(2, GridLevelConverter.Fill(loaded, parsed, layout, sizes));

            Assert.IsTrue(loaded.TryGetAt(At(1, 0, 2), out GridPlacement prizm));
            Assert.AreEqual(Prizm, prizm.Type);
            Assert.IsTrue(loaded.TryGetAt(At(0, 0, 0), out GridPlacement jar));
            Assert.AreEqual(Jar, jar.Type);
        }

        [Test]
        public void Fill_SkipsUnknownTypesAndReportsHowManyLoaded()
        {
            var level = new LevelDefinition
            {
                ballCount = 3,
                objects = new[]
                {
                    new PlacedObject(Cube, new Vector3(Origin.x + 1.05f, 1.05f, Origin.z + 1.05f), Vector3.zero),
                    new PlacedObject("Mystery", Vector3.zero, Vector3.zero)
                }
            };

            LevelGrid grid = NewGrid();

            Assert.AreEqual(1, GridLevelConverter.Fill(grid, level, layout, sizes));
            Assert.AreEqual(1, grid.Count);
        }

        [Test]
        public void Fill_SkipsObjectsThatFallOutsideTheGrid()
        {
            var level = new LevelDefinition
            {
                ballCount = 3,
                objects = new[]
                {
                    new PlacedObject(Cube, new Vector3(9999f, 0f, 9999f), Vector3.zero)
                }
            };

            LevelGrid grid = NewGrid();

            Assert.AreEqual(0, GridLevelConverter.Fill(grid, level, layout, sizes));
        }

        [Test]
        public void NullArguments_Throw()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => GridLevelConverter.ToDefinition(null, layout, sizes, "x", 1, null));
            Assert.Throws<System.ArgumentNullException>(
                () => GridLevelConverter.ToDefinition(NewGrid(), layout, null, "x", 1, null));
            Assert.Throws<System.ArgumentNullException>(
                () => GridLevelConverter.Fill(null, new LevelDefinition(), layout, sizes));
            Assert.Throws<System.ArgumentNullException>(
                () => GridLevelConverter.Fill(NewGrid(), null, layout, sizes));
        }
    }
}
