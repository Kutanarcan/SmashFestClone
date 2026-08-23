using System.Collections.Generic;
using Game.Core;
using Game.Core.Firing;
using Game.Core.Inputs;
using Game.Core.Levels;
using Game.Core.Timing;
using Game.Runtime.Inputs;
using Game.Runtime.Levels;
using Game.Runtime.Timing;
using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [SerializeField] private CannonController cannon;

    [SerializeField] private LevelLoader levelLoader;

    private readonly List<ITickable> tickables = new List<ITickable>();

    private TickLoop tickLoop;
    private LevelSession session;

    private void Awake()
    {
        if (cannon == null)
        {
            Debug.LogError($"{nameof(GameInstaller)}: cannon is not assigned.", this);
            enabled = false;
            return;
        }

        IPointerInputSource input = new PointerInputSource();
        ITimeProvider time = new UnityTimeProvider();

        var ballRegistry = new BallRegistry();

        session = levelLoader != null ? levelLoader.Load(ballRegistry) : null;

        IAmmoSource ammo = session != null ? (IAmmoSource)session : new UnlimitedAmmo();

        cannon.Initialize(input, time, ammo, ballRegistry);

        tickables.Clear();
        tickables.Add(cannon);

        if (session != null)
        {
            session.Finished += OnLevelFinished;
            tickables.Add(session);
        }

        tickLoop = new TickLoop(tickables.ToArray());
    }

    private void OnDestroy()
    {
        if (session != null) session.Finished -= OnLevelFinished;
    }

    private void Update()
    {
        tickLoop.Tick(Time.deltaTime);
    }

    private void OnLevelFinished(LevelState state)
    {
        Debug.Log($"Level {state}. Balls left: {session.BallsRemaining}, objectives left: {session.ObjectivesRemaining}.");
    }
}
