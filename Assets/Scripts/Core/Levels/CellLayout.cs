using UnityEngine;

namespace Game.Core.Levels
{
    public readonly struct CellLayout
    {
        public readonly Vector3 UnitSize;
        public readonly Vector3 Origin;

        public CellLayout(Vector3 unitSize, Vector3 origin)
        {
            UnitSize = unitSize;
            Origin = origin;
        }
    }
}
