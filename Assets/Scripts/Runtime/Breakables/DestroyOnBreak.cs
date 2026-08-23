using Game.Core.Impacts;
using UnityEngine;

[RequireComponent(typeof(Breakable))]
public class DestroyOnBreak : MonoBehaviour
{
    [SerializeField] private float delay;

    private Breakable breakable;

    private void Awake()
    {
        breakable = GetComponent<Breakable>();
        breakable.Broke += OnBroke;
    }

    private void OnDestroy()
    {
        if (breakable != null) breakable.Broke -= OnBroke;
    }

    private void OnBroke(in ImpactEvent impact) => Destroy(gameObject, delay);
}
