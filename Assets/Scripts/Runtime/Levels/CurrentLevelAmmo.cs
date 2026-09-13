using Game.Core.Firing;

namespace Game.Runtime.Levels
{
    public sealed class CurrentLevelAmmo : IAmmoSource
    {
        private readonly LevelFlowController flow;

        public CurrentLevelAmmo(LevelFlowController flow)
        {
            this.flow = flow;
        }

        public bool TryConsumeBall() =>
            flow != null && flow.Session != null && flow.Session.TryConsumeBall();
    }
}
