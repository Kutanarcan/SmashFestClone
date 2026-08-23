using UnityEngine;

namespace Game.Core.Impacts
{
    public readonly struct ImpactEvent
    {
        public readonly ImpactSource Source;
        public readonly float Speed;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;

        public ImpactEvent(ImpactSource source, float speed, Vector3 point, Vector3 normal)
        {
            Source = source;
            Speed = speed;
            Point = point;
            Normal = normal;
        }
    }
}
