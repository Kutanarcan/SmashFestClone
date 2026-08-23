using UnityEngine;

namespace Game.Core.Cannon
{
    public interface IBallLauncher
    {
        void LaunchTowards(Vector3 worldTarget);
    }
}
