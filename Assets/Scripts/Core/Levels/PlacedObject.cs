using System;
using UnityEngine;

namespace Game.Core.Levels
{
    [Serializable]
    public struct PlacedObject
    {
        public string type;
        public Vector3 position;
        public Vector3 rotation;

        public PlacedObject(string type, Vector3 position, Vector3 rotation)
        {
            this.type = type;
            this.position = position;
            this.rotation = rotation;
        }
    }
}
