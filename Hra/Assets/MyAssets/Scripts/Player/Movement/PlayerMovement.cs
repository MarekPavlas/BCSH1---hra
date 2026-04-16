using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    [Header("Stats (optional)")]
    public PlayerStats stats; 

    [Header("Movement (fallback if stats == null)")]
    public float speed = 12f;
    public float turnSpeed = 12f;
    public Transform cameraTransform;

    [Header("Jump & Gravity")]
    public float gravity = -19.62f;
    public float jumpHeight = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (stats == null) stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        else
            isGrounded = controller != null && controller.isGrounded;

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            move = camRight * x + camForward * z;
        }
        else
        {
            move = transform.right * x + transform.forward * z;
        }

        float currentSpeed = (stats != null) ? stats.Get(PlayerStatType.MoveSpeed) : speed;

        if (controller != null)
            controller.Move(move * currentSpeed * Time.deltaTime);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        if (controller != null)
            controller.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}