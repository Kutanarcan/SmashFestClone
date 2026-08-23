using Game.Core.Timing;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeTimeProvider : ITimeProvider
    {
        public float Now { get; set; }

        public void Advance(float seconds) => Now += seconds;
    }
}
