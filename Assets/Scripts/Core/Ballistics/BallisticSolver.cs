using UnityEngine;

namespace Game.Core.Ballistics
{
    public readonly struct BallisticSolver
    {
        private const float MinHorizontalDistance = 0.0001f;

        private readonly float gravity;

        public BallisticSolver(float gravity)
        {
            this.gravity = gravity;
        }

        public bool TrySolveVelocity(
            Vector3 origin,
            Vector3 target,
            float speed,
            ArcPreference arc,
            out Vector3 velocity)
        {
            Vector3 delta = target - origin;

            if (gravity <= 0f)
            {
                velocity = delta.sqrMagnitude < MinHorizontalDistance
                    ? Vector3.zero
                    : delta.normalized * speed;
                return velocity != Vector3.zero;
            }

            Vector3 horizontal = new Vector3(delta.x, 0f, delta.z);
            float x = horizontal.magnitude;

            if (x < MinHorizontalDistance)
                return TrySolveVertical(delta.y, speed, out velocity);

            float v2 = speed * speed;
            float discriminant = v2 * v2 - gravity * (gravity * x * x + 2f * delta.y * v2);

            if (discriminant < 0f)
            {
                velocity = default;
                return false;
            }

            float root = Mathf.Sqrt(discriminant);
            float numerator = arc == ArcPreference.High ? v2 + root : v2 - root;
            float angle = Mathf.Atan2(numerator, gravity * x);

            velocity = Compose(horizontal / x, angle, speed);
            return true;
        }

        public Vector3 MaxRangeVelocity(Vector3 origin, Vector3 target, float speed)
        {
            Vector3 delta = target - origin;
            Vector3 horizontal = new Vector3(delta.x, 0f, delta.z);
            float x = horizontal.magnitude;

            if (x < MinHorizontalDistance)
                return Vector3.up * speed;

            if (gravity <= 0f)
                return horizontal / x * speed;

            float angle = Mathf.Atan2(speed * speed, gravity * x);
            return Compose(horizontal / x, angle, speed);
        }

        private bool TrySolveVertical(float height, float speed, out Vector3 velocity)
        {
            if (height <= 0f)
            {
                velocity = Vector3.down * speed;
                return true;
            }

            if (speed * speed < 2f * gravity * height)
            {
                velocity = default;
                return false;
            }

            velocity = Vector3.up * speed;
            return true;
        }

        private static Vector3 Compose(Vector3 horizontalDirection, float angle, float speed)
        {
            return horizontalDirection * (speed * Mathf.Cos(angle))
                 + Vector3.up * (speed * Mathf.Sin(angle));
        }
    }
}
