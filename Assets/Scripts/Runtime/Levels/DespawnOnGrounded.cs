using UnityEngine;

[RequireComponent(typeof(LevelObjective))]
public class DespawnOnGrounded : MonoBehaviour
{
    [SerializeField] private float delay = 1.5f;

    private LevelObjective objective;

    private void Awake()
    {
        objective = GetComponent<LevelObjective>();
        objective.Grounded += OnGrounded;
    }

    private void OnDestroy()
    {
        if (objective != null) objective.Grounded -= OnGrounded;
    }

    private void OnGrounded() => Destroy(gameObject, delay);
}
