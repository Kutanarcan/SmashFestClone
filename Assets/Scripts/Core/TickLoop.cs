using System;

namespace Game.Core
{
    public sealed class TickLoop
    {
        private readonly ITickable[] tickables;

        public TickLoop(ITickable[] tickables)
        {
            if (tickables == null) throw new ArgumentNullException(nameof(tickables));

            for (int i = 0; i < tickables.Length; i++)
            {
                if (tickables[i] == null)
                    throw new ArgumentException($"Tickable at index {i} is null.", nameof(tickables));
            }

            this.tickables = tickables;
        }

        public int Count => tickables.Length;

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < tickables.Length; i++)
                tickables[i].Tick(deltaTime);
        }
    }
}
