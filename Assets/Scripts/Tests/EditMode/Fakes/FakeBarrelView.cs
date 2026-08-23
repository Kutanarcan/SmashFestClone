using Game.Core.Aiming;
using Game.Core.Cannon;
using UnityEngine;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeBarrelView : IBarrelView
    {
        public bool CanResolveDirection { get; set; } = true;
        public Vector3 LocalDirection { get; set; } = Vector3.forward;

        public int ApplyCount { get; private set; }
        public AimAngles LastAngles { get; private set; }
        public float LastDeltaTime { get; private set; }
        public Vector3 LastWorldTarget { get; private set; }

        public bool TryGetLocalDirection(Vector3 worldTarget, out Vector3 localDirection)
        {
            LastWorldTarget = worldTarget;
            localDirection = CanResolveDirection ? LocalDirection : default;
            return CanResolveDirection;
        }

        public void Apply(AimAngles angles, float deltaTime)
        {
            ApplyCount++;
            LastAngles = angles;
            LastDeltaTime = deltaTime;
        }
    }
}
