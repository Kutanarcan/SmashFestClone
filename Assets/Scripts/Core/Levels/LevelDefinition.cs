using System;

namespace Game.Core.Levels
{
    [Serializable]
    public class LevelDefinition
    {
        public const int CurrentVersion = 1;

        public int version;
        public string id;
        public int ballCount;
        public PlatformSpec[] platforms;
        public PlacedObject[] objects;

        public LevelDefinition()
        {
            id = string.Empty;
            platforms = Array.Empty<PlatformSpec>();
            objects = Array.Empty<PlacedObject>();
        }

        public void NormalizeArrays()
        {
            platforms ??= Array.Empty<PlatformSpec>();
            objects ??= Array.Empty<PlacedObject>();
        }
    }
}
