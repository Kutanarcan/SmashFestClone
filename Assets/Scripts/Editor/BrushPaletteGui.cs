using Game.Core.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class BrushPaletteGui
    {
        public static int Draw(LevelObjectCatalog catalog, int selectedIndex, Vector3 unitSize, float yaw)
        {
            if (catalog == null || catalog.Count == 0)
            {
                EditorGUILayout.HelpBox("Assign a catalog with at least one entry.", MessageType.Warning);
                return selectedIndex;
            }

            var labels = new string[catalog.Count];
            for (int i = 0; i < labels.Length; i++) labels[i] = catalog.TypeAt(i);

            selectedIndex = Mathf.Clamp(selectedIndex, 0, labels.Length - 1);
            selectedIndex = GUILayout.SelectionGrid(selectedIndex, labels, 3);

            DrawFootprint(catalog.PrefabAt(selectedIndex), labels[selectedIndex], unitSize, yaw);

            return selectedIndex;
        }

        private static void DrawFootprint(GameObject prefab, string label, Vector3 unitSize, float yaw)
        {
            Footprint footprint = BrushFootprint.Of(prefab);

            if (!footprint.Valid)
            {
                EditorGUILayout.HelpBox(
                    $"'{label}' has no non-trigger collider, so its size is unknown.",
                    MessageType.Error);
                return;
            }

            Vector3 size = footprint.HalfExtents * 2f;
            CellSpan span = SpanFor(footprint, unitSize, yaw);

            EditorGUILayout.LabelField(
                "Collider size",
                $"{size.x:0.##} x {size.y:0.##} x {size.z:0.##}   ->   {span} cells");
        }

        public static CellSpan SpanFor(Footprint footprint, Vector3 unitSize, float yaw)
        {
            Vector3 rotated = GridSpace.WorldAlignedSize(
                footprint.HalfExtents * 2f, Quaternion.Euler(0f, yaw, 0f));

            return GridSpace.SpanFor(rotated, unitSize);
        }
    }
}
