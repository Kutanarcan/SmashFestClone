using System;
using Game.Core.Aiming;
using Game.Core.Firing;
using Game.Core.Inputs;
using UnityEngine;

namespace Game.Core.Cannon
{
    public sealed class CannonBrain : ITickable
    {
        private readonly IPointerInputSource input;
        private readonly IAimRaycaster raycaster;
        private readonly IBarrelView barrel;
        private readonly IBallLauncher launcher;
        private readonly FireControl fireControl;
        private readonly BarrelAim barrelAim;

        private Vector3 aimPoint;

        public CannonBrain(
            IPointerInputSource input,
            IAimRaycaster raycaster,
            IBarrelView barrel,
            IBallLauncher launcher,
            FireControl fireControl,
            BarrelAim barrelAim)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.raycaster = raycaster ?? throw new ArgumentNullException(nameof(raycaster));
            this.barrel = barrel ?? throw new ArgumentNullException(nameof(barrel));
            this.launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            this.fireControl = fireControl ?? throw new ArgumentNullException(nameof(fireControl));
            this.barrelAim = barrelAim;
        }

        public bool HasAimed { get; private set; }

        public Vector3 AimPoint => aimPoint;

        public void Tick(float deltaTime)
        {
            if (input.WasPressedThisFrame && !input.IsOverUI)
                HandleTap(input.Position);

            UpdateBarrel(deltaTime);
        }

        private void HandleTap(Vector2 screenPosition)
        {
            if (!raycaster.TryResolve(screenPosition, out Vector3 worldPoint))
                return;

            aimPoint = worldPoint;
            HasAimed = true;

            if (!fireControl.TryFire())
                return;

            launcher.LaunchTowards(aimPoint);
        }

        private void UpdateBarrel(float deltaTime)
        {
            if (!HasAimed) return;

            if (!barrel.TryGetLocalDirection(aimPoint, out Vector3 localDirection))
                return;

            if (!barrelAim.TryResolve(localDirection, out AimAngles angles))
                return;

            barrel.Apply(angles, deltaTime);
        }
    }
}
