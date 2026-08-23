using Game.Core.Cannon;
using UnityEngine;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeAimRaycaster : IAimRaycaster
    {
        public bool ShouldHit { get; set; } = true;
        public Vector3 HitPoint { get; set; }

        public int CallCount { get; private set; }
        public Vector2 LastScreenPosition { get; private set; }

        public bool TryResolve(Vector2 screenPosition, out Vector3 worldPoint)
        {
            CallCount++;
            LastScreenPosition = screenPosition;

            worldPoint = ShouldHit ? HitPoint : default;
            return ShouldHit;
        }
    }
}
