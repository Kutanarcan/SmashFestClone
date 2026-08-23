using System.Collections.Generic;
using Game.Core;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeTickable : ITickable
    {
        private readonly string name;
        private readonly List<string> tickLog;

        public FakeTickable(string name = "tickable", List<string> tickLog = null)
        {
            this.name = name;
            this.tickLog = tickLog;
        }

        public int TickCount { get; private set; }
        public float LastDeltaTime { get; private set; }

        public void Tick(float deltaTime)
        {
            TickCount++;
            LastDeltaTime = deltaTime;
            tickLog?.Add(name);
        }
    }
}
