using UnityEngine;

public class PlayerAnimatorDriver : MonoBehaviour
{
    public Animator animator;
    public CharacterController characterController;

    [Header("Animator Params")]
    public string speedParam = "Speed";

    [Header("Tuning")]
    public float speedLerp = 12f;
    public float maxMoveSpeedForAnim = 12f;
    public float deadZone = 0.1f;
    public bool useWorldVelocity = false;
    public bool debugLogs = false;

    float currentAnimSpeed;
    Vector3 lastPosition;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (animator == null)
            return;

        float moveSpeed = GetCurrentMoveSpeed();
        float normalizedSpeed = Mathf.Clamp01(moveSpeed / Mathf.Max(0.01f, maxMoveSpeedForAnim));

        currentAnimSpeed = Mathf.Lerp(
            currentAnimSpeed,
            normalizedSpeed,
            Time.deltaTime * speedLerp
        );

        if (currentAnimSpeed < deadZone)
            currentAnimSpeed = 0f;

        animator.SetFloat(speedParam, currentAnimSpeed);

        if (debugLogs)
            Debug.Log($"[PlayerAnimatorDriver] moveSpeed={moveSpeed:0.00} animSpeed={currentAnimSpeed:0.00}");

        lastPosition = transform.position;
    }

    float GetCurrentMoveSpeed()
    {
        if (!useWorldVelocity && characterController != null)
        {
            Vector3 vel = characterController.velocity;
            vel.y = 0f;
            return vel.magnitude;
        }

        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;

        return delta.magnitude / Mathf.Max(0.0001f, Time.deltaTime);
    }
}