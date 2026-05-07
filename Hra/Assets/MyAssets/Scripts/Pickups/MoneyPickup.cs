using UnityEngine;

public class MoneyPickup : MonoBehaviour
{
    [Header("Value")]
    public int amount = 1;

    [Header("Collect")]
    public float collectDistance = 1.0f;
    public Vector3 targetOffset = new Vector3(0f, 1f, 0f);

    Transform target;
    float targetMaxSpeed;
    float targetAccel;
    float currentSpeed;
    bool lockY;

    CurrencyWallet wallet;
    bool collected;

    void Awake()
    {
        wallet = FindFirstObjectByType<CurrencyWallet>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void AttractTo(Transform t, float maxSpeed, float accel, bool lockYToPlayer)
    {
        if (collected)
            return;

        if (target == null)
            currentSpeed = 0f;

        target = t;
        targetMaxSpeed = Mathf.Max(0.1f, maxSpeed);
        targetAccel = Mathf.Max(0.1f, accel);
        lockY = lockYToPlayer;
    }

    void Update()
    {
        if (collected || target == null)
            return;

        Vector3 targetPos = target.position + targetOffset;

        if (lockY)
            targetPos.y = transform.position.y;

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= collectDistance)
        {
            Collect();
            return;
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetMaxSpeed,
            targetAccel * Time.deltaTime
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            currentSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) <= collectDistance)
        {
            Collect();
        }
    }

    void Collect()
    {
        if (collected)
            return;

        collected = true;

        if (wallet == null)
            wallet = FindFirstObjectByType<CurrencyWallet>();

        if (wallet != null)
            wallet.AddMoney(amount);

        if (SaveStatsManager.Instance != null)
            SaveStatsManager.Instance.AddMoneyEarned(amount);

        Destroy(gameObject);
    }
}