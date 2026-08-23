using Game.Core.Timing;
using UnityEngine;

namespace Game.Runtime.Timing
{
    public sealed class UnityTimeProvider : ITimeProvider
    {
        public float Now => Time.time;
    }
}
