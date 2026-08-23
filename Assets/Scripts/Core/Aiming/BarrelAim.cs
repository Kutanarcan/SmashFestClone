using UnityEngine;

namespace Game.Core.Aiming
{
    public readonly struct BarrelAim
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly float minElevation;
        private readonly float maxElevation;
        private readonly float maxYaw;

        public BarrelAim(float minElevation, float maxElevation, float maxYaw)
        {
            this.minElevation = minElevation;
            this.maxElevation = maxElevation;
            this.maxYaw = maxYaw;
        }

        public bool TryResolve(in Vector3 localDirection, out AimAngles angles)
        {
            if (localDirection.sqrMagnitude < MinDirectionSqrMagnitude)
            {
                angles = default;
                return false;
            }

            Vector3 dir = localDirection.normalized;

            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            float elevation = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;

            angles = new AimAngles(
                Mathf.Clamp(yaw, -maxYaw, maxYaw),
                Mathf.Clamp(elevation, minElevation, maxElevation));

            return true;
        }
    }
}
