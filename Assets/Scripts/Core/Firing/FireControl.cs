using System;
using Game.Core.Timing;

namespace Game.Core.Firing
{
    public sealed class FireControl
    {
        private readonly ITimeProvider time;
        private readonly float cooldown;

        private float lastFireTime = float.NegativeInfinity;

        public FireControl(ITimeProvider time, float cooldown)
        {
            this.time = time ?? throw new ArgumentNullException(nameof(time));
            this.cooldown = cooldown;
        }

        public bool CanFire => time.Now >= lastFireTime + cooldown;

        public bool TryFire()
        {
            if (!CanFire) return false;

            lastFireTime = time.Now;
            return true;
        }
    }
}
