using UnityEngine;

namespace Game.Core.Cannon
{
    public interface IBallLauncher
    {
        Vector3 MuzzlePosition { get; }

        void Launch(Vector3 velocity);
    }
}
