using Game.Core.Levels;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeWorldRestQuery : IWorldRestQuery
    {
        public bool IsAtRest { get; set; }
    }
}
