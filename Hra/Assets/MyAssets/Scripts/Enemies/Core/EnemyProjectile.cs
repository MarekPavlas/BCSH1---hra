using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    public float lifetime = 4f;

    Rigidbody rb;
    Collider col;

    float damage;
    Transform owner;

    bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        col.enabled = false;
        col.isTrigger = true;

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Init(Vector3 dir, float speed, float dmg, Transform owner)
    {
        this.damage = dmg;
        this.owner = owner;

        if (owner != null)
        {
            var ownerCols = owner.GetComponentsInChildren<Collider>(true);
            foreach (var c in ownerCols)
                if (c != null) Physics.IgnoreCollision(col, c, true);
        }

        col.enabled = true;

        rb.linearVelocity = dir.normalized * speed;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (other == null) return;

        if (owner != null && other.transform != null && other.transform.root == owner) return;

        if (other.CompareTag("Player") || (other.transform != null && other.transform.root.CompareTag("Player")))
        {
            var stats = other.GetComponentInParent<PlayerStats>();
            if (stats != null)
                stats.TakeDamage(damage);

            hasHit = true;
            Destroy(gameObject);
            return;
        }

        hasHit = true;
        Destroy(gameObject);
    }
}