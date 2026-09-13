using System;

namespace Game.Core.Levels
{
    [Serializable]
    public readonly struct GridPlacement
    {
        public readonly string Type;
        public readonly CellIndex Origin;
        public readonly CellSpan Span;
        public readonly float Yaw;

        public GridPlacement(string type, CellIndex origin, CellSpan span, float yaw)
        {
            Type = type;
            Origin = origin;
            Span = span;
            Yaw = yaw;
        }
    }
}
