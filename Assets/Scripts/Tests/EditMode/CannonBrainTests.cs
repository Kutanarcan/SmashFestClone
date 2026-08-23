using System;
using Game.Core.Aiming;
using Game.Core.Ballistics;
using Game.Core.Cannon;
using Game.Core.Firing;
using Game.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class CannonBrainTests
    {
        private const float Cooldown = 0.35f;
        private const float MinElevation = -10f;
        private const float MaxElevation = 60f;
        private const float MaxYaw = 45f;
        private const float LaunchSpeed = 100f;
        private const float Dt = 0.016f;
        private const float Tolerance = 0.001f;

        private FakePointerInputSource input;
        private FakeAimRaycaster raycaster;
        private FakeBarrelView barrel;
        private FakeBallLauncher launcher;
        private FakeTimeProvider time;
        private CannonBrain brain;

        [SetUp]
        public void SetUp()
        {
            input = new FakePointerInputSource();
            raycaster = new FakeAimRaycaster { HitPoint = new Vector3(0f, 0f, 10f) };
            barrel = new FakeBarrelView();
            launcher = new FakeBallLauncher { MuzzlePosition = Vector3.zero };
            time = new FakeTimeProvider();

            brain = Build(gravity: 0f);
        }

        private CannonBrain Build(float gravity) => new CannonBrain(
            input,
            raycaster,
            barrel,
            launcher,
            new FireControl(time, Cooldown),
            new BarrelAim(MinElevation, MaxElevation, MaxYaw),
            new BallisticAim(new BallisticSolver(gravity), LaunchSpeed, ArcPreference.Low));

        [Test]
        public void TapOnGeometry_LaunchesExactlyOnce()
        {
            input.PressAt(new Vector2(100f, 200f));

            brain.Tick(Dt);

            Assert.AreEqual(1, launcher.LaunchCount);
        }

        [Test]
        public void TapOnGeometry_PassesScreenPositionToRaycaster()
        {
            input.PressAt(new Vector2(100f, 200f));

            brain.Tick(Dt);

            Assert.AreEqual(new Vector2(100f, 200f), raycaster.LastScreenPosition);
        }

        [Test]
        public void Launch_UsesTheSolvedVelocityAtTheConfiguredSpeed()
        {
            raycaster.HitPoint = new Vector3(3f, 1f, 7f);
            input.PressAt(Vector2.zero);

            brain.Tick(Dt);

            Assert.AreEqual(new Vector3(3f, 1f, 7f), brain.AimPoint);
            Assert.AreEqual(LaunchSpeed, launcher.LastVelocity.magnitude, 0.01f);
        }

        [Test]
        public void WithGravity_BarrelPointsAlongTheArcNotAtTheTarget()
        {
            brain = Build(gravity: 15f);
            raycaster.HitPoint = new Vector3(0f, 0f, 40f);
            input.PressAt(Vector2.zero);

            brain.Tick(Dt);

            Assert.AreEqual(0f, raycaster.HitPoint.y, Tolerance);
            Assert.Greater(barrel.LastWorldDirection.y, 0f);
            Assert.Greater(launcher.LastVelocity.y, 0f);
        }

        [Test]
        public void UnreachableTarget_StillFires_ButReportsItCannotReach()
        {
            brain = Build(gravity: 15f);
            raycaster.HitPoint = new Vector3(0f, 0f, 5000f);
            input.PressAt(Vector2.zero);

            brain.Tick(Dt);

            Assert.IsFalse(brain.CanReachAim);
            Assert.AreEqual(1, launcher.LaunchCount);
            Assert.AreEqual(LaunchSpeed, launcher.LastVelocity.magnitude, 0.01f);
        }

        [Test]
        public void ReachableTarget_ReportsItCanReach()
        {
            brain = Build(gravity: 15f);
            raycaster.HitPoint = new Vector3(0f, 0f, 40f);
            input.PressAt(Vector2.zero);

            brain.Tick(Dt);

            Assert.IsTrue(brain.CanReachAim);
        }

        [Test]
        public void NoPress_DoesNotRaycastOrLaunch()
        {
            brain.Tick(Dt);

            Assert.AreEqual(0, raycaster.CallCount);
            Assert.AreEqual(0, launcher.LaunchCount);
        }

        [Test]
        public void PressOverUI_IsIgnored()
        {
            input.PressAt(Vector2.zero);
            input.IsOverUI = true;

            brain.Tick(Dt);

            Assert.AreEqual(0, raycaster.CallCount);
            Assert.AreEqual(0, launcher.LaunchCount);
            Assert.IsFalse(brain.HasAimed);
        }

        [Test]
        public void TapIntoEmptySpace_DoesNotAimAndDoesNotLaunch()
        {
            raycaster.ShouldHit = false;
            input.PressAt(Vector2.zero);

            brain.Tick(Dt);

            Assert.IsFalse(brain.HasAimed);
            Assert.AreEqual(0, launcher.LaunchCount);
        }

        [Test]
        public void TapDuringCooldown_ReAimsButDoesNotLaunch()
        {
            input.PressAt(Vector2.zero);
            brain.Tick(Dt);
            Assert.AreEqual(1, launcher.LaunchCount);

            raycaster.HitPoint = new Vector3(9f, 0f, 9f);
            input.PressAt(Vector2.zero);
            brain.Tick(Dt);

            Assert.AreEqual(1, launcher.LaunchCount);
            Assert.AreEqual(new Vector3(9f, 0f, 9f), brain.AimPoint);
        }

        [Test]
        public void TapAfterCooldownElapsed_LaunchesAgain()
        {
            input.PressAt(Vector2.zero);
            brain.Tick(Dt);

            time.Advance(Cooldown);
            input.PressAt(Vector2.zero);
            brain.Tick(Dt);

            Assert.AreEqual(2, launcher.LaunchCount);
        }

        [Test]
        public void Barrel_IsNotTouchedBeforeFirstAim()
        {
            brain.Tick(Dt);
            brain.Tick(Dt);

            Assert.AreEqual(0, barrel.ApplyCount);
        }

        [Test]
        public void Barrel_KeepsUpdatingAfterAim_EvenWithoutFurtherInput()
        {
            input.PressAt(Vector2.zero);
            brain.Tick(Dt);
            input.Release();

            brain.Tick(Dt);
            brain.Tick(Dt);

            Assert.AreEqual(3, barrel.ApplyCount);
        }

        [Test]
        public void Barrel_ReceivesClampedAngles()
        {
            barrel.LocalDirection = Vector3.right;
            input.PressAt(Vector2.zero);

            brain.Tick(Dt);

            Assert.AreEqual(MaxYaw, barrel.LastAngles.Yaw, Tolerance);
            Assert.AreEqual(0f, barrel.LastAngles.Elevation, Tolerance);
        }

        [Test]
        public void Barrel_ReceivesTheDeltaTime()
        {
            input.PressAt(Vector2.zero);

            brain.Tick(0.25f);

            Assert.AreEqual(0.25f, barrel.LastDeltaTime, Tolerance);
        }

        [Test]
        public void Barrel_UnresolvableDirection_IsNotApplied()
        {
            barrel.CanResolveDirection = false;
            input.PressAt(Vector2.zero);

            brain.Tick(Dt);

            Assert.AreEqual(0, barrel.ApplyCount);
        }

        [Test]
        public void NullDependencies_Throw()
        {
            var fire = new FireControl(time, Cooldown);
            var aim = new BarrelAim(MinElevation, MaxElevation, MaxYaw);
            var ballistic = new BallisticAim(new BallisticSolver(15f), LaunchSpeed, ArcPreference.Low);

            Assert.Throws<ArgumentNullException>(
                () => new CannonBrain(null, raycaster, barrel, launcher, fire, aim, ballistic));
            Assert.Throws<ArgumentNullException>(
                () => new CannonBrain(input, null, barrel, launcher, fire, aim, ballistic));
            Assert.Throws<ArgumentNullException>(
                () => new CannonBrain(input, raycaster, null, launcher, fire, aim, ballistic));
            Assert.Throws<ArgumentNullException>(
                () => new CannonBrain(input, raycaster, barrel, null, fire, aim, ballistic));
            Assert.Throws<ArgumentNullException>(
                () => new CannonBrain(input, raycaster, barrel, launcher, null, aim, ballistic));
        }
    }
}
