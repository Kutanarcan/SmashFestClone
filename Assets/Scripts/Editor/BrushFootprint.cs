using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor
{
    public struct Footprint
    {
        public bool Valid;
        public int ColliderCount;
        public Vector3 HalfExtents;
        public Vector3 CenterOffset;
    }

    public static class BrushFootprint
    {
        private const float MinExtent = 0.0001f;

        private static readonly Dictionary<GameObject, Footprint> Cache =
            new Dictionary<GameObject, Footprint>();

        public static void ClearCache() => Cache.Clear();

        public static Footprint Of(GameObject prefab)
        {
            if (prefab == null) return default;

            if (Cache.TryGetValue(prefab, out Footprint cached)) return cached;

            Footprint measured = Measure(prefab);
            Cache[prefab] = measured;

            return measured;
        }

        private static Footprint Measure(GameObject prefab)
        {
            GameObject probe = Object.Instantiate(prefab);
            probe.hideFlags = HideFlags.HideAndDontSave;
            probe.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            probe.transform.localScale = prefab.transform.localScale;

            Footprint footprint = default;

            if (TryGatherColliderBounds(probe, out Bounds bounds, out int colliderCount))
            {
                footprint.ColliderCount = colliderCount;
                footprint.HalfExtents = bounds.extents;
                footprint.CenterOffset = bounds.center;
                footprint.Valid = bounds.extents.x > MinExtent
                               && bounds.extents.y > MinExtent
                               && bounds.extents.z > MinExtent;
            }

            Object.DestroyImmediate(probe);

            return footprint;
        }

        private static bool TryGatherColliderBounds(GameObject root, out Bounds bounds, out int colliderCount)
        {
            bounds = default;
            colliderCount = 0;

            Collider[] colliders = root.GetComponentsInChildren<Collider>();

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].isTrigger) continue;

                if (colliderCount == 0) bounds = colliders[i].bounds;
                else bounds.Encapsulate(colliders[i].bounds);

                colliderCount++;
            }

            return colliderCount > 0;
        }
    }
}
