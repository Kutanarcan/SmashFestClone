using System;
using Game.Core;
using Game.Core.Levels;
using Game.Runtime.Levels;
using UnityEngine;

public class LevelFlowController : MonoBehaviour, ITickable
{
    [SerializeField] private LevelLoader loader;

    [SerializeField] private TextAsset[] levels;

    private BallRegistry ballRegistry;
    private LevelProgress progress;

    public LevelSession Session { get; private set; }

    public LevelProgress Progress => progress;

    public event Action<LevelSession> LevelStarted;

    public event Action<LevelState> LevelFinished;

    public void Initialize(BallRegistry registry)
    {
        ballRegistry = registry;
        progress = new LevelProgress(levels != null ? levels.Length : 0);

        StartCurrent();
    }

    public void Tick(float deltaTime) => Session?.Tick(deltaTime);

    public void Restart() => StartCurrent();

    public bool Next()
    {
        if (progress == null || !progress.TryAdvance()) return false;

        StartCurrent();
        return true;
    }

    private void StartCurrent()
    {
        if (loader == null || progress == null || !progress.HasLevels)
        {
            Debug.LogError($"{nameof(LevelFlowController)}: no loader or no levels assigned.", this);
            return;
        }

        Detach();

        Session = loader.Load(levels[progress.CurrentIndex], ballRegistry);

        if (Session == null) return;

        Session.Finished += OnFinished;
        LevelStarted?.Invoke(Session);
    }

    private void Detach()
    {
        if (Session == null) return;

        Session.Finished -= OnFinished;
        Session = null;
    }

    private void OnDestroy() => Detach();

    private void OnFinished(LevelState state) => LevelFinished?.Invoke(state);
}
