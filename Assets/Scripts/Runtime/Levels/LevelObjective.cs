using System;
using Game.Core.Impacts;
using Game.Core.Levels;
using UnityEngine;

public class LevelObjective : MonoBehaviour
{
    private LevelSession session;
    private Breakable breakable;
    private LayerMask groundLayers;
    private int index;
    private bool reported;
    private bool grounded;

    public event Action Grounded;

    public void Initialize(int index, LevelSession session, LayerMask groundLayers)
    {
        this.index = index;
        this.session = session;
        this.groundLayers = groundLayers;

        breakable = GetComponent<Breakable>();
        if (breakable != null) breakable.Broke += OnBroke;
    }

    private void OnDestroy()
    {
        if (breakable != null) breakable.Broke -= OnBroke;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((groundLayers.value & (1 << collision.gameObject.layer)) == 0) return;

        Report();

        if (grounded) return;

        grounded = true;
        Grounded?.Invoke();
    }

    private void OnBroke(in ImpactEvent impact) => Report();

    private void Report()
    {
        if (reported || session == null) return;

        reported = true;
        session.ClearObjective(index);
    }
}
