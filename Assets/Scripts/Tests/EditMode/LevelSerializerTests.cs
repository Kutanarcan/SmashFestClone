using Game.Core.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class LevelSerializerTests
    {
        private const float Tolerance = 0.001f;

        private static LevelDefinition SampleLevel()
        {
            return new LevelDefinition
            {
                id = "level_01",
                ballCount = 5,
                platforms = new[]
                {
                    new PlatformSpec(new Vector3(0f, 0f, 12f), new Vector3(8f, 0.5f, 6f)),
                    new PlatformSpec(new Vector3(0f, 0f, 22f), new Vector3(9f, 0.5f, 9f))
                },
                objects = new[]
                {
                    new PlacedObject("Cube", new Vector3(0f, 1.25f, 12f), new Vector3(0f, 45f, 0f)),
                    new PlacedObject("Jar", new Vector3(1.5f, 1.25f, 12f), Vector3.zero),
                    new PlacedObject("Prizm", new Vector3(-1.5f, 1.25f, 12f), new Vector3(0f, 90f, 0f))
                }
            };
        }

        private static LevelDefinition RoundTrip(LevelDefinition level)
        {
            string json = LevelSerializer.ToJson(level);

            Assert.IsTrue(
                LevelSerializer.TryFromJson(json, out LevelDefinition parsed, out string error),
                $"Round-trip failed: {error}");

            return parsed;
        }

        [Test]
        public void RoundTrip_PreservesIdAndBallCount()
        {
            LevelDefinition parsed = RoundTrip(SampleLevel());

            Assert.AreEqual("level_01", parsed.id);
            Assert.AreEqual(5, parsed.ballCount);
        }

        [Test]
        public void RoundTrip_PreservesPlatformPositionsAndSizes()
        {
            LevelDefinition parsed = RoundTrip(SampleLevel());

            Assert.AreEqual(2, parsed.platforms.Length);

            Assert.AreEqual(new Vector3(0f, 0f, 12f), parsed.platforms[0].position);
            Assert.AreEqual(new Vector3(8f, 0.5f, 6f), parsed.platforms[0].size);

            Assert.AreEqual(new Vector3(0f, 0f, 22f), parsed.platforms[1].position);
            Assert.AreEqual(new Vector3(9f, 0.5f, 9f), parsed.platforms[1].size);
        }

        [Test]
        public void RoundTrip_PreservesObjectTypesPositionsAndRotations()
        {
            LevelDefinition parsed = RoundTrip(SampleLevel());

            Assert.AreEqual(3, parsed.objects.Length);

            Assert.AreEqual("Cube", parsed.objects[0].type);
            Assert.AreEqual(new Vector3(0f, 1.25f, 12f), parsed.objects[0].position);
            Assert.AreEqual(45f, parsed.objects[0].rotation.y, Tolerance);

            Assert.AreEqual("Jar", parsed.objects[1].type);
            Assert.AreEqual(new Vector3(1.5f, 1.25f, 12f), parsed.objects[1].position);
        }

        [Test]
        public void RoundTrip_PreservesObjectOrder()
        {
            LevelDefinition parsed = RoundTrip(SampleLevel());

            Assert.AreEqual("Cube", parsed.objects[0].type);
            Assert.AreEqual("Jar", parsed.objects[1].type);
            Assert.AreEqual("Prizm", parsed.objects[2].type);
        }

        [Test]
        public void EmptyLevel_RoundTrips()
        {
            var empty = new LevelDefinition { id = "empty", ballCount = 0 };

            LevelDefinition parsed = RoundTrip(empty);

            Assert.AreEqual("empty", parsed.id);
            Assert.AreEqual(0, parsed.platforms.Length);
            Assert.AreEqual(0, parsed.objects.Length);
        }

        [Test]
        public void ToJson_StampsTheCurrentVersion()
        {
            var level = new LevelDefinition { id = "unstamped" };

            LevelSerializer.ToJson(level);

            Assert.AreEqual(LevelDefinition.CurrentVersion, level.version);
        }

        [Test]
        public void ParsedLevel_NeverHasNullArrays()
        {
            string json = "{\"version\":1,\"id\":\"bare\",\"ballCount\":3}";

            Assert.IsTrue(LevelSerializer.TryFromJson(json, out LevelDefinition parsed, out _));

            Assert.IsNotNull(parsed.platforms);
            Assert.IsNotNull(parsed.objects);
            Assert.AreEqual(0, parsed.platforms.Length);
            Assert.AreEqual(0, parsed.objects.Length);
        }

        [Test]
        public void NullJson_Fails()
        {
            Assert.IsFalse(LevelSerializer.TryFromJson(null, out _, out string error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void WhitespaceJson_Fails()
        {
            Assert.IsFalse(LevelSerializer.TryFromJson("   ", out _, out string error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void MalformedJson_FailsInsteadOfThrowing()
        {
            Assert.IsFalse(
                LevelSerializer.TryFromJson("{ this is not json", out _, out string error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void EmptyJsonObject_FailsBecauseVersionIsAbsent()
        {
            Assert.IsFalse(LevelSerializer.TryFromJson("{}", out _, out string error));
            StringAssert.Contains("version", error.ToLowerInvariant());
        }

        [Test]
        public void UnsupportedVersion_Fails()
        {
            string json = "{\"version\":99,\"id\":\"future\",\"ballCount\":1}";

            Assert.IsFalse(LevelSerializer.TryFromJson(json, out _, out string error));
            StringAssert.Contains("99", error);
        }

        [Test]
        public void NegativeBallCount_Fails()
        {
            string json = "{\"version\":1,\"id\":\"bad\",\"ballCount\":-1}";

            Assert.IsFalse(LevelSerializer.TryFromJson(json, out _, out string error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void ObjectWithoutType_Fails()
        {
            var level = new LevelDefinition
            {
                id = "bad",
                ballCount = 1,
                objects = new[] { new PlacedObject(string.Empty, Vector3.zero, Vector3.zero) }
            };

            string json = LevelSerializer.ToJson(level);

            Assert.IsFalse(LevelSerializer.TryFromJson(json, out _, out string error));
            StringAssert.Contains("type", error.ToLowerInvariant());
        }

        [Test]
        public void PlatformWithNonPositiveSize_Fails()
        {
            var level = new LevelDefinition
            {
                id = "bad",
                ballCount = 1,
                platforms = new[] { new PlatformSpec(Vector3.zero, new Vector3(8f, 0f, 6f)) }
            };

            string json = LevelSerializer.ToJson(level);

            Assert.IsFalse(LevelSerializer.TryFromJson(json, out _, out string error));
            StringAssert.Contains("size", error.ToLowerInvariant());
        }

        [Test]
        public void ToJson_RejectsNullLevel()
        {
            Assert.Throws<System.ArgumentNullException>(() => LevelSerializer.ToJson(null));
        }
    }
}
