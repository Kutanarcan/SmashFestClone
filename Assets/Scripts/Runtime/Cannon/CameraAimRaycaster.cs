using Game.Core.Cannon;
using UnityEngine;

namespace Game.Runtime.Cannon
{
    public sealed class CameraAimRaycaster : IAimRaycaster
    {
        private readonly Camera cam;
        private readonly LayerMask layerMask;
        private readonly float maxDistance;

        public CameraAimRaycaster(Camera cam, LayerMask layerMask, float maxDistance)
        {
            this.cam = cam;
            this.layerMask = layerMask;
            this.maxDistance = maxDistance;
        }

        public bool TryResolve(Vector2 screenPosition, out Vector3 worldPoint)
        {
            worldPoint = default;

            if (cam == null) return false;

            Ray ray = cam.ScreenPointToRay(screenPosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
                return false;

            worldPoint = hit.point;
            return true;
        }
    }
}
