using System;
using UnityEngine;

namespace Game.Core.Levels
{
    [Serializable]
    public readonly struct CellSpan : IEquatable<CellSpan>
    {
        public static readonly CellSpan Single = new CellSpan(1, 1, 1);

        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;

        public CellSpan(int width, int height, int depth)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            Depth = Mathf.Max(1, depth);
        }

        public int CellCount => Width * Height * Depth;

        public bool Equals(CellSpan other) =>
            Width == other.Width && Height == other.Height && Depth == other.Depth;

        public override bool Equals(object obj) => obj is CellSpan other && Equals(other);

        public override int GetHashCode() => (Width * 397 ^ Height) * 397 ^ Depth;

        public override string ToString() => $"{Width}x{Height}x{Depth}";
    }
}
