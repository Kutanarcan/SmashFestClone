using System;

namespace Game.Core.Levels
{
    [Serializable]
    public readonly struct CellIndex : IEquatable<CellIndex>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public CellIndex(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(CellIndex other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is CellIndex other && Equals(other);

        public override int GetHashCode() => (X * 397 ^ Y) * 397 ^ Z;

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
