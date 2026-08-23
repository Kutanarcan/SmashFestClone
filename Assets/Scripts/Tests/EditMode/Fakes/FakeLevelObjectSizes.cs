using System.Collections.Generic;
using Game.Core.Levels;
using UnityEngine;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakeLevelObjectSizes : ILevelObjectSizes
    {
        private readonly Dictionary<string, Vector3> sizes = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Vector3> offsets = new Dictionary<string, Vector3>();

        public FakeLevelObjectSizes Add(string type, Vector3 size) => Add(type, size, Vector3.zero);

        public FakeLevelObjectSizes Add(string type, Vector3 size, Vector3 centerOffset)
        {
            sizes[type] = size;
            offsets[type] = centerOffset;
            return this;
        }

        public bool TryGet(string type, out Vector3 size, out Vector3 centerOffset)
        {
            centerOffset = Vector3.zero;

            if (type == null || !sizes.TryGetValue(type, out size))
            {
                size = Vector3.zero;
                return false;
            }

            centerOffset = offsets[type];
            return true;
        }
    }
}
