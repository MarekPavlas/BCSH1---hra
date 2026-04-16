using UnityEngine;

public class CameraPivotOrbit : MonoBehaviour
{
    public Transform player;                 // Player root
    public float sensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 70f;

    float yaw;
    float pitch = 15f;

    void Start()
    {
        if (player == null) player = transform.parent;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;

        Debug.Log("[CameraPivotOrbit] STARTED");
    }

    void LateUpdate()
    {
        if (player == null) return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        yaw += mx * sensitivity;
        pitch -= my * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.position = player.position + Vector3.up * 1.6f;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}