using UnityEngine;

namespace Game.Core.Cannon
{
    public interface IAimRaycaster
    {
        bool TryResolve(Vector2 screenPosition, out Vector3 worldPoint);
    }
}
