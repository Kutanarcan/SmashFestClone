using Game.Core.Breakables;
using Game.Core.Impacts;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 10f;

    [SerializeField] private float damagePerSpeedUnit = 1f;

    [Header("Damage Sources")]
    [SerializeField] private LayerMask ballLayers;

    [SerializeField] private bool damagedByBall = true;
    [SerializeField] private float ballMinSpeed = 2f;

    [SerializeField] private LayerMask groundLayers;

    [SerializeField] private bool damagedByGround;
    [SerializeField] private float groundMinSpeed = 5f;

    [SerializeField] private LayerMask debrisLayers;

    [SerializeField] private bool damagedByDebris;
    [SerializeField] private float debrisMinSpeed = 5f;

    private BreakableBody body;

    public event ImpactHandler Broke;

    public float Health => body?.Health ?? maxHealth;

    public bool IsBroken => body != null && body.IsBroken;

    private void Awake()
    {
        body = new BreakableBody(new BreakableSettings(
            maxHealth,
            damagePerSpeedUnit,
            MinSpeedOrImmune(damagedByBall, ballMinSpeed),
            MinSpeedOrImmune(damagedByGround, groundMinSpeed),
            MinSpeedOrImmune(damagedByDebris, debrisMinSpeed)));

        body.Broke += OnBodyBroke;
    }

    private void OnDestroy()
    {
        if (body != null) body.Broke -= OnBodyBroke;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount == 0) return;

        ImpactSource source = ClassifySource(collision.gameObject.layer);
        if (source == ImpactSource.Unknown) return;

        ContactPoint contact = collision.GetContact(0);

        var impact = new ImpactEvent(
            source,
            collision.relativeVelocity.magnitude,
            contact.point,
            contact.normal);

        body.TakeImpact(impact);
    }

    private ImpactSource ClassifySource(int layer)
    {
        int bit = 1 << layer;

        if ((ballLayers.value & bit) != 0) return ImpactSource.Ball;
        if ((groundLayers.value & bit) != 0) return ImpactSource.Ground;
        if ((debrisLayers.value & bit) != 0) return ImpactSource.Debris;

        return ImpactSource.Unknown;
    }

    private void OnBodyBroke(in ImpactEvent impact) => Broke?.Invoke(impact);

    private static float MinSpeedOrImmune(bool enabled, float minSpeed)
        => enabled ? minSpeed : BreakableSettings.Immune;
}
