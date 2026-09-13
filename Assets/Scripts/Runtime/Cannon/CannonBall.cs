using System;
using Game.Core.Impacts;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CannonBall : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float maxLifetime = 8f;

    [Header("Punch-Through")]
    [Range(0f, 1f)]
    [SerializeField] private float breakableSpeedRetention = 0.9f;

    [SerializeField] private LayerMask breakableLayers;

    private Rigidbody rb;
    private PunchThrough punchThrough;
    private Vector3 velocityBeforeCollision;
    private Action<CannonBall> release;
    private float despawnAt;
    private bool live;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        punchThrough = new PunchThrough(breakableSpeedRetention);
    }

    public void Launch(Vector3 velocity, Action<CannonBall> release)
    {
        this.release = release;

        rb.linearVelocity = velocity;
        rb.angularVelocity = Vector3.zero;

        velocityBeforeCollision = velocity;
        despawnAt = Time.time + maxLifetime;
        live = true;
    }

    private void FixedUpdate()
    {
        if (!live) return;

        velocityBeforeCollision = rb.linearVelocity;

        if (Time.time >= despawnAt) Despawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!live) return;

        bool hitBreakable = (breakableLayers.value & (1 << collision.gameObject.layer)) != 0;

        if (punchThrough.TryResolve(velocityBeforeCollision, hitBreakable, out Vector3 velocity))
            rb.linearVelocity = velocity;
    }

    private void Despawn()
    {
        live = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (release != null) release(this);
        else Destroy(gameObject);
    }
}
