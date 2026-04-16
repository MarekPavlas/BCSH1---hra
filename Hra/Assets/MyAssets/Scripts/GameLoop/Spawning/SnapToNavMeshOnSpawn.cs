using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SnapToNavMeshOnSpawn : MonoBehaviour
{
    public float searchRadius = 10f;

    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();

        if (NavMesh.SamplePosition(transform.position, out var hit, searchRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"[SnapToNavMeshOnSpawn] No NavMesh near {name} at {transform.position}");
        }
    }
}