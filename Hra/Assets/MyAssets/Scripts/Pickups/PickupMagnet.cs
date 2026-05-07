using UnityEngine;

public class PickupMagnet : MonoBehaviour
{
    [Header("Magnet")]
    public float radius = 10f;
    public float pullSpeed = 35f;
    public float pullAccel = 120f;
    public bool lockYToPlayer = true;

    [Header("Filter")]
    public LayerMask pickupMask;

    void Update()
    {
        var hits = Physics.OverlapSphere(transform.position, radius, pickupMask, QueryTriggerInteraction.Collide);

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;

            var money = c.GetComponentInParent<MoneyPickup>();
            if (money == null) continue;

            money.AttractTo(transform, pullSpeed, pullAccel, lockYToPlayer);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}