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
    private Vector3 velocityBeforeCollision;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
        velocityBeforeCollision = velocity;

        Destroy(gameObject, maxLifetime);
    }

    private void FixedUpdate()
    {
        velocityBeforeCollision = rb.linearVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool isBreakable = (breakableLayers.value & (1 << collision.gameObject.layer)) != 0;
        if (!isBreakable) return;

        rb.linearVelocity = velocityBeforeCollision * breakableSpeedRetention;
    }
}