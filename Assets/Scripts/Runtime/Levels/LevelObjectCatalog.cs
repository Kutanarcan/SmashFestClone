using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelObjectCatalog", menuName = "SmashFest/Level Object Catalog")]
public class LevelObjectCatalog : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string type;
        public GameObject prefab;
    }

    [SerializeField] private Entry[] entries;

    public bool TryGetPrefab(string type, out GameObject prefab)
    {
        prefab = null;

        if (entries == null || string.IsNullOrEmpty(type)) return false;

        for (int i = 0; i < entries.Length; i++)
        {
            if (!string.Equals(entries[i].type, type, StringComparison.Ordinal)) continue;

            prefab = entries[i].prefab;
            return prefab != null;
        }

        return false;
    }
}
