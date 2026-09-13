using System.Collections.Generic;

namespace Game.Runtime.Levels
{
    public sealed class BallRegistry
    {
        private readonly List<CannonBall> live = new List<CannonBall>();

        public void Register(CannonBall ball)
        {
            if (ball == null || live.Contains(ball)) return;

            live.Add(ball);
        }

        public bool HasLiveBalls
        {
            get
            {
                Prune();
                return live.Count > 0;
            }
        }

        private void Prune()
        {
            for (int i = live.Count - 1; i >= 0; i--)
            {
                if (live[i] == null || !live[i].gameObject.activeInHierarchy)
                    live.RemoveAt(i);
            }
        }
    }
}
