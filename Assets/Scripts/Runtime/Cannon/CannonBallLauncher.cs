using System;
using Game.Core.Cannon;
using Game.Runtime.Levels;
using Game.Runtime.Pooling;
using UnityEngine;

namespace Game.Runtime.Cannon
{
    public sealed class CannonBallLauncher : IBallLauncher
    {
        private const float MinVelocitySqrMagnitude = 0.0001f;

        private readonly Transform muzzle;
        private readonly GameObjectPool<CannonBall> pool;
        private readonly BallRegistry registry;
        private readonly Action<CannonBall> releaseHandler;

        public CannonBallLauncher(
            Transform muzzle, GameObjectPool<CannonBall> pool, BallRegistry registry)
        {
            this.muzzle = muzzle;
            this.pool = pool;
            this.registry = registry;

            releaseHandler = Release;
        }

        public Vector3 MuzzlePosition => muzzle != null ? muzzle.position : Vector3.zero;

        public void Launch(Vector3 velocity)
        {
            if (muzzle == null || pool == null) return;
            if (velocity.sqrMagnitude < MinVelocitySqrMagnitude) return;

            CannonBall ball = pool.Get(
                muzzle.position, Quaternion.LookRotation(velocity.normalized));

            if (ball == null) return;

            registry?.Register(ball);
            ball.Launch(velocity, releaseHandler);
        }

        private void Release(CannonBall ball) => pool.Release(ball);
    }
}
