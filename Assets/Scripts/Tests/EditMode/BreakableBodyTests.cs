using Game.Core.Breakables;
using Game.Core.Impacts;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class BreakableBodyTests
    {
        private const float MaxHealth = 10f;
        private const float DamagePerSpeedUnit = 1f;
        private const float BallMinSpeed = 2f;
        private const float GroundMinSpeed = 5f;
        private const float Tolerance = 0.001f;

        private static BreakableSettings JarLike() => new BreakableSettings(
            MaxHealth, DamagePerSpeedUnit, BallMinSpeed, GroundMinSpeed, BreakableSettings.Immune);

        private static BreakableSettings CubeLike() => new BreakableSettings(
            MaxHealth, DamagePerSpeedUnit, BreakableSettings.Immune, GroundMinSpeed, BreakableSettings.Immune);

        private static ImpactEvent Impact(ImpactSource source, float speed) =>
            new ImpactEvent(source, speed, Vector3.zero, Vector3.up);

        [Test]
        public void StartsAtFullHealthAndUnbroken()
        {
            var body = new BreakableBody(JarLike());

            Assert.AreEqual(MaxHealth, body.Health, Tolerance);
            Assert.IsFalse(body.IsBroken);
        }

        [Test]
        public void ImpactBelowThreshold_DealsNoDamage()
        {
            var body = new BreakableBody(JarLike());

            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed - 0.5f));

            Assert.AreEqual(MaxHealth, body.Health, Tolerance);
        }

        [Test]
        public void ImpactExactlyAtThreshold_DealsNoDamage()
        {
            var body = new BreakableBody(JarLike());

            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed));

            Assert.AreEqual(MaxHealth, body.Health, Tolerance);
        }

        [Test]
        public void ImpactAboveThreshold_DamagesByTheExcessSpeed()
        {
            var body = new BreakableBody(JarLike());

            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed + 3f));

            Assert.AreEqual(MaxHealth - 3f, body.Health, Tolerance);
            Assert.IsFalse(body.IsBroken);
        }

        [Test]
        public void AccumulatedDamage_EventuallyBreaks()
        {
            var body = new BreakableBody(JarLike());

            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed + 6f));
            Assert.IsFalse(body.IsBroken);

            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed + 6f));
            Assert.IsTrue(body.IsBroken);
        }

        [Test]
        public void Breaking_RaisesBrokeExactlyOnce()
        {
            var body = new BreakableBody(JarLike());
            int raised = 0;
            body.Broke += (in ImpactEvent _) => raised++;

            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed + MaxHealth));
            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed + MaxHealth));

            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Broke_CarriesTheImpactThatCausedIt()
        {
            var body = new BreakableBody(JarLike());
            ImpactEvent captured = default;
            body.Broke += (in ImpactEvent impact) => captured = impact;

            var lethal = new ImpactEvent(
                ImpactSource.Ball, BallMinSpeed + MaxHealth, new Vector3(1f, 2f, 3f), Vector3.left);
            body.TakeImpact(lethal);

            Assert.AreEqual(ImpactSource.Ball, captured.Source);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), captured.Point);
            Assert.AreEqual(Vector3.left, captured.Normal);
        }

        [Test]
        public void HealthNeverGoesBelowZero()
        {
            var body = new BreakableBody(JarLike());

            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed + MaxHealth * 10f));

            Assert.AreEqual(0f, body.Health, Tolerance);
        }

        [Test]
        public void BrokenBody_IgnoresFurtherImpacts()
        {
            var body = new BreakableBody(JarLike());
            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed + MaxHealth));

            body.TakeImpact(Impact(ImpactSource.Ground, GroundMinSpeed + 100f));

            Assert.AreEqual(0f, body.Health, Tolerance);
        }

        [Test]
        public void ImmuneSource_DealsNoDamageAtAnySpeed()
        {
            var body = new BreakableBody(CubeLike());

            body.TakeImpact(Impact(ImpactSource.Ball, 10000f));

            Assert.AreEqual(MaxHealth, body.Health, Tolerance);
            Assert.IsFalse(body.IsBroken);
        }

        [Test]
        public void CubeLike_IgnoresBallButBreaksOnGround()
        {
            var body = new BreakableBody(CubeLike());

            body.TakeImpact(Impact(ImpactSource.Ball, 500f));
            Assert.IsFalse(body.IsBroken);

            body.TakeImpact(Impact(ImpactSource.Ground, GroundMinSpeed + MaxHealth));
            Assert.IsTrue(body.IsBroken);
        }

        [Test]
        public void UnknownSource_DealsNoDamage()
        {
            var body = new BreakableBody(JarLike());

            body.TakeImpact(Impact(ImpactSource.Unknown, 10000f));

            Assert.AreEqual(MaxHealth, body.Health, Tolerance);
        }

        [Test]
        public void ThresholdsAreIndependentPerSource()
        {
            var body = new BreakableBody(JarLike());

            body.TakeImpact(Impact(ImpactSource.Ground, GroundMinSpeed - 1f));
            Assert.AreEqual(MaxHealth, body.Health, Tolerance);

            body.TakeImpact(Impact(ImpactSource.Ball, BallMinSpeed + 1f));
            Assert.AreEqual(MaxHealth - 1f, body.Health, Tolerance);
        }
    }
}
