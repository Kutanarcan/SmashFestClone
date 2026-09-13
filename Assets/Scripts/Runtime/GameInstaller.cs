using System.Collections.Generic;
using Game.Core;
using Game.Core.Firing;
using Game.Core.Inputs;
using Game.Core.Timing;
using Game.Runtime.Inputs;
using Game.Runtime.Levels;
using Game.Runtime.Pooling;
using Game.Runtime.Timing;
using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private CannonController cannon;

    [SerializeField] private LevelFlowController flow;

    [SerializeField] private LevelHud hud;

    [Header("Ball Pool")]
    [SerializeField] private CannonBall ballPrefab;

    [SerializeField] private int ballPrewarm = 8;

    private readonly List<ITickable> tickables = new List<ITickable>();

    private TickLoop tickLoop;

    private void Awake()
    {
        if (cannon == null || flow == null || ballPrefab == null)
        {
            Debug.LogError($"{nameof(GameInstaller)}: cannon, flow and ball prefab must all be assigned.", this);
            enabled = false;
            return;
        }

        Physics.reuseCollisionCallbacks = true;
        Application.targetFrameRate = 60;

        var poolRoot = new GameObject("BallPool").transform;
        poolRoot.SetParent(transform, false);

        var pool = new GameObjectPool<CannonBall>(ballPrefab, poolRoot, ballPrewarm);
        var ballRegistry = new BallRegistry();

        IPointerInputSource input = new PointerInputSource();
        ITimeProvider time = new UnityTimeProvider();
        IAmmoSource ammo = new CurrentLevelAmmo(flow);

        cannon.Initialize(input, time, ammo, pool, ballRegistry);

        tickables.Clear();
        tickables.Add(cannon);
        tickables.Add(flow);

        if (hud != null)
        {
            hud.Initialize(flow);
            tickables.Add(hud);
        }

        tickLoop = new TickLoop(tickables.ToArray());

        flow.Initialize(ballRegistry);
    }

    private void Update()
    {
        tickLoop.Tick(Time.deltaTime);
    }
}
