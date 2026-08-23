using Game.Core;
using Game.Core.Inputs;
using Game.Core.Timing;
using Game.Runtime.Inputs;
using Game.Runtime.Timing;
using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [SerializeField] private CannonController cannon;

    private TickLoop tickLoop;

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

        cannon.Initialize(input, time);

        tickLoop = new TickLoop(new ITickable[] { cannon });
    }

    private void Update()
    {
        tickLoop.Tick(Time.deltaTime);
    }
}
