using Game.Core;
using Game.Core.Aiming;
using Game.Core.Ballistics;
using Game.Core.Cannon;
using Game.Core.Firing;
using Game.Core.Inputs;
using Game.Core.Timing;
using Game.Runtime.Cannon;
using Game.Runtime.Levels;
using UnityEngine;

public class CannonController : MonoBehaviour, ITickable
{
    [Header("References")]
    [SerializeField] private Transform barrel;

    [SerializeField] private Transform muzzle;

    [SerializeField] private CannonBall ballPrefab;
    [SerializeField] private Camera cam;

    [Header("Aiming")]
    [SerializeField] private LayerMask aimLayerMask;

    [SerializeField] private Vector3 barrelAxisOffset = Vector3.zero;

    [SerializeField] private float minElevation = -10f;

    [SerializeField] private float maxElevation = 60f;

    [SerializeField] private float maxYaw = 45f;

    [SerializeField] private float rotationSpeed = 20f;

    [Header("Firing")]
    [SerializeField] private float launchSpeed = 30f;

    [SerializeField] private ArcPreference arc = ArcPreference.Low;

    [SerializeField] private float fireCooldown = 0.35f;
    [SerializeField] private float maxAimDistance = 500f;

    private CannonBrain brain;

    public void Initialize(
        IPointerInputSource input,
        ITimeProvider time,
        IAmmoSource ammo,
        BallRegistry ballRegistry)
    {
        if (cam == null) cam = Camera.main;

        brain = new CannonBrain(
            input,
            new CameraAimRaycaster(cam, aimLayerMask, maxAimDistance),
            new TransformBarrelView(barrel, barrelAxisOffset, rotationSpeed),
            new CannonBallLauncher(muzzle, ballPrefab, ballRegistry),
            new FireGate(new FireControl(time, fireCooldown), ammo),
            new BarrelAim(minElevation, maxElevation, maxYaw),
            new BallisticAim(new BallisticSolver(-Physics.gravity.y), launchSpeed, arc));
    }

    public void Tick(float deltaTime)
    {
        brain?.Tick(deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || brain == null || !brain.HasAimed || muzzle == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(brain.AimPoint, 0.3f);
        Gizmos.DrawLine(muzzle.position, brain.AimPoint);
    }
}
