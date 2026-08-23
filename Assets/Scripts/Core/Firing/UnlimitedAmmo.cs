namespace Game.Core.Firing
{
    public sealed class UnlimitedAmmo : IAmmoSource
    {
        public bool TryConsumeBall() => true;
    }
}
