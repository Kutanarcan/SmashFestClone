using System;
using UnityEngine;

namespace Game.Core.Levels
{
    [Serializable]
    public struct PlatformSpec
    {
        public Vector3 position;
        public Vector3 size;

        public PlatformSpec(Vector3 position, Vector3 size)
        {
            this.position = position;
            this.size = size;
        }
    }
}
