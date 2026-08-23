using Game.Core.Aiming;
using UnityEngine;

namespace Game.Core.Cannon
{
    public interface IBarrelView
    {
        bool TryGetLocalDirection(Vector3 worldTarget, out Vector3 localDirection);

        void Apply(AimAngles angles, float deltaTime);
    }
}
