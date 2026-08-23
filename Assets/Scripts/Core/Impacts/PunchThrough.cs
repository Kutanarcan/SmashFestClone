using UnityEngine;

namespace Game.Core.Impacts
{
    public readonly struct PunchThrough
    {
        private readonly float speedRetention;

        public PunchThrough(float speedRetention)
        {
            this.speedRetention = Mathf.Clamp01(speedRetention);
        }

        public bool TryResolve(Vector3 velocityBeforeCollision, bool hitBreakable, out Vector3 velocity)
        {
            if (!hitBreakable)
            {
                velocity = default;
                return false;
            }

            velocity = velocityBeforeCollision * speedRetention;
            return true;
        }
    }
}
