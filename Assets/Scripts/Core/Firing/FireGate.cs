using System;

namespace Game.Core.Firing
{
    public sealed class FireGate
    {
        private readonly FireControl cooldown;
        private readonly IAmmoSource ammo;

        public FireGate(FireControl cooldown, IAmmoSource ammo)
        {
            this.cooldown = cooldown ?? throw new ArgumentNullException(nameof(cooldown));
            this.ammo = ammo ?? throw new ArgumentNullException(nameof(ammo));
        }

        public bool TryFire()
        {
            if (!cooldown.CanFire) return false;
            if (!ammo.TryConsumeBall()) return false;

            return cooldown.TryFire();
        }
    }
}
