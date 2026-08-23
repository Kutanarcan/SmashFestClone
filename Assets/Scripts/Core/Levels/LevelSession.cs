using System;
using Game.Core.Firing;

namespace Game.Core.Levels
{
    public sealed class LevelSession : ITickable, IAmmoSource
    {
        private readonly IWorldRestQuery worldRest;
        private readonly float settleTimeout;
        private readonly bool[] cleared;

        private float settleElapsed;

        public LevelSession(
            int objectiveCount,
            int ballCount,
            IWorldRestQuery worldRest,
            float settleTimeout)
        {
            if (objectiveCount < 0)
                throw new ArgumentOutOfRangeException(nameof(objectiveCount));

            if (ballCount < 0)
                throw new ArgumentOutOfRangeException(nameof(ballCount));

            this.worldRest = worldRest ?? throw new ArgumentNullException(nameof(worldRest));
            this.settleTimeout = settleTimeout;

            cleared = new bool[objectiveCount];
            ObjectivesRemaining = objectiveCount;
            BallsRemaining = ballCount;

            State = objectiveCount == 0
                ? LevelState.Won
                : ballCount == 0
                    ? LevelState.Settling
                    : LevelState.Playing;
        }

        public event Action<LevelState> Finished;

        public LevelState State { get; private set; }

        public int BallsRemaining { get; private set; }

        public int ObjectivesRemaining { get; private set; }

        public bool IsFinished => State == LevelState.Won || State == LevelState.Lost;

        public bool TryConsumeBall()
        {
            if (State != LevelState.Playing) return false;
            if (BallsRemaining <= 0) return false;

            BallsRemaining--;

            if (BallsRemaining == 0)
            {
                State = LevelState.Settling;
                settleElapsed = 0f;
            }

            return true;
        }

        public void ClearObjective(int index)
        {
            if (index < 0 || index >= cleared.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (IsFinished) return;
            if (cleared[index]) return;

            cleared[index] = true;
            ObjectivesRemaining--;

            if (ObjectivesRemaining == 0)
                Finish(LevelState.Won);
        }

        public void Tick(float deltaTime)
        {
            if (State != LevelState.Settling) return;

            settleElapsed += deltaTime;

            if (worldRest.IsAtRest || settleElapsed >= settleTimeout)
                Finish(ObjectivesRemaining == 0 ? LevelState.Won : LevelState.Lost);
        }

        private void Finish(LevelState finalState)
        {
            State = finalState;
            Finished?.Invoke(finalState);
        }
    }
}
