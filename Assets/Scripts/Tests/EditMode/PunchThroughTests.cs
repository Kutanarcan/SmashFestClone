using Game.Core.Impacts;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class PunchThroughTests
    {
        private static readonly Vector3 Incoming = new Vector3(0f, 0f, 30f);

        [Test]
        public void BreakableHit_RetainsTheConfiguredFraction()
        {
            var punchThrough = new PunchThrough(0.9f);

            Assert.IsTrue(punchThrough.TryResolve(Incoming, true, out Vector3 velocity));
            Assert.AreEqual(27f, velocity.z, 0.001f);
        }

        [Test]
        public void NonBreakableHit_ReturnsFalseSoPhysicsIsLeftAlone()
        {
            var punchThrough = new PunchThrough(0.9f);

            Assert.IsFalse(punchThrough.TryResolve(Incoming, false, out Vector3 _));
        }

        [Test]
        public void FullRetention_LeavesVelocityUnchanged()
        {
            var punchThrough = new PunchThrough(1f);

            Assert.IsTrue(punchThrough.TryResolve(Incoming, true, out Vector3 velocity));
            Assert.AreEqual(Incoming, velocity);
        }

        [Test]
        public void ZeroRetention_StopsTheBall()
        {
            var punchThrough = new PunchThrough(0f);

            Assert.IsTrue(punchThrough.TryResolve(Incoming, true, out Vector3 velocity));
            Assert.AreEqual(Vector3.zero, velocity);
        }

        [Test]
        public void RetentionIsClampedToZeroOne()
        {
            var over = new PunchThrough(5f);
            var under = new PunchThrough(-5f);

            over.TryResolve(Incoming, true, out Vector3 clampedHigh);
            under.TryResolve(Incoming, true, out Vector3 clampedLow);

            Assert.AreEqual(Incoming, clampedHigh);
            Assert.AreEqual(Vector3.zero, clampedLow);
        }
    }
}
