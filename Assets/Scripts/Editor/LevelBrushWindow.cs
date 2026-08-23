using System;
using System.Collections.Generic;
using Game.Core.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class LevelBrushWindow : EditorWindow
    {
        [Serializable]
        private struct SavedPlacement
        {
            public string type;
            public int x, y, z;
            public int width, height, depth;
            public float yaw;
        }

        [SerializeField] private LevelObjectCatalog catalog;
        [SerializeField] private Transform levelAnchor;
        [SerializeField] private Vector2 platformSize = new Vector2(8f, 6f);
        [SerializeField] private Vector3 unitSize = new Vector3(2.1f, 2.1f, 2.1f);
        [SerializeField] private int levels = 4;
        [SerializeField] private int level;
        [SerializeField] private int selectedIndex;
        [SerializeField] private float yaw;
        [SerializeField] private float rotationStep = 90f;
        [SerializeField] private BrushTool tool = BrushTool.Paint;
        [SerializeField] private bool brushEnabled;
        [SerializeField] private string levelId = "level_01";
        [SerializeField] private int ballCount = 5;
        [SerializeField] private float platformThickness = 0.5f;
        [SerializeField] private List<SavedPlacement> saved = new List<SavedPlacement>();

        private readonly LevelScenePreview preview = new LevelScenePreview();

        private LevelGrid grid;
        private GridDragState drag;
        private Vector2 scroll;

        [MenuItem("Tools/SmashFest/Level Brush")]
        private static void Open() => GetWindow<LevelBrushWindow>("Level Brush");

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            RebuildGrid();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            preview.Destroy();
            BrushFootprint.ClearCache();
        }

        private void OnUndoRedo()
        {
            ReloadGridFromSaved();
            RefreshPreview();
            Repaint();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawBrushToggle();
            DrawSettings();

            EditorGUILayout.Space();
            tool = LevelGridGui.DrawToolbar(tool);

            EditorGUILayout.Space();
            selectedIndex = BrushPaletteGui.Draw(catalog, selectedIndex, unitSize, yaw);

            EditorGUILayout.Space();
            DrawGrid();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Paint places the selected prefab. Rotate turns the object under the cursor " +
                "by the rotation step, re-fitting its cells. Erase removes it.\n" +
                "Paint and Erase can be dragged across cells. Shift-click always erases.\n" +
                "Brush active spawns a preview object in the scene; unchecking destroys it.",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void DrawGrid()
        {
            if (grid == null) return;

            level = LevelGridGui.DrawLevelTabs(level, levels);

            EditorGUILayout.Space();

            GridInput input = LevelGridGui.Draw(grid, level, ref drag);
            if (!input.Acted) return;

            BrushTool active = input.Shift ? BrushTool.Erase : tool;

            if (input.IsDrag && active == BrushTool.Rotate) return;

            Apply(active, input.Cell);
            Repaint();
        }

        private void DrawBrushToggle()
        {
            bool wasEnabled = brushEnabled;
            brushEnabled = EditorGUILayout.ToggleLeft("Brush active", brushEnabled);

            if (brushEnabled == wasEnabled) return;

            if (brushEnabled)
            {
                preview.Create(levelAnchor);
                RefreshPreview();
            }
            else
            {
                preview.Destroy();
            }
        }

        private void DrawSettings()
        {
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();

            catalog = (LevelObjectCatalog)EditorGUILayout.ObjectField(
                "Catalog", catalog, typeof(LevelObjectCatalog), false);
            levelAnchor = (Transform)EditorGUILayout.ObjectField(
                "Level Anchor", levelAnchor, typeof(Transform), true);

            platformSize = EditorGUILayout.Vector2Field("Platform Size (X, Z)", platformSize);
            unitSize = EditorGUILayout.Vector3Field("Unit Size", unitSize);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);

                if (GUILayout.Button("Measure unit from selected prefab"))
                    MeasureUnitFromSelection();
            }

            levels = Mathf.Max(1, EditorGUILayout.IntField("Levels", levels));

            if (EditorGUI.EndChangeCheck()) RebuildGrid();

            if (grid != null)
                EditorGUILayout.LabelField("Grid", $"{grid.Width} x {grid.Depth} cells, {grid.Height} levels");

            yaw = EditorGUILayout.FloatField("Paint Yaw", yaw);
            rotationStep = EditorGUILayout.FloatField("Rotation Step", rotationStep);

            EditorGUILayout.Space();
            levelId = EditorGUILayout.TextField("Level Id", levelId);
            ballCount = Mathf.Max(0, EditorGUILayout.IntField("Ball Count", ballCount));
            platformThickness = EditorGUILayout.FloatField("Platform Thickness", platformThickness);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(catalog == null))
                {
                    if (GUILayout.Button("Save Level JSON")) SaveLevel();
                    if (GUILayout.Button("Load Level JSON")) LoadLevel();
                }
            }
        }

        private GameObject SelectedPrefab =>
            catalog != null && catalog.Count > 0
                ? catalog.PrefabAt(Mathf.Clamp(selectedIndex, 0, catalog.Count - 1))
                : null;

        private void MeasureUnitFromSelection()
        {
            Footprint footprint = BrushFootprint.Of(SelectedPrefab);
            if (!footprint.Valid) return;

            unitSize = footprint.HalfExtents * 2f;
            RebuildGrid();
        }

        private void Apply(BrushTool active, CellIndex cell)
        {
            Undo.RecordObject(this, $"{active} level cell");

            bool changed;

            switch (active)
            {
                case BrushTool.Erase:
                    changed = grid.TryRemoveAt(cell);
                    break;
                case BrushTool.Rotate:
                    changed = TryRotate(cell);
                    break;
                default:
                    changed = TryPlaceSelected(cell);
                    break;
            }

            if (!changed) return;

            SaveGrid();
            RefreshPreview();
        }

        private bool TryPlaceSelected(CellIndex cell)
        {
            GameObject prefab = SelectedPrefab;
            if (prefab == null) return false;

            Footprint footprint = BrushFootprint.Of(prefab);
            if (!footprint.Valid) return false;

            string type = catalog.TypeAt(Mathf.Clamp(selectedIndex, 0, catalog.Count - 1));

            return grid.TryPlace(type, cell, BrushPaletteGui.SpanFor(footprint, unitSize, yaw), yaw);
        }

        private bool TryRotate(CellIndex cell)
        {
            if (!grid.TryGetAt(cell, out GridPlacement placement)) return false;
            if (catalog == null || !catalog.TryGetPrefab(placement.Type, out GameObject prefab)) return false;

            Footprint footprint = BrushFootprint.Of(prefab);
            if (!footprint.Valid) return false;

            float nextYaw = Mathf.Repeat(placement.Yaw + rotationStep, 360f);

            return grid.TryRotateAt(
                cell, BrushPaletteGui.SpanFor(footprint, unitSize, nextYaw), nextYaw);
        }

        private void ReloadGridFromSaved()
        {
            Vector2Int size = GridSpace.GridSizeFor(platformSize, new Vector2(unitSize.x, unitSize.z));

            var rebuilt = new LevelGrid(size.x, levels, size.y);

            for (int i = 0; i < saved.Count; i++)
            {
                SavedPlacement item = saved[i];

                rebuilt.TryPlace(
                    item.type,
                    new CellIndex(item.x, item.y, item.z),
                    new CellSpan(item.width, item.height, item.depth),
                    item.yaw);
            }

            grid = rebuilt;
            level = Mathf.Clamp(level, 0, levels - 1);
        }

        private void RebuildGrid()
        {
            ReloadGridFromSaved();

            SaveGrid();
            RefreshPreview();
        }

        private CellLayout Layout() =>
            new CellLayout(unitSize, preview.OriginFor(grid, unitSize, levelAnchor));

        private PlatformSpec[] PlatformSpecs()
        {
            Vector3 anchor = levelAnchor != null ? levelAnchor.position : Vector3.zero;

            return new[]
            {
                new PlatformSpec(
                    anchor - new Vector3(0f, platformThickness * 0.5f, 0f),
                    new Vector3(platformSize.x, platformThickness, platformSize.y))
            };
        }

        private void SaveLevel()
        {
            if (grid == null || catalog == null) return;

            LevelDefinition level = GridLevelConverter.ToDefinition(
                grid, Layout(), new CatalogObjectSizes(catalog),
                levelId, ballCount, PlatformSpecs());

            LevelJsonIo.Save(level, string.IsNullOrWhiteSpace(levelId) ? "level" : levelId);
        }

        private void LoadLevel()
        {
            if (grid == null || catalog == null) return;
            if (!LevelJsonIo.TryLoad(out LevelDefinition loaded)) return;

            Undo.RecordObject(this, "Load level");

            levelId = loaded.id;
            ballCount = loaded.ballCount;

            if (loaded.platforms.Length > 0)
            {
                Vector3 size = loaded.platforms[0].size;
                platformSize = new Vector2(size.x, size.z);
                platformThickness = size.y;
            }

            ReloadEmptyGrid();

            int placed = GridLevelConverter.Fill(grid, loaded, Layout(), new CatalogObjectSizes(catalog));

            if (placed < loaded.objects.Length)
            {
                Debug.LogWarning(
                    $"Loaded {placed} of {loaded.objects.Length} objects. " +
                    "The rest fell outside the grid or use unknown types.");
            }

            SaveGrid();
            RefreshPreview();
        }

        private void ReloadEmptyGrid()
        {
            Vector2Int size = GridSpace.GridSizeFor(platformSize, new Vector2(unitSize.x, unitSize.z));

            grid = new LevelGrid(size.x, levels, size.y);
            level = Mathf.Clamp(level, 0, levels - 1);
        }

        private void SaveGrid()
        {
            saved.Clear();

            for (int i = 0; i < grid.Count; i++)
            {
                GridPlacement placement = grid.PlacementAt(i);

                saved.Add(new SavedPlacement
                {
                    type = placement.Type,
                    x = placement.Origin.X,
                    y = placement.Origin.Y,
                    z = placement.Origin.Z,
                    width = placement.Span.Width,
                    height = placement.Span.Height,
                    depth = placement.Span.Depth,
                    yaw = placement.Yaw
                });
            }
        }

        private void RefreshPreview()
        {
            if (!preview.IsActive || grid == null || catalog == null) return;

            preview.Rebuild(grid, catalog, unitSize, preview.OriginFor(grid, unitSize, levelAnchor));
        }
    }
}
