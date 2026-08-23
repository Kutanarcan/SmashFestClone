using Game.Core.Cannon;
using UnityEngine;

namespace Game.Runtime.Cannon
{
    public sealed class CannonBallLauncher : IBallLauncher
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly Transform muzzle;
        private readonly CannonBall prefab;
        private readonly float launchSpeed;

        public CannonBallLauncher(Transform muzzle, CannonBall prefab, float launchSpeed)
        {
            this.muzzle = muzzle;
            this.prefab = prefab;
            this.launchSpeed = launchSpeed;
        }

        public void LaunchTowards(Vector3 worldTarget)
        {
            if (muzzle == null || prefab == null) return;

            Vector3 toTarget = worldTarget - muzzle.position;
            if (toTarget.sqrMagnitude < MinDirectionSqrMagnitude) return;

            Vector3 dir = toTarget.normalized;

            CannonBall ball = Object.Instantiate(prefab, muzzle.position, Quaternion.LookRotation(dir));
            ball.Launch(dir * launchSpeed);
        }
    }
}
