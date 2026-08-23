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

        public BreakableSettings(
            float maxHealth,
            float damagePerSpeedUnit,
            float ballMinSpeed,
            float groundMinSpeed)
        {
            MaxHealth = maxHealth;
            DamagePerSpeedUnit = damagePerSpeedUnit;
            this.ballMinSpeed = ballMinSpeed;
            this.groundMinSpeed = groundMinSpeed;
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
            _ => Immune
        };
    }
}
