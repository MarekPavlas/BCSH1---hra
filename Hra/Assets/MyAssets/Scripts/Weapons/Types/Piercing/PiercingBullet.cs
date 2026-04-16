using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PiercingBullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 20f;
    public float damage = 10f;
    public int pierceCount = 3;        
    public float lifetime = 3f;

    [Header("Debug")]
    public bool debugLogs = false;

    Rigidbody rb;
    Collider col;

    int pierceLeft;
    readonly HashSet<int> hitEnemyIds = new HashSet<int>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        col.isTrigger = true;

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnEnable()
    {
        hitEnemyIds.Clear();
        pierceLeft = Mathf.Max(1, pierceCount);

        CancelInvoke();
        Invoke(nameof(SelfDestruct), lifetime);

        if (debugLogs)
            Debug.Log($"[PiercingBullet] Enabled speed={speed} dmg={damage} pierceLeft={pierceLeft}");
    }

    void FixedUpdate()
    {
        Vector3 nextPos = rb.position + transform.forward * (speed * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();
        if (eh == null) return;

        int id = eh.GetInstanceID();
        if (hitEnemyIds.Contains(id)) return; 

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