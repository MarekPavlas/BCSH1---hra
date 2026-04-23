using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ShotgunPellet : MonoBehaviour
{
    [Header("Runtime (set by weapon)")]
    public float speed = 40f;
    public float damage = 5f;
    public float lifetime = 2f;

    [Header("Hit")]
    public string enemyTag = "Enemy";

    Rigidbody rb;
    Collider col;
    Transform ownerRoot;

    bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        col.isTrigger = true;
    }

    public void Launch(Vector3 dir, float speed, float damage, float lifetime, Transform ownerRoot)
    {
        this.speed = speed;
        this.damage = damage;
        this.lifetime = lifetime;
        this.ownerRoot = ownerRoot;

        hasHit = false;

        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;

        rb.linearVelocity = dir.normalized * speed;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (other == null) return;

        if (ownerRoot != null && other.transform.root == ownerRoot)
            return;

        if (!(other.CompareTag(enemyTag) || other.transform.root.CompareTag(enemyTag)))
            return;

        var eh = other.GetComponentInParent<EnemyHealth>();
        if (eh == null) return;

        hasHit = true;

        if (col != null) col.enabled = false;

        eh.TakeDamage(damage);
        Destroy(gameObject);
    }
}   