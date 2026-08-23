using Game.Core.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public struct GridDragState
    {
        public bool Dragging;
        public bool HasLast;
        public CellIndex LastCell;
    }

    public struct GridInput
    {
        public bool Acted;
        public bool IsDrag;
        public bool Shift;
        public CellIndex Cell;
    }

    public static class LevelGridGui
    {
        private const float CellSize = 34f;

        private static readonly Color OriginColor = new Color(0.35f, 0.75f, 1f);
        private static readonly Color SpanColor = new Color(0.25f, 0.45f, 0.6f);
        private static readonly Color BelowColor = new Color(0.45f, 0.45f, 0.45f);

        public static BrushTool DrawToolbar(BrushTool tool)
        {
            var labels = new[] { "Paint", "Rotate", "Erase" };

            return (BrushTool)GUILayout.Toolbar((int)tool, labels);
        }

        public static int DrawLevelTabs(int level, int levels)
        {
            var labels = new string[levels];
            for (int i = 0; i < levels; i++) labels[i] = $"Level {i}";

            return GUILayout.Toolbar(Mathf.Clamp(level, 0, levels - 1), labels);
        }

        public static GridInput Draw(LevelGrid grid, int level, ref GridDragState drag)
        {
            var input = new GridInput();

            if (grid == null) return input;

            Event current = Event.current;

            if (current.rawType == EventType.MouseUp && current.button == 0)
            {
                drag.Dragging = false;
                drag.HasLast = false;
            }

            Color previous = GUI.backgroundColor;

            for (int z = grid.Depth - 1; z >= 0; z--)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < grid.Width; x++)
                {
                    var cell = new CellIndex(x, level, z);

                    Rect rect = GUILayoutUtility.GetRect(
                        CellSize, CellSize, GUILayout.Width(CellSize), GUILayout.Height(CellSize));

                    GUI.backgroundColor = BackgroundFor(grid, cell, out string label);
                    GUI.Box(rect, label, GUI.skin.button);

                    HandleCell(rect, cell, current, ref drag, ref input);
                }

                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = previous;

            return input;
        }

        private static void HandleCell(
            Rect rect, CellIndex cell, Event current, ref GridDragState drag, ref GridInput input)
        {
            if (current.button != 0 || !rect.Contains(current.mousePosition)) return;

            if (current.type == EventType.MouseDown)
            {
                drag.Dragging = true;
                Report(cell, current, false, ref drag, ref input);
                current.Use();
                return;
            }

            if (current.type != EventType.MouseDrag || !drag.Dragging) return;
            if (drag.HasLast && drag.LastCell.Equals(cell)) return;

            Report(cell, current, true, ref drag, ref input);
            current.Use();
        }

        private static void Report(
            CellIndex cell, Event current, bool isDrag, ref GridDragState drag, ref GridInput input)
        {
            drag.LastCell = cell;
            drag.HasLast = true;

            input.Acted = true;
            input.IsDrag = isDrag;
            input.Shift = current.shift;
            input.Cell = cell;
        }

        private static Color BackgroundFor(LevelGrid grid, CellIndex cell, out string label)
        {
            if (grid.TryGetAt(cell, out GridPlacement placement))
            {
                bool isOrigin = placement.Origin.Equals(cell);

                label = isOrigin ? Marker(placement) : Initial(placement.Type);
                return isOrigin ? OriginColor : SpanColor;
            }

            if (cell.Y > 0
                && grid.TryGetAt(new CellIndex(cell.X, cell.Y - 1, cell.Z), out GridPlacement below))
            {
                label = Initial(below.Type).ToLowerInvariant();
                return BelowColor;
            }

            label = string.Empty;
            return Color.white;
        }

        private static string Marker(GridPlacement placement)
        {
            string initial = Initial(placement.Type);

            return Mathf.Abs(placement.Yaw) < 0.01f ? initial : $"{initial}↻";
        }

        private static string Initial(string type) =>
            string.IsNullOrEmpty(type) ? "?" : type.Substring(0, 1).ToUpperInvariant();
    }
}
