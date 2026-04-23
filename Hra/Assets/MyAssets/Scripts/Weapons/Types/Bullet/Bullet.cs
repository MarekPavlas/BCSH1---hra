using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 25f;
    public float lifetime = 3f;

    private Rigidbody rb;
    private Collider col;

    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        col.isTrigger = true;
    }

    void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        rb.linearVelocity = dir.normalized * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (other == null) return;

        if (other.CompareTag("Player") || (other.transform != null && other.transform.root.CompareTag("Player")))
            return;

        if (other.CompareTag("Enemy") || (other.transform != null && other.transform.root.CompareTag("Enemy")))
        {
            hasHit = true;
            col.enabled = false;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;

            var enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}