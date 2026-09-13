using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Levels
{
    public sealed class LevelGrid
    {
        private readonly int[] cells;
        private readonly List<GridPlacement> placements = new List<GridPlacement>();

        public LevelGrid(int width, int height, int depth)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            Depth = Mathf.Max(1, depth);

            cells = new int[Width * Height * Depth];
        }

        public int Width { get; }

        public int Height { get; }

        public int Depth { get; }

        public int Count => placements.Count;

        public GridPlacement PlacementAt(int index) => placements[index];

        public bool Contains(CellIndex cell) =>
            cell.X >= 0 && cell.X < Width &&
            cell.Y >= 0 && cell.Y < Height &&
            cell.Z >= 0 && cell.Z < Depth;

        public bool Fits(CellIndex origin, CellSpan span) =>
            Contains(origin) &&
            origin.X + span.Width <= Width &&
            origin.Y + span.Height <= Height &&
            origin.Z + span.Depth <= Depth;

        public bool CanPlace(CellIndex origin, CellSpan span)
        {
            if (!Fits(origin, span)) return false;

            for (int y = 0; y < span.Height; y++)
            for (int z = 0; z < span.Depth; z++)
            for (int x = 0; x < span.Width; x++)
            {
                if (cells[IndexOf(origin.X + x, origin.Y + y, origin.Z + z)] != 0) return false;
            }

            return true;
        }

        public bool TryPlace(string type, CellIndex origin, CellSpan span, float yaw)
        {
            if (string.IsNullOrWhiteSpace(type)) return false;
            if (!CanPlace(origin, span)) return false;

            placements.Add(new GridPlacement(type, origin, span, yaw));
            Stamp(origin, span, placements.Count);

            return true;
        }

        public bool TryGetAt(CellIndex cell, out GridPlacement placement)
        {
            placement = default;

            if (!Contains(cell)) return false;

            int stamp = cells[IndexOf(cell.X, cell.Y, cell.Z)];
            if (stamp == 0) return false;

            placement = placements[stamp - 1];
            return true;
        }

        public bool TryRemoveAt(CellIndex cell)
        {
            if (!Contains(cell)) return false;

            int stamp = cells[IndexOf(cell.X, cell.Y, cell.Z)];
            if (stamp == 0) return false;

            int index = stamp - 1;
            GridPlacement removed = placements[index];
            Stamp(removed.Origin, removed.Span, 0);

            int last = placements.Count - 1;

            if (index != last)
            {
                GridPlacement moved = placements[last];
                placements[index] = moved;
                Stamp(moved.Origin, moved.Span, index + 1);
            }

            placements.RemoveAt(last);
            return true;
        }

        public bool TryRotateAt(CellIndex cell, CellSpan newSpan, float newYaw)
        {
            if (!TryGetAt(cell, out GridPlacement existing)) return false;

            CellIndex origin = existing.Origin;

            TryRemoveAt(cell);

            if (TryPlace(existing.Type, origin, newSpan, newYaw)) return true;

            TryPlace(existing.Type, origin, existing.Span, existing.Yaw);
            return false;
        }

        public void Clear()
        {
            Array.Clear(cells, 0, cells.Length);
            placements.Clear();
        }

        private void Stamp(CellIndex origin, CellSpan span, int stamp)
        {
            for (int y = 0; y < span.Height; y++)
            for (int z = 0; z < span.Depth; z++)
            for (int x = 0; x < span.Width; x++)
            {
                cells[IndexOf(origin.X + x, origin.Y + y, origin.Z + z)] = stamp;
            }
        }

        private int IndexOf(int x, int y, int z) => (y * Depth + z) * Width + x;
    }
}
