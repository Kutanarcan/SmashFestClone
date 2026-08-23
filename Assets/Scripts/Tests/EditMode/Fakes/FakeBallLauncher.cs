using Game.Core.Cannon;
using UnityEngine;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeBallLauncher : IBallLauncher
    {
        public Vector3 MuzzlePosition { get; set; }

        public int LaunchCount { get; private set; }
        public Vector3 LastVelocity { get; private set; }

        public void Launch(Vector3 velocity)
        {
            LaunchCount++;
            LastVelocity = velocity;
        }
    }
}
