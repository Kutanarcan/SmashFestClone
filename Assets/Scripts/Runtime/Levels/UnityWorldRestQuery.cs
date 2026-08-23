using System.Collections.Generic;
using Game.Core.Levels;
using UnityEngine;

namespace Game.Runtime.Levels
{
    public sealed class UnityWorldRestQuery : IWorldRestQuery
    {
        private readonly BallRegistry balls;
        private readonly List<Rigidbody> objectives;

        public UnityWorldRestQuery(BallRegistry balls, List<Rigidbody> objectives)
        {
            this.balls = balls;
            this.objectives = objectives;
        }

        public bool IsAtRest
        {
            get
            {
                if (balls != null && balls.HasLiveBalls) return false;
                if (objectives == null) return true;

                for (int i = 0; i < objectives.Count; i++)
                {
                    Rigidbody body = objectives[i];

                    if (body == null) continue;
                    if (!body.IsSleeping()) return false;
                }

                return true;
            }
        }
    }
}
