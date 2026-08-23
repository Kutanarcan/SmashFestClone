using UnityEngine;

namespace Game.Core.Ballistics
{
    public readonly struct BallisticAim
    {
        private readonly BallisticSolver solver;
        private readonly float speed;
        private readonly ArcPreference arc;

        public BallisticAim(BallisticSolver solver, float speed, ArcPreference arc)
        {
            this.solver = solver;
            this.speed = speed;
            this.arc = arc;
        }

        public bool TryVelocityTo(Vector3 origin, Vector3 target, out Vector3 velocity)
        {
            if (solver.TrySolveVelocity(origin, target, speed, arc, out velocity))
                return true;

            velocity = solver.MaxRangeVelocity(origin, target, speed);
            return false;
        }
    }
}
