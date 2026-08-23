using System;
using UnityEngine;

namespace Game.Core.Levels
{
    public static class LevelSerializer
    {
        public static string ToJson(LevelDefinition level, bool prettyPrint = true)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));

            level.version = LevelDefinition.CurrentVersion;
            level.NormalizeArrays();

            return JsonUtility.ToJson(level, prettyPrint);
        }

        public static bool TryFromJson(string json, out LevelDefinition level, out string error)
        {
            level = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Level JSON is empty.";
                return false;
            }

            LevelDefinition parsed;
            try
            {
                parsed = JsonUtility.FromJson<LevelDefinition>(json);
            }
            catch (Exception exception)
            {
                error = $"Level JSON is malformed: {exception.Message}";
                return false;
            }

            if (parsed == null)
            {
                error = "Level JSON did not describe an object.";
                return false;
            }

            parsed.NormalizeArrays();

            if (!Validate(parsed, out error))
                return false;

            level = parsed;
            error = null;
            return true;
        }

        private static bool Validate(LevelDefinition level, out string error)
        {
            if (level.version != LevelDefinition.CurrentVersion)
            {
                error = $"Unsupported level version {level.version}, expected {LevelDefinition.CurrentVersion}.";
                return false;
            }

            if (level.ballCount < 0)
            {
                error = $"Ball count cannot be negative (was {level.ballCount}).";
                return false;
            }

            return ValidatePlatforms(level, out error) && ValidateObjects(level, out error);
        }

        private static bool ValidatePlatforms(LevelDefinition level, out string error)
        {
            for (int i = 0; i < level.platforms.Length; i++)
            {
                Vector3 size = level.platforms[i].size;

                if (size.x <= 0f || size.y <= 0f || size.z <= 0f)
                {
                    error = $"Platform {i} has a non-positive size {size}.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static bool ValidateObjects(LevelDefinition level, out string error)
        {
            for (int i = 0; i < level.objects.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(level.objects[i].type))
                {
                    error = $"Object {i} has no type.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
