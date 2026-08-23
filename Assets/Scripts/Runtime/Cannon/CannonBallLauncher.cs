using Game.Core.Cannon;
using Game.Runtime.Levels;
using UnityEngine;

namespace Game.Runtime.Cannon
{
    public sealed class CannonBallLauncher : IBallLauncher
    {
        private const float MinVelocitySqrMagnitude = 0.0001f;

        private readonly Transform muzzle;
        private readonly CannonBall prefab;
        private readonly BallRegistry registry;

        public CannonBallLauncher(Transform muzzle, CannonBall prefab, BallRegistry registry)
        {
            this.muzzle = muzzle;
            this.prefab = prefab;
            this.registry = registry;
        }

        public Vector3 MuzzlePosition => muzzle != null ? muzzle.position : Vector3.zero;

        public void Launch(Vector3 velocity)
        {
            if (muzzle == null || prefab == null) return;
            if (velocity.sqrMagnitude < MinVelocitySqrMagnitude) return;

            CannonBall ball = Object.Instantiate(
                prefab, muzzle.position, Quaternion.LookRotation(velocity.normalized));

            registry?.Register(ball);
            ball.Launch(velocity);
        }
    }
}
