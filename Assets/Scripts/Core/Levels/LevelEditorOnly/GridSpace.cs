using UnityEngine;

namespace Game.Core.Levels
{
    public static class GridSpace
    {
        public static Vector3 WorldAlignedSize(Vector3 size, Quaternion rotation)
        {
            Matrix4x4 m = Matrix4x4.Rotate(rotation);

            return new Vector3(
                Mathf.Abs(m.m00) * size.x + Mathf.Abs(m.m01) * size.y + Mathf.Abs(m.m02) * size.z,
                Mathf.Abs(m.m10) * size.x + Mathf.Abs(m.m11) * size.y + Mathf.Abs(m.m12) * size.z,
                Mathf.Abs(m.m20) * size.x + Mathf.Abs(m.m21) * size.y + Mathf.Abs(m.m22) * size.z);
        }

        public static CellSpan SpanFor(Vector3 worldSize, Vector3 unitSize)
        {
            return new CellSpan(
                CellsFor(worldSize.x, unitSize.x),
                CellsFor(worldSize.y, unitSize.y),
                CellsFor(worldSize.z, unitSize.z));
        }

        public static Vector3 CellToWorldCenter(
            CellIndex origin, CellSpan span, Vector3 unitSize, Vector3 gridOrigin)
        {
            var minCorner = new Vector3(
                gridOrigin.x + origin.X * unitSize.x,
                gridOrigin.y + origin.Y * unitSize.y,
                gridOrigin.z + origin.Z * unitSize.z);

            var halfBlock = new Vector3(
                span.Width * unitSize.x * 0.5f,
                span.Height * unitSize.y * 0.5f,
                span.Depth * unitSize.z * 0.5f);

            return minCorner + halfBlock;
        }

        public static CellIndex CellForBlockCenter(
            Vector3 center, CellSpan span, Vector3 unitSize, Vector3 gridOrigin)
        {
            return new CellIndex(
                RoundAxis(center.x - gridOrigin.x - span.Width * unitSize.x * 0.5f, unitSize.x),
                RoundAxis(center.y - gridOrigin.y - span.Height * unitSize.y * 0.5f, unitSize.y),
                RoundAxis(center.z - gridOrigin.z - span.Depth * unitSize.z * 0.5f, unitSize.z));
        }

        public static CellIndex WorldToCell(Vector3 world, Vector3 unitSize, Vector3 gridOrigin)
        {
            return new CellIndex(
                FloorAxis(world.x - gridOrigin.x, unitSize.x),
                FloorAxis(world.y - gridOrigin.y, unitSize.y),
                FloorAxis(world.z - gridOrigin.z, unitSize.z));
        }

        public static Vector2Int GridSizeFor(Vector2 platformSize, Vector2 unitSize)
        {
            return new Vector2Int(
                Mathf.Max(1, Mathf.FloorToInt(SafeDivide(platformSize.x, unitSize.x))),
                Mathf.Max(1, Mathf.FloorToInt(SafeDivide(platformSize.y, unitSize.y))));
        }

        private static int CellsFor(float size, float unit)
        {
            if (unit <= 0f) return 1;

            return Mathf.Max(1, Mathf.RoundToInt(size / unit));
        }

        private static int RoundAxis(float offset, float unit)
        {
            if (unit <= 0f) return 0;

            return Mathf.RoundToInt(offset / unit);
        }

        private static int FloorAxis(float offset, float unit)
        {
            if (unit <= 0f) return 0;

            return Mathf.FloorToInt(offset / unit);
        }

        private static float SafeDivide(float value, float divisor)
        {
            return divisor <= 0f ? 1f : value / divisor;
        }
    }
}
