using UnityEngine;

namespace Game.Core.Levels
{
    public sealed class LevelProgress
    {
        public LevelProgress(int levelCount)
        {
            Count = Mathf.Max(0, levelCount);
        }

        public int Count { get; }

        public int CurrentIndex { get; private set; }

        public bool HasLevels => Count > 0;

        public bool HasNext => CurrentIndex + 1 < Count;

        public bool IsOnLastLevel => HasLevels && !HasNext;

        public bool TryAdvance()
        {
            if (!HasNext) return false;

            CurrentIndex++;
            return true;
        }

        public void GoTo(int index)
        {
            CurrentIndex = Count == 0 ? 0 : Mathf.Clamp(index, 0, Count - 1);
        }

        public void Reset() => CurrentIndex = 0;
    }
}
