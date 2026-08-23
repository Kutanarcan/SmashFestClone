using Game.Core.Aiming;
using Game.Core.Cannon;
using UnityEngine;

namespace Game.Runtime.Cannon
{
    public sealed class TransformBarrelView : IBarrelView
    {
        private readonly Transform barrel;
        private readonly Vector3 axisOffset;
        private readonly float rotationSpeed;

        public TransformBarrelView(Transform barrel, Vector3 axisOffset, float rotationSpeed)
        {
            this.barrel = barrel;
            this.axisOffset = axisOffset;
            this.rotationSpeed = rotationSpeed;
        }

        public bool TryGetLocalDirection(Vector3 worldTarget, out Vector3 localDirection)
        {
            localDirection = default;

            if (barrel == null) return false;

            Vector3 toTarget = worldTarget - barrel.position;

            localDirection = barrel.parent != null
                ? barrel.parent.InverseTransformDirection(toTarget)
                : toTarget;

            return true;
        }

        public void Apply(AimAngles angles, float deltaTime)
        {
            if (barrel == null) return;

            Quaternion target = Quaternion.Euler(-angles.Elevation, angles.Yaw, 0f)
                              * Quaternion.Euler(axisOffset);

            barrel.localRotation = rotationSpeed <= 0f
                ? target
                : Quaternion.Slerp(barrel.localRotation, target, rotationSpeed * deltaTime);
        }
    }
}
