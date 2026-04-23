using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OrbitingBladeHit : MonoBehaviour
{
    public OrbitingBladesWeapon weapon;
    public float damage = 10f;
    public float hitCooldown = 0.25f;
    public bool debugLogs = false;

    readonly Dictionary<int, float> lastHit = new();

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other == null)
            return;

        Transform root = other.transform.root;
        bool isEnemy = other.CompareTag("Enemy") || (root != null && root.CompareTag("Enemy"));
        if (!isEnemy)
            return;

        EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();
        if (eh == null)
            return;

        int id = eh.GetInstanceID();
        float t = Time.time;

        if (lastHit.TryGetValue(id, out float last) && (t - last) < hitCooldown)
            return;

        lastHit[id] = t;

        float finalDamage = weapon != null ? weapon.GetCurrentDamage() : damage;
        eh.TakeDamage(finalDamage);

        if (debugLogs)
            Debug.Log($"[Blade] Hit {eh.name} dmg={finalDamage:0.0}");
    }
}
