using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Levels
{
    public static class GridLevelConverter
    {
        public static LevelDefinition ToDefinition(
            LevelGrid grid,
            CellLayout layout,
            ILevelObjectSizes sizes,
            string id,
            int ballCount,
            PlatformSpec[] platforms)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (sizes == null) throw new ArgumentNullException(nameof(sizes));

            var objects = new List<PlacedObject>(grid.Count);

            for (int i = 0; i < grid.Count; i++)
            {
                GridPlacement placement = grid.PlacementAt(i);

                if (!sizes.TryGet(placement.Type, out _, out Vector3 centerOffset)) continue;

                Quaternion rotation = Quaternion.Euler(0f, placement.Yaw, 0f);

                Vector3 colliderCenter = GridSpace.CellToWorldCenter(
                    placement.Origin, placement.Span, layout.UnitSize, layout.Origin);

                objects.Add(new PlacedObject(
                    placement.Type,
                    colliderCenter - rotation * centerOffset,
                    new Vector3(0f, placement.Yaw, 0f)));
            }

            return new LevelDefinition
            {
                id = id ?? string.Empty,
                ballCount = ballCount,
                platforms = platforms ?? Array.Empty<PlatformSpec>(),
                objects = objects.ToArray()
            };
        }

        public static int Fill(
            LevelGrid grid,
            LevelDefinition level,
            CellLayout layout,
            ILevelObjectSizes sizes)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (sizes == null) throw new ArgumentNullException(nameof(sizes));

            level.NormalizeArrays();

            int loaded = 0;

            for (int i = 0; i < level.objects.Length; i++)
            {
                PlacedObject placed = level.objects[i];

                if (!sizes.TryGet(placed.type, out Vector3 size, out Vector3 centerOffset)) continue;

                Quaternion rotation = Quaternion.Euler(placed.rotation);

                CellSpan span = GridSpace.SpanFor(
                    GridSpace.WorldAlignedSize(size, rotation), layout.UnitSize);

                Vector3 colliderCenter = placed.position + rotation * centerOffset;

                CellIndex cell = GridSpace.CellForBlockCenter(
                    colliderCenter, span, layout.UnitSize, layout.Origin);

                if (grid.TryPlace(placed.type, cell, span, placed.rotation.y)) loaded++;
            }

            return loaded;
        }
    }
}
