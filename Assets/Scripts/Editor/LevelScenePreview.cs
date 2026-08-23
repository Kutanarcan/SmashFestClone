using Game.Core.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public sealed class LevelScenePreview
    {
        private const string ContainerName = "~LevelBrushPreview";

        private GameObject container;

        public bool IsActive => container != null;

        public void Create(Transform parent)
        {
            if (container != null) return;

            container = new GameObject(ContainerName) { hideFlags = HideFlags.DontSave };

            if (parent != null) container.transform.SetParent(parent, false);
        }

        public void Destroy()
        {
            if (container != null) Object.DestroyImmediate(container);

            container = null;
        }

        public Vector3 OriginFor(LevelGrid grid, Vector3 unitSize, Transform anchor)
        {
            Vector3 basePosition = anchor != null ? anchor.position : Vector3.zero;

            return basePosition - new Vector3(
                grid.Width * unitSize.x * 0.5f,
                0f,
                grid.Depth * unitSize.z * 0.5f);
        }

        public void Rebuild(
            LevelGrid grid,
            LevelObjectCatalog catalog,
            Vector3 unitSize,
            Vector3 gridOrigin)
        {
            if (container == null || grid == null || catalog == null) return;

            ClearChildren();

            for (int i = 0; i < grid.Count; i++)
                Spawn(grid.PlacementAt(i), catalog, unitSize, gridOrigin);
        }

        private void ClearChildren()
        {
            for (int i = container.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(container.transform.GetChild(i).gameObject);
        }

        private void Spawn(
            GridPlacement placement,
            LevelObjectCatalog catalog,
            Vector3 unitSize,
            Vector3 gridOrigin)
        {
            if (!catalog.TryGetPrefab(placement.Type, out GameObject prefab)) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, container.transform);
            if (instance == null) return;

            Quaternion rotation = Quaternion.Euler(0f, placement.Yaw, 0f);

            Vector3 colliderCenter = GridSpace.CellToWorldCenter(
                placement.Origin, placement.Span, unitSize, gridOrigin);

            Footprint footprint = BrushFootprint.Of(prefab);
            Vector3 rootOffset = footprint.Valid ? rotation * footprint.CenterOffset : Vector3.zero;

            instance.transform.SetPositionAndRotation(colliderCenter - rootOffset, rotation);
            instance.hideFlags = HideFlags.DontSave;
        }
    }
}
