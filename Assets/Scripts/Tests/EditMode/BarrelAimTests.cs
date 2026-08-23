using Game.Core.Aiming;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class BarrelAimTests
    {
        private const float Tolerance = 0.001f;

        private const float MinElevation = -10f;
        private const float MaxElevation = 60f;
        private const float MaxYaw = 45f;

        private static BarrelAim CreateAim() => new BarrelAim(MinElevation, MaxElevation, MaxYaw);

        [Test]
        public void StraightAhead_IsZeroYawAndZeroElevation()
        {
            Assert.IsTrue(CreateAim().TryResolve(Vector3.forward, out AimAngles angles));

            Assert.AreEqual(0f, angles.Yaw, Tolerance);
            Assert.AreEqual(0f, angles.Elevation, Tolerance);
        }

        [Test]
        public void FortyFiveDegreesRight_IsNotClamped()
        {
            Assert.IsTrue(CreateAim().TryResolve(new Vector3(1f, 0f, 1f), out AimAngles angles));

            Assert.AreEqual(45f, angles.Yaw, Tolerance);
        }

        [Test]
        public void HardRight_ClampsToMaxYaw()
        {
            Assert.IsTrue(CreateAim().TryResolve(Vector3.right, out AimAngles angles));

            Assert.AreEqual(MaxYaw, angles.Yaw, Tolerance);
        }

        [Test]
        public void HardLeft_ClampsToNegativeMaxYaw()
        {
            Assert.IsTrue(CreateAim().TryResolve(Vector3.left, out AimAngles angles));

            Assert.AreEqual(-MaxYaw, angles.Yaw, Tolerance);
        }

        [Test]
        public void StraightUp_ClampsToMaxElevation()
        {
            Assert.IsTrue(CreateAim().TryResolve(Vector3.up, out AimAngles angles));

            Assert.AreEqual(MaxElevation, angles.Elevation, Tolerance);
        }

        [Test]
        public void StraightDown_ClampsToMinElevation()
        {
            Assert.IsTrue(CreateAim().TryResolve(Vector3.down, out AimAngles angles));

            Assert.AreEqual(MinElevation, angles.Elevation, Tolerance);
        }

        [Test]
        public void UpwardDiagonal_WithinLimits_KeepsExactAngle()
        {
            Assert.IsTrue(CreateAim().TryResolve(new Vector3(0f, 1f, 1f), out AimAngles angles));

            Assert.AreEqual(45f, angles.Elevation, Tolerance);
        }

        [Test]
        public void MagnitudeDoesNotChangeResult()
        {
            BarrelAim aim = CreateAim();

            Assert.IsTrue(aim.TryResolve(new Vector3(0f, 1f, 1f), out AimAngles unitScale));
            Assert.IsTrue(aim.TryResolve(new Vector3(0f, 250f, 250f), out AimAngles largeScale));

            Assert.AreEqual(unitScale.Yaw, largeScale.Yaw, Tolerance);
            Assert.AreEqual(unitScale.Elevation, largeScale.Elevation, Tolerance);
        }

        /// <summary>
        /// Geriye bakan yon eulerAngles ile 180 / -180 arasinda sicrar ve clamp'i bozardi.
        /// Atan2 tek deger dondurdugu icin sonuc her zaman maxYaw'a oturur.
        /// </summary>
        [Test]
        public void Backwards_ClampsWithoutGimbalJump()
        {
            Assert.IsTrue(CreateAim().TryResolve(Vector3.back, out AimAngles angles));

            Assert.AreEqual(MaxYaw, angles.Yaw, Tolerance);
            Assert.AreEqual(0f, angles.Elevation, Tolerance);
        }

        [Test]
        public void BackwardsAndSlightlyLeft_ClampsToNegativeMaxYaw()
        {
            Assert.IsTrue(CreateAim().TryResolve(new Vector3(-0.01f, 0f, -1f), out AimAngles angles));

            Assert.AreEqual(-MaxYaw, angles.Yaw, Tolerance);
        }

        [Test]
        public void ZeroDirection_ReturnsFalse()
        {
            Assert.IsFalse(CreateAim().TryResolve(Vector3.zero, out AimAngles _));
        }

        [Test]
        public void NearZeroDirection_ReturnsFalse()
        {
            Assert.IsFalse(CreateAim().TryResolve(new Vector3(0f, 0.001f, 0f), out AimAngles _));
        }
    }
}
