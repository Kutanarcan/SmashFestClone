using Game.Core.Impacts;

namespace Game.Core.Breakables
{
    public readonly struct BreakableSettings
    {
        public const float Immune = float.PositiveInfinity;

        public readonly float MaxHealth;
        public readonly float DamagePerSpeedUnit;

        private readonly float ballMinSpeed;
        private readonly float groundMinSpeed;
        private readonly float debrisMinSpeed;

        public BreakableSettings(
            float maxHealth,
            float damagePerSpeedUnit,
            float ballMinSpeed,
            float groundMinSpeed,
            float debrisMinSpeed)
        {
            MaxHealth = maxHealth;
            DamagePerSpeedUnit = damagePerSpeedUnit;
            this.ballMinSpeed = ballMinSpeed;
            this.groundMinSpeed = groundMinSpeed;
            this.debrisMinSpeed = debrisMinSpeed;
        }

        public float DamageFor(in ImpactEvent impact)
        {
            float excess = impact.Speed - MinSpeedFor(impact.Source);
            return excess <= 0f ? 0f : excess * DamagePerSpeedUnit;
        }

        private float MinSpeedFor(ImpactSource source) => source switch
        {
            ImpactSource.Ball => ballMinSpeed,
            ImpactSource.Ground => groundMinSpeed,
            ImpactSource.Debris => debrisMinSpeed,
            _ => Immune
        };
    }
}
