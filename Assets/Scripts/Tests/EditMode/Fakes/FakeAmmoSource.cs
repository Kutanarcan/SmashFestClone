using Game.Core.Firing;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeAmmoSource : IAmmoSource
    {
        public int Remaining { get; set; } = int.MaxValue;

        public int ConsumeAttempts { get; private set; }

        public bool TryConsumeBall()
        {
            ConsumeAttempts++;

            if (Remaining <= 0) return false;

            Remaining--;
            return true;
        }
    }
}
