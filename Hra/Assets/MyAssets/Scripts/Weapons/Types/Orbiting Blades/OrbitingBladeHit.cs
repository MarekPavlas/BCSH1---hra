using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OrbitingBladeHit : MonoBehaviour
{
    public float damage = 10f;
    public float hitCooldown = 0.25f;
    public bool debugLogs = false;

    Dictionary<int, float> lastHit = new Dictionary<int, float>();

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        int id = other.GetInstanceID();
        float t = Time.time;

        if (lastHit.TryGetValue(id, out float last) && (t - last) < hitCooldown)
            return;

        lastHit[id] = t;

        var eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null)
        {
            eh.TakeDamage(damage);
            if (debugLogs) Debug.Log($"[Blade] Hit {eh.name} dmg={damage}");
        }
    }
}