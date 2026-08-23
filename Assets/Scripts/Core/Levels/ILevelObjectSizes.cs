using UnityEngine;

namespace Game.Core.Levels
{
    public interface ILevelObjectSizes
    {
        bool TryGet(string type, out Vector3 size, out Vector3 centerOffset);
    }
}
