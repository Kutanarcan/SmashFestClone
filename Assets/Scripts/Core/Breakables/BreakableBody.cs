using Game.Core.Impacts;

namespace Game.Core.Breakables
{
    public sealed class BreakableBody
    {
        private readonly BreakableSettings settings;

        public BreakableBody(BreakableSettings settings)
        {
            this.settings = settings;
            Health = settings.MaxHealth;
        }

        public event ImpactHandler Broke;

        public float Health { get; private set; }

        public bool IsBroken => Health <= 0f;

        public void TakeImpact(in ImpactEvent impact)
        {
            if (IsBroken) return;

            float damage = settings.DamageFor(impact);
            if (damage <= 0f) return;

            Health -= damage;
            if (Health > 0f) return;

            Health = 0f;
            Broke?.Invoke(impact);
        }
    }
}
