using System;
using Game.Core.Firing;
using Game.Core.Levels;
using Game.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class FireGateTests
    {
        private const float Cooldown = 0.35f;

        private FakeTimeProvider time;
        private FakeAmmoSource ammo;
        private FireGate gate;

        [SetUp]
        public void SetUp()
        {
            time = new FakeTimeProvider();
            ammo = new FakeAmmoSource();
            gate = new FireGate(new FireControl(time, Cooldown), ammo);
        }

        [Test]
        public void FirstShot_SucceedsAndConsumesOneBall()
        {
            ammo.Remaining = 3;

            Assert.IsTrue(gate.TryFire());
            Assert.AreEqual(2, ammo.Remaining);
        }

        [Test]
        public void DuringCooldown_Fails()
        {
            gate.TryFire();

            Assert.IsFalse(gate.TryFire());
        }

        [Test]
        public void DuringCooldown_DoesNotTouchAmmo()
        {
            ammo.Remaining = 3;
            gate.TryFire();

            gate.TryFire();
            gate.TryFire();

            Assert.AreEqual(2, ammo.Remaining);
            Assert.AreEqual(1, ammo.ConsumeAttempts);
        }

        [Test]
        public void AfterCooldownElapses_SucceedsAgain()
        {
            ammo.Remaining = 3;
            gate.TryFire();

            time.Advance(Cooldown);

            Assert.IsTrue(gate.TryFire());
            Assert.AreEqual(1, ammo.Remaining);
        }

        [Test]
        public void WithNoAmmo_Fails()
        {
            ammo.Remaining = 0;

            Assert.IsFalse(gate.TryFire());
        }

        [Test]
        public void WithNoAmmo_DoesNotStartACooldown()
        {
            ammo.Remaining = 0;
            Assert.IsFalse(gate.TryFire());

            ammo.Remaining = 1;

            Assert.IsTrue(gate.TryFire());
        }

        [Test]
        public void ExhaustingAmmo_StopsFiringEvenAfterCooldowns()
        {
            ammo.Remaining = 2;

            Assert.IsTrue(gate.TryFire());
            time.Advance(Cooldown);
            Assert.IsTrue(gate.TryFire());
            time.Advance(Cooldown);

            Assert.IsFalse(gate.TryFire());
        }

        [Test]
        public void BackedByALevelSession_SpendsTheLevelBudget()
        {
            var session = new LevelSession(2, 1, new FakeWorldRestQuery(), 3f);
            var levelGate = new FireGate(new FireControl(time, Cooldown), session);

            Assert.IsTrue(levelGate.TryFire());

            Assert.AreEqual(0, session.BallsRemaining);
            Assert.AreEqual(LevelState.Settling, session.State);

            time.Advance(Cooldown);
            Assert.IsFalse(levelGate.TryFire());
        }

        [Test]
        public void NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(
                () => new FireGate(null, ammo));
            Assert.Throws<ArgumentNullException>(
                () => new FireGate(new FireControl(time, Cooldown), null));
        }
    }
}
