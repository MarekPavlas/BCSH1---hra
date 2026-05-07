using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealthPickup : MonoBehaviour
{
    [Header("Heal")]
    public float healAmount = 10f;

    [Header("Pickup")]
    public bool destroyIfFullHp = false;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        Transform root = other.transform.root;

        if (!other.CompareTag("Player") && (root == null || !root.CompareTag("Player")))
            return;

        PlayerStats stats = other.GetComponentInParent<PlayerStats>();
        if (stats == null && root != null)
            stats = root.GetComponent<PlayerStats>();

        if (stats == null)
            return;

        if (stats.currentHP >= stats.maxHp.Value)
        {
            if (!destroyIfFullHp)
                return;
        }

        stats.Heal(healAmount);
        Destroy(gameObject);
    }
}
