using System.IO;
using Game.Core.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class LevelJsonIo
    {
        private const string DefaultFolder = "Assets/Levels";

        public static bool Save(LevelDefinition level, string suggestedName)
        {
            string path = EditorUtility.SaveFilePanel(
                "Save Level", FolderOrAssets(), $"{suggestedName}.json", "json");

            if (string.IsNullOrEmpty(path)) return false;

            File.WriteAllText(path, LevelSerializer.ToJson(level));

            if (path.StartsWith(Application.dataPath)) AssetDatabase.Refresh();

            Debug.Log($"Saved level to {path}");
            return true;
        }

        public static bool TryLoad(out LevelDefinition level)
        {
            level = null;

            string path = EditorUtility.OpenFilePanel("Load Level", FolderOrAssets(), "json");
            if (string.IsNullOrEmpty(path)) return false;

            string json = File.ReadAllText(path);

            if (LevelSerializer.TryFromJson(json, out level, out string error)) return true;

            EditorUtility.DisplayDialog("Load Level", error, "OK");
            return false;
        }

        private static string FolderOrAssets() =>
            Directory.Exists(DefaultFolder) ? DefaultFolder : "Assets";
    }
}
