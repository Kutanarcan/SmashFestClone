using System.Collections.Generic;
using Game.Core.Levels;
using Game.Runtime.Levels;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private TextAsset levelJson;

    [Header("Spawning")]
    [SerializeField] private LevelObjectCatalog catalog;

    [SerializeField] private GameObject platformPrefab;

    [SerializeField] private Transform levelRoot;

    [Header("Rules")]
    [SerializeField] private LayerMask groundLayers;

    [SerializeField] private float settleTimeout = 3f;

    private readonly List<LevelObjective> objectives = new List<LevelObjective>();
    private readonly List<Rigidbody> objectiveBodies = new List<Rigidbody>();

    public LevelSession Load(BallRegistry ballRegistry)
    {
        if (levelJson == null)
        {
            Debug.LogError($"{nameof(LevelLoader)}: no level JSON assigned.", this);
            return null;
        }

        if (!LevelSerializer.TryFromJson(levelJson.text, out LevelDefinition level, out string error))
        {
            Debug.LogError($"{nameof(LevelLoader)}: {error}", this);
            return null;
        }

        objectives.Clear();
        objectiveBodies.Clear();

        SpawnPlatforms(level);
        SpawnObjects(level);

        var restQuery = new UnityWorldRestQuery(ballRegistry, objectiveBodies);
        var session = new LevelSession(objectives.Count, level.ballCount, restQuery, settleTimeout);

        for (int i = 0; i < objectives.Count; i++)
            objectives[i].Initialize(i, session, groundLayers);

        return session;
    }

    private void SpawnPlatforms(LevelDefinition level)
    {
        if (platformPrefab == null)
        {
            Debug.LogError($"{nameof(LevelLoader)}: no platform prefab assigned.", this);
            return;
        }

        for (int i = 0; i < level.platforms.Length; i++)
        {
            PlatformSpec spec = level.platforms[i];

            GameObject instance = Instantiate(
                platformPrefab, spec.position, Quaternion.identity, levelRoot);

            PlatformView view = instance.GetComponent<PlatformView>();

            if (view != null)
                view.SetSize(spec.size);
            else
                Debug.LogWarning($"{nameof(LevelLoader)}: platform prefab has no {nameof(PlatformView)}, size ignored.", this);
        }
    }

    private void SpawnObjects(LevelDefinition level)
    {
        if (catalog == null)
        {
            Debug.LogError($"{nameof(LevelLoader)}: no catalog assigned.", this);
            return;
        }

        for (int i = 0; i < level.objects.Length; i++)
        {
            PlacedObject placed = level.objects[i];

            if (!catalog.TryGetPrefab(placed.type, out GameObject prefab))
            {
                Debug.LogError($"{nameof(LevelLoader)}: object {i} has unknown type '{placed.type}'.", this);
                continue;
            }

            GameObject instance = Instantiate(
                prefab, placed.position, Quaternion.Euler(placed.rotation), levelRoot);

            LevelObjective objective = instance.GetComponent<LevelObjective>();

            if (objective == null)
            {
                Debug.LogError($"{nameof(LevelLoader)}: prefab '{placed.type}' has no {nameof(LevelObjective)}.", this);
                continue;
            }

            objectives.Add(objective);
            objectiveBodies.Add(instance.GetComponent<Rigidbody>());
        }
    }
}
