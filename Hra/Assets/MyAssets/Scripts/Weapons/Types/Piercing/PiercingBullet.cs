using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PiercingBullet : MonoBehaviour
{
    [Header("Runtime")]
    public float speed = 20f;
    public float damage = 10f;
    public int pierceCount = 3;
    public float lifetime = 3f;
    public string enemyTag = "Enemy";

    [Header("Debug")]
    public bool debugLogs = false;

    Rigidbody rb;
    Collider col;
    Transform ownerRoot;

    int pierceLeft;
    readonly HashSet<int> hitEnemyIds = new();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        col.isTrigger = true;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Init(Vector3 dir, float speed, float damage, int pierceCount, float lifetime, Transform ownerRoot)
    {
        this.speed = speed;
        this.damage = damage;
        this.pierceCount = Mathf.Max(1, pierceCount);
        this.lifetime = lifetime;
        this.ownerRoot = ownerRoot;

        pierceLeft = this.pierceCount;
        hitEnemyIds.Clear();

        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;

        rb.linearVelocity = dir.normalized * this.speed;

        CancelInvoke();
        Invoke(nameof(SelfDestruct), this.lifetime);

        if (debugLogs)
            Debug.Log($"[PiercingBullet] Init speed={this.speed} dmg={this.damage} pierce={pierceLeft}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (ownerRoot != null && other.transform.root == ownerRoot)
            return;

        Transform root = other.transform.root;
        bool isEnemy = other.CompareTag(enemyTag) || (root != null && root.CompareTag(enemyTag));
        if (!isEnemy)
            return;

        EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();
        if (eh == null)
            return;

        int id = eh.GetInstanceID();
        if (hitEnemyIds.Contains(id))
            return;

        hitEnemyIds.Add(id);
        eh.TakeDamage(damage);
        pierceLeft--;

        if (debugLogs)
            Debug.Log($"[PiercingBullet] Hit {eh.name} dmg={damage} pierceLeft={pierceLeft}");

        if (pierceLeft <= 0)
            Destroy(gameObject);
    }

    void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
