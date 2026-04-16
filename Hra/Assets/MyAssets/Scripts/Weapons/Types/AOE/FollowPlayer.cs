using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float yOffset = 0.02f;

    void LateUpdate()
    {
        if (!player) return;

        transform.position = new Vector3(player.position.x, yOffset, player.position.z);
        transform.rotation = Quaternion.identity; 
    }
}