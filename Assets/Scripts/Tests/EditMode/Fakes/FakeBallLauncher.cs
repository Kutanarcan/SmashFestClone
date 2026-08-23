using Game.Core.Cannon;
using UnityEngine;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeBallLauncher : IBallLauncher
    {
        public int LaunchCount { get; private set; }
        public Vector3 LastTarget { get; private set; }

        public void LaunchTowards(Vector3 worldTarget)
        {
            LaunchCount++;
            LastTarget = worldTarget;
        }
    }
}
