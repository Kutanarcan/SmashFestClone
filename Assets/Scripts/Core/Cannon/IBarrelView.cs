using Game.Core.Aiming;
using UnityEngine;

namespace Game.Core.Cannon
{
    public interface IBarrelView
    {
        bool TryToLocalDirection(Vector3 worldDirection, out Vector3 localDirection);

        void Apply(AimAngles angles, float deltaTime);
    }
}
