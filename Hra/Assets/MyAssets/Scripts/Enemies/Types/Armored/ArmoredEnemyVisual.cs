using UnityEngine;

public class ArmoredEnemyVisual : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject armoredVisual;
    public GameObject unarmoredVisual;

    [Header("Armor settings")]
    [Range(0.1f, 1f)] public float armoredDamageMultiplier = 0.6f;
    public bool removeArmorAtHalfHp = true;

    [Header("Optional buffs after armor breaks")]
    public bool buffAfterBreak = false;
    public float speedMultiplierAfterBreak = 1.2f;

    EnemyHealth health;
    bool armorBroken;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    void Start()
    {
        SetArmoredState(true);
        armorBroken = false;
    }

    void Update()
    {
        if (health == null) return;
        if (!removeArmorAtHalfHp) return;
        if (armorBroken) return;

        if (health.currentHealth <= health.health * 0.5f)
        {
            armorBroken = true;
            SetArmoredState(false);

            if (buffAfterBreak)
            {
                var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.speed *= speedMultiplierAfterBreak;
            }
        }
    }

    void SetArmoredState(bool armored)
    {
        if (armoredVisual != null) armoredVisual.SetActive(armored);
        if (unarmoredVisual != null) unarmoredVisual.SetActive(!armored);

        if (health != null)
            health.damageMultiplier = armored ? armoredDamageMultiplier : 1f;
    }
}