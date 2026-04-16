using UnityEngine;

public class SimpleCinemachineOrbit : MonoBehaviour
{
    public Transform target;                 // Player
    public float sensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 70f;

    float yaw;
    float pitch = 15f;

    void Start()
    {
        if (target == null)
            Debug.LogWarning("SimpleCinemachineOrbit: target not set!");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        Debug.Log("Orbit running, mouseX=" + Input.GetAxis("Mouse X"));
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // drž kameru rig na hráči a otáčej ji
        transform.position = target.position;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}