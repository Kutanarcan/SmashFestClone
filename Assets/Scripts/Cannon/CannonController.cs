using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mobil odakli nisan alma ve ates etme.
/// Hover yoktur: dokunma aninda raycast atilir, namlu hedefe cevrilir ve top firlatilir.
/// Namlu sadece gorseldir; atis yonu her zaman dogrudan nisan noktasina hesaplanir,
/// bu sayede govdenin veya mesh'in hangi eksende durdugu onemsizlesir.
/// </summary>
public class CannonController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Donen namlu. Sadece gorsel amaclidir, atis yonunu belirlemez.")]
    [SerializeField] private Transform barrel;

    [Tooltip("Topun spawn oldugu nokta. Namlu ucunun biraz DISINDA olmali.")]
    [SerializeField] private Transform muzzle;

    [SerializeField] private CannonBall ballPrefab;
    [SerializeField] private Camera cam;

    [Header("Aiming")]
    [Tooltip("Breakable + Static + AimPlane secili olmali. Ball ve Cannon SECILI OLMAMALI.")]
    [SerializeField] private LayerMask aimLayerMask;

    [Tooltip("Namlu mesh'i +Z bakiyorsa (0,0,0). +Y bakiyorsa (90,0,0). +X bakiyorsa (0,-90,0).")]
    [SerializeField] private Vector3 barrelAxisOffset = Vector3.zero;

    [Tooltip("Namlunun asagi bakabilecegi maksimum aci (negatif = asagi).")]
    [SerializeField] private float minElevation = -10f;

    [Tooltip("Namlunun yukari bakabilecegi maksimum aci.")]
    [SerializeField] private float maxElevation = 60f;

    [Tooltip("Namlunun saga/sola donebilecegi maksimum aci (merkeze gore).")]
    [SerializeField] private float maxYaw = 45f;

    [Tooltip("Namlunun hedefe donme hizi. Gorsel bir gecikmedir, atisi etkilemez.")]
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Firing")]
    [Tooltip("Sabit cikis hizi (m/s). Topun kutlesinden bagimsizdir.")]
    [SerializeField] private float launchSpeed = 30f;

    [SerializeField] private float fireCooldown = 0.35f;
    [SerializeField] private float maxAimDistance = 500f;

    private float lastFireTime = -999f;
    private Vector3 aimPoint;
    private bool hasAimed;

    public bool CanFire => Time.time >= lastFireTime + fireCooldown;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        aimPoint = muzzle != null ? muzzle.position + transform.forward * 10f : Vector3.zero;
    }

    private void Update()
    {
        if (WasPressedThisFrame() && !IsPointerOverUI())
            HandleTap(GetPointerPosition());

        UpdateBarrelRotation();
    }

    // ---------------------------------------------------------------- input

    private bool WasPressedThisFrame()
    {
        // Pointer.current hem fareyi hem dokunmayi kapsar.
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }

    private Vector2 GetPointerPosition()
    {
        return Pointer.current.position.ReadValue();
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    // ------------------------------------------------------------------ tap

    private void HandleTap(Vector2 screenPos)
    {
        if (!ResolveAimPoint(screenPos)) return;
        if (!CanFire) return;

        Fire();
    }

    /// <summary>
    /// Tek raycast, iki davranis: objeye dokunulduysa objenin yuzeyi,
    /// bosluga dokunulduysa arkadaki AimPlane nisan noktasi olur.
    /// </summary>
    private bool ResolveAimPoint(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimLayerMask))
            return false;

        aimPoint = hit.point;
        hasAimed = true;
        return true;
    }

    // ----------------------------------------------------------------- fire

    private void Fire()
    {
        lastFireTime = Time.time;

        Vector3 dir = (aimPoint - muzzle.position).normalized;

        CannonBall ball = Instantiate(ballPrefab, muzzle.position, Quaternion.LookRotation(dir));
        ball.Launch(dir * launchSpeed);
    }

    // --------------------------------------------------------------- barrel

    /// <summary>
    /// Namluyu son nisan noktasina cevirir.
    ///
    /// Aci hesabi eulerAngles yerine dogrudan yon vektorunden yapilir:
    /// eulerAngles ayni rotasyon icin birden fazla temsil dondurebildigi
    /// icin clamp'i bozar (gimbal sicramasi). Atan2/Asin ile bu sorun yok.
    ///
    /// Yon parent'in YEREL uzayina cevrilir, boylece govdenin dunyada
    /// yatik durmasi acilari etkilemez.
    /// </summary>
    private void UpdateBarrelRotation()
    {
        if (!hasAimed || barrel == null) return;

        Vector3 toTarget = aimPoint - barrel.position;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        // Dunya yonu -> parent'a gore yerel yon
        Vector3 dir = barrel.parent != null
            ? barrel.parent.InverseTransformDirection(toTarget)
            : toTarget;
        dir.Normalize();

        // Yatay sapma: +Z ekseni referans, saga donus pozitif
        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        // Dikey aci: yukari bakmak pozitif
        float elevation = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        elevation = Mathf.Clamp(elevation, minElevation, maxElevation);

        // Unity'de +X euler asagi baktigi icin elevation ters isaretle girilir.
        Quaternion target = Quaternion.Euler(-elevation, yaw, 0f)
                          * Quaternion.Euler(barrelAxisOffset);

        barrel.localRotation = rotationSpeed <= 0f
            ? target
            : Quaternion.Slerp(barrel.localRotation, target, rotationSpeed * Time.deltaTime);
    }

    // ---------------------------------------------------------------- debug

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !hasAimed || muzzle == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimPoint, 0.3f);
        Gizmos.DrawLine(muzzle.position, aimPoint);
    }
}