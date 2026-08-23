using Game.Core;
using Game.Core.Aiming;
using Game.Core.Firing;
using Game.Core.Inputs;
using Game.Core.Timing;
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

    [SerializeField] private float fireCooldown = 0.35f;
    [SerializeField] private float maxAimDistance = 500f;

    private IPointerInputSource input;
    private FireControl fireControl;

    private Vector3 aimPoint;
    private bool hasAimed;

    public bool CanFire => fireControl != null && fireControl.CanFire;

    public void Initialize(IPointerInputSource input, ITimeProvider time)
    {
        this.input = input;
        fireControl = new FireControl(time, fireCooldown);
    }

    private void Awake()
    {
        if (cam == null) cam = Camera.main;

        aimPoint = muzzle != null ? muzzle.position + transform.forward * 10f : Vector3.zero;
    }

    public void Tick(float deltaTime)
    {
        if (input == null) return;

        if (input.WasPressedThisFrame && !input.IsOverUI)
            HandleTap(input.Position);

        UpdateBarrelRotation(deltaTime);
    }

    private void HandleTap(Vector2 screenPos)
    {
        if (!ResolveAimPoint(screenPos))
            return;

        if (!fireControl.TryFire())
            return;

        Fire();
    }

    private bool ResolveAimPoint(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimLayerMask))
            return false;

        aimPoint = hit.point;
        hasAimed = true;
        return true;
    }

    private void Fire()
    {
        Vector3 dir = (aimPoint - muzzle.position).normalized;

        CannonBall ball = Instantiate(ballPrefab, muzzle.position, Quaternion.LookRotation(dir));
        ball.Launch(dir * launchSpeed);
    }

    private void UpdateBarrelRotation(float deltaTime)
    {
        if (!hasAimed || barrel == null) return;

        Vector3 toTarget = aimPoint - barrel.position;

        Vector3 localDir = barrel.parent != null
            ? barrel.parent.InverseTransformDirection(toTarget)
            : toTarget;

        var aim = new BarrelAim(minElevation, maxElevation, maxYaw);
        if (!aim.TryResolve(localDir, out AimAngles angles)) return;

        Quaternion target = Quaternion.Euler(-angles.Elevation, angles.Yaw, 0f)
                          * Quaternion.Euler(barrelAxisOffset);

        barrel.localRotation = rotationSpeed <= 0f
            ? target
            : Quaternion.Slerp(barrel.localRotation, target, rotationSpeed * deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !hasAimed || muzzle == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimPoint, 0.3f);
        Gizmos.DrawLine(muzzle.position, aimPoint);
    }
}
