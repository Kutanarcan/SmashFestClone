using Game.Core.Firing;
using Game.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class FireControlTests
    {
        private const float Cooldown = 0.35f;

        private FakeTimeProvider time;
        private FireControl fireControl;

        [SetUp]
        public void SetUp()
        {
            time = new FakeTimeProvider();
            fireControl = new FireControl(time, Cooldown);
        }

        [Test]
        public void FirstShot_IsAllowedAtTimeZero()
        {
            Assert.IsTrue(fireControl.CanFire);
            Assert.IsTrue(fireControl.TryFire());
        }

        [Test]
        public void SecondShot_InSameFrame_IsDenied()
        {
            Assert.IsTrue(fireControl.TryFire());
            Assert.IsFalse(fireControl.TryFire());
        }

        [Test]
        public void Shot_BeforeCooldownElapsed_IsDenied()
        {
            fireControl.TryFire();
            time.Advance(Cooldown - 0.01f);

            Assert.IsFalse(fireControl.TryFire());
        }

        [Test]
        public void Shot_ExactlyAtCooldownBoundary_IsAllowed()
        {
            fireControl.TryFire();
            time.Advance(Cooldown);

            Assert.IsTrue(fireControl.TryFire());
        }

        [Test]
        public void DeniedShot_DoesNotExtendTheCooldown()
        {
            fireControl.TryFire();

            time.Advance(0.2f);
            Assert.IsFalse(fireControl.TryFire());

            time.Advance(0.15f);
            Assert.IsTrue(fireControl.TryFire());
        }

        [Test]
        public void CanFire_DoesNotConsumeTheShot()
        {
            Assert.IsTrue(fireControl.CanFire);
            Assert.IsTrue(fireControl.CanFire);
            Assert.IsTrue(fireControl.TryFire());
        }

        [Test]
        public void ZeroCooldown_AllowsEveryShot()
        {
            var rapid = new FireControl(time, 0f);

            Assert.IsTrue(rapid.TryFire());
            Assert.IsTrue(rapid.TryFire());
            Assert.IsTrue(rapid.TryFire());
        }

        [Test]
        public void NullTimeProvider_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new FireControl(null, Cooldown));
        }
    }
}
