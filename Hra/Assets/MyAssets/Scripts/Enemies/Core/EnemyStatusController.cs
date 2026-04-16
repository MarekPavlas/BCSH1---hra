using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyStatusController : MonoBehaviour
{
    [Header("Debug")]
    public bool debugLogs = false;

    EnemyHealth health;
    NavMeshAgent agent;

    int bleedStacks;
    float bleedTickTimer;

    float burnTimeLeft;
    float burnDps;

    float slowTimeLeft;
    float slowMultiplier = 1f;

    float baseAgentSpeed = -1f;

    float stunTimeLeft;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
            baseAgentSpeed = agent.speed;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (stunTimeLeft > 0f)
        {
            stunTimeLeft -= dt;
            ApplyAgentSpeed(0f);
        }
        else
        {
            if (slowTimeLeft > 0f)
            {
                slowTimeLeft -= dt;

                float baseSpeed = GetBaseSpeedSafe();
                ApplyAgentSpeed(baseSpeed * slowMultiplier);
            }
            else
            {
                if (agent != null)
                {
                    float current = agent.speed;

                    if (baseAgentSpeed <= 0f)
                        baseAgentSpeed = current;

                    if (!Mathf.Approximately(current, baseAgentSpeed))
                        baseAgentSpeed = current;
                }
            }
        }

        if (bleedStacks > 0)
        {
            bleedTickTimer -= dt;
            if (bleedTickTimer <= 0f)
            {
                bleedTickTimer = 1f;
                float dmg = bleedStacks * 2f;
                health.TakeDamage(dmg);

                if (debugLogs)
                    Debug.Log($"[Status] BLEED tick {dmg} stacks={bleedStacks} on {name}");
            }
        }

        if (burnTimeLeft > 0f)
        {
            burnTimeLeft -= dt;
            float dmg = burnDps * dt;
            health.TakeDamage(dmg);
        }
    }

    float GetBaseSpeedSafe()
    {
        if (agent == null) return 0f;

        if (baseAgentSpeed <= 0f)
            baseAgentSpeed = agent.speed;

        return Mathf.Max(0f, baseAgentSpeed);
    }

    void ApplyAgentSpeed(float s)
    {
        if (agent == null) return;
        if (!agent.isOnNavMesh) return;

        agent.speed = Mathf.Max(0f, s);
    }

    public void AddBleedStack(int addStacks, float bleedDmgPerStackPerSec = 2f)
    {
        bleedStacks = Mathf.Clamp(bleedStacks + addStacks, 0, 999);
        bleedTickTimer = Mathf.Min(bleedTickTimer, 0.2f);

        if (debugLogs)
            Debug.Log($"[Status] BLEED +{addStacks} => {bleedStacks} on {name}");
    }

    public int GetBleedStacks() => bleedStacks;

    public void ClearBleed()
    {
        bleedStacks = 0;
        bleedTickTimer = 0f;
    }

    public void ApplyBurn(float duration, float dps)
    {
        burnTimeLeft = Mathf.Max(burnTimeLeft, duration);
        burnDps = Mathf.Max(burnDps, dps);

        if (debugLogs)
            Debug.Log($"[Status] BURN duration={burnTimeLeft:0.0}s dps={burnDps:0.0} on {name}");
    }

    public void ApplySlow(float duration, float multiplier)
    {
        if (duration <= 0f) return;

        slowTimeLeft = Mathf.Max(slowTimeLeft, duration);

        float m = Mathf.Clamp(multiplier, 0.05f, 1f);

        slowMultiplier = Mathf.Min(slowMultiplier, m);

        if (debugLogs)
            Debug.Log($"[Status] SLOW x{slowMultiplier:0.00} {slowTimeLeft:0.0}s on {name}");
    }

    public void ApplySlowMultiplier(float moveSpeedMultiplier, float duration)
    {
        ApplySlow(duration, moveSpeedMultiplier);
    }

    public void ApplyStun(float duration)
    {
        if (duration <= 0f) return;

        stunTimeLeft = Mathf.Max(stunTimeLeft, duration);

        if (debugLogs)
            Debug.Log($"[Status] STUN {stunTimeLeft:0.0}s on {name}");
    }

    public bool TryExecuteUnderPercent(float hpPercentThreshold)
    {
        if (health == null) return false;

        float max = Mathf.Max(0.01f, health.health);
        float pct = health.currentHealth / max;

        if (pct <= hpPercentThreshold)
        {
            health.TakeDamage(health.currentHealth + 99999f);

            if (debugLogs)
                Debug.Log($"[Status] EXECUTE on {name} (pct={pct:0.00})");

            return true;
        }

        return false;
    }
}