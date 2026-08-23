using Game.Core.Levels;
using UnityEngine;

namespace Game.Editor
{
    public sealed class CatalogObjectSizes : ILevelObjectSizes
    {
        private readonly LevelObjectCatalog catalog;

        public CatalogObjectSizes(LevelObjectCatalog catalog)
        {
            this.catalog = catalog;
        }

        public bool TryGet(string type, out Vector3 size, out Vector3 centerOffset)
        {
            size = Vector3.zero;
            centerOffset = Vector3.zero;

            if (catalog == null || !catalog.TryGetPrefab(type, out GameObject prefab)) return false;

            Footprint footprint = BrushFootprint.Of(prefab);
            if (!footprint.Valid) return false;

            size = footprint.HalfExtents * 2f;
            centerOffset = footprint.CenterOffset;

            return true;
        }
    }
}
