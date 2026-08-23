using Game.Core.Ballistics;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class BallisticSolverTests
    {
        private const float Gravity = 15f;
        private const float Speed = 100f;
        private const float Tolerance = 0.01f;

        private static readonly Vector3 Muzzle = new Vector3(0f, 1f, 0f);

        private static BallisticSolver Solver() => new BallisticSolver(Gravity);

        private static float HeightWhenAbove(Vector3 origin, Vector3 target, Vector3 velocity)
        {
            Vector3 delta = target - origin;
            float horizontalDistance = new Vector2(delta.x, delta.z).magnitude;
            float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;

            float t = horizontalDistance / horizontalSpeed;
            return origin.y + velocity.y * t - 0.5f * Gravity * t * t;
        }

        [Test]
        public void LowArc_TrajectoryPassesThroughTheTarget()
        {
            var target = new Vector3(0f, 3f, 40f);

            Assert.IsTrue(Solver().TrySolveVelocity(
                Muzzle, target, Speed, ArcPreference.Low, out Vector3 velocity));

            Assert.AreEqual(target.y, HeightWhenAbove(Muzzle, target, velocity), Tolerance);
        }

        [Test]
        public void HighArc_TrajectoryPassesThroughTheTarget()
        {
            var target = new Vector3(0f, 3f, 40f);

            Assert.IsTrue(Solver().TrySolveVelocity(
                Muzzle, target, Speed, ArcPreference.High, out Vector3 velocity));

            Assert.AreEqual(target.y, HeightWhenAbove(Muzzle, target, velocity), Tolerance);
        }

        [Test]
        public void HighArc_LaunchesSteeperThanLowArc()
        {
            var target = new Vector3(0f, 3f, 40f);
            BallisticSolver solver = Solver();

            solver.TrySolveVelocity(Muzzle, target, Speed, ArcPreference.Low, out Vector3 low);
            solver.TrySolveVelocity(Muzzle, target, Speed, ArcPreference.High, out Vector3 high);

            Assert.Greater(high.y, low.y);
        }

        [Test]
        public void SolvedVelocity_HasExactlyTheRequestedSpeed()
        {
            var target = new Vector3(12f, 3f, 40f);

            Solver().TrySolveVelocity(Muzzle, target, Speed, ArcPreference.Low, out Vector3 velocity);

            Assert.AreEqual(Speed, velocity.magnitude, Tolerance);
        }

        [Test]
        public void LowArc_AimsAboveTheStraightLine()
        {
            var target = new Vector3(0f, 1f, 40f);

            Solver().TrySolveVelocity(Muzzle, target, Speed, ArcPreference.Low, out Vector3 velocity);

            Assert.Greater(velocity.y, 0f);
        }

        [Test]
        public void TargetBelowTheMuzzle_IsReachable()
        {
            var target = new Vector3(0f, -5f, 30f);

            Assert.IsTrue(Solver().TrySolveVelocity(
                Muzzle, target, Speed, ArcPreference.Low, out Vector3 velocity));

            Assert.AreEqual(target.y, HeightWhenAbove(Muzzle, target, velocity), Tolerance);
        }

        [Test]
        public void TargetBeyondRange_ReturnsFalse()
        {
            var target = new Vector3(0f, 0f, 5000f);

            Assert.IsFalse(Solver().TrySolveVelocity(
                Muzzle, target, Speed, ArcPreference.Low, out Vector3 _));
        }

        [Test]
        public void MaxRangeVelocity_KeepsTheRequestedSpeed()
        {
            var target = new Vector3(0f, 0f, 5000f);

            Vector3 velocity = Solver().MaxRangeVelocity(Muzzle, target, Speed);

            Assert.AreEqual(Speed, velocity.magnitude, Tolerance);
        }

        [Test]
        public void ZeroGravity_FiresStraightAtTheTarget()
        {
            var solver = new BallisticSolver(0f);
            var target = new Vector3(0f, 10f, 40f);

            Assert.IsTrue(solver.TrySolveVelocity(
                Muzzle, target, Speed, ArcPreference.Low, out Vector3 velocity));

            Vector3 straight = (target - Muzzle).normalized * Speed;
            Assert.AreEqual(straight.x, velocity.x, Tolerance);
            Assert.AreEqual(straight.y, velocity.y, Tolerance);
            Assert.AreEqual(straight.z, velocity.z, Tolerance);
        }

        [Test]
        public void TargetDirectlyAbove_WithinReach_FiresStraightUp()
        {
            var target = new Vector3(0f, 20f, 0f);

            Assert.IsTrue(Solver().TrySolveVelocity(
                Muzzle, target, Speed, ArcPreference.Low, out Vector3 velocity));

            Assert.AreEqual(Speed, velocity.y, Tolerance);
        }

        [Test]
        public void TargetDirectlyAbove_BeyondReach_ReturnsFalse()
        {
            var slow = new BallisticSolver(Gravity);
            var target = new Vector3(0f, 5000f, 0f);

            Assert.IsFalse(slow.TrySolveVelocity(
                Muzzle, target, Speed, ArcPreference.Low, out Vector3 _));
        }

        [Test]
        public void BallisticAim_FallsBackToMaxRange_AndReportsUnreachable()
        {
            var aim = new BallisticAim(Solver(), Speed, ArcPreference.Low);
            var target = new Vector3(0f, 0f, 5000f);

            Assert.IsFalse(aim.TryVelocityTo(Muzzle, target, out Vector3 velocity));
            Assert.AreEqual(Speed, velocity.magnitude, Tolerance);
        }

        [Test]
        public void BallisticAim_ReportsReachableTargets()
        {
            var aim = new BallisticAim(Solver(), Speed, ArcPreference.Low);
            var target = new Vector3(0f, 3f, 40f);

            Assert.IsTrue(aim.TryVelocityTo(Muzzle, target, out Vector3 velocity));
            Assert.AreEqual(target.y, HeightWhenAbove(Muzzle, target, velocity), Tolerance);
        }
    }
}
