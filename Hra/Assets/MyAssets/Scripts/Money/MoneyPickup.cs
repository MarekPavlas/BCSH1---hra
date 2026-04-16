using UnityEngine;

public class MoneyPickup : MonoBehaviour
{
    [Header("Value")]
    public int amount = 1;

    [Header("Collect")]
    public float collectDistance = 0.35f;

    Transform target;
    float targetMaxSpeed;
    float targetAccel;
    float currentSpeed;
    bool lockY;

    CurrencyWallet wallet;

    void Awake()
    {
        wallet = FindFirstObjectByType<CurrencyWallet>();
    }

    public void AttractTo(Transform t, float maxSpeed, float accel, bool lockYToPlayer)
    {
        if (target == null) currentSpeed = 0f;

        target = t;
        targetMaxSpeed = maxSpeed;
        targetAccel = accel;
        lockY = lockYToPlayer;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;
        if (lockY) targetPos.y = transform.position.y;

        Vector3 to = targetPos - transform.position;
        float dist = to.magnitude;

        if (dist <= collectDistance)
        {
            Collect();
            return;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetMaxSpeed, targetAccel * Time.deltaTime);

        transform.position += (to / Mathf.Max(dist, 0.0001f)) * currentSpeed * Time.deltaTime;
    }

    void Collect()
    {
        if (wallet != null) wallet.AddMoney(amount);
        Destroy(gameObject);
    }
}