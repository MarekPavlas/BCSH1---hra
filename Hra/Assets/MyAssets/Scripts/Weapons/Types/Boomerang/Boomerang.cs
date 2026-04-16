using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BoomerangProjectile : MonoBehaviour
{
    Rigidbody rb;
    Collider triggerCol;

    BoomerangWeapon weapon;
    Transform owner;
    Transform catchPoint;

    float damage;
    float outgoingSpeed;
    float returnSpeed;
    float outgoingTime;
    float catchDistance;
    float maxLifeTime;

    Vector3 dir;
    float stateTimer;
    bool returning;

    [Header("Obstacle return (works even with Trigger collider)")]
    public bool returnOnObstacle = true;

    public LayerMask obstacleMask = ~0;

    public float probeRadius = 0.25f;
    public float probeExtraDistance = 0.15f;
    public float ignoreObstacleCheckFor = 0.05f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        triggerCol = GetComponent<Collider>();

        triggerCol.isTrigger = true;

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    public void Init(
        BoomerangWeapon weapon,
        Transform owner,
        Transform catchPoint,
        Vector3 direction,
        float damage,
        float outgoingSpeed,
        float returnSpeed,
        float outgoingTime,
        float catchDistance,
        float maxLifeTime
    )
    {
        this.weapon = weapon;
        this.owner = owner;
        this.catchPoint = catchPoint != null ? catchPoint : owner;

        dir = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;

        this.damage = damage;
        this.outgoingSpeed = outgoingSpeed;
        this.returnSpeed = returnSpeed;
        this.outgoingTime = outgoingTime;
        this.catchDistance = catchDistance;
        this.maxLifeTime = maxLifeTime;

        rb.isKinematic = false;
        rb.useGravity = false;

        returning = false;
        stateTimer = 0f;

        rb.linearVelocity = dir * outgoingSpeed;

        Destroy(gameObject, maxLifeTime);
    }

    void FixedUpdate()
    {
        if (owner == null)
        {
            KillAndNotify();
            return;
        }

        if (rb.isKinematic) rb.isKinematic = false;

        stateTimer += Time.fixedDeltaTime;

        if (!returning && stateTimer >= outgoingTime)
            returning = true;

        if (!returning)
        {
            rb.linearVelocity = dir * outgoingSpeed;

            if (returnOnObstacle && stateTimer >= ignoreObstacleCheckFor)
            {
                float stepDist = outgoingSpeed * Time.fixedDeltaTime + probeExtraDistance;

                if (Physics.SphereCast(
                        transform.position,
                        probeRadius,
                        dir,
                        out RaycastHit hit,
                        stepDist,
                        obstacleMask,
                        QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform != null && owner != null && hit.transform.root == owner)
                        return;

                    if (hit.transform != null && (hit.transform.CompareTag("Enemy") || hit.transform.root.CompareTag("Enemy")))
                        return;

                    returning = true;
                    stateTimer = outgoingTime;
                }
            }

            return;
        }

        Vector3 target = catchPoint != null ? catchPoint.position : owner.position;
        Vector3 to = target - transform.position;
        float dist = to.magnitude;

        if (dist <= catchDistance)
        {
            KillAndNotify();
            return;
        }

        Vector3 v = (dist > 0.0001f) ? (to / dist) * returnSpeed : Vector3.zero;
        rb.linearVelocity = v;

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity.normalized, Vector3.up);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (owner != null && other.transform.root == owner) return;

        if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
        {
            var eh = other.GetComponentInParent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(damage);
        }
    }

    void OnDestroy()
    {
        if (weapon != null)
            weapon.NotifyReturned(this);
    }

    void KillAndNotify()
    {
        Destroy(gameObject);
    }
}