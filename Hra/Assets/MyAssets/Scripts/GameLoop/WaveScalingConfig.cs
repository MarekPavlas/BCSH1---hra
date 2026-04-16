using UnityEngine;

[CreateAssetMenu(menuName = "Game/Waves/Wave Scaling Config")]
public class WaveScalingConfig : ScriptableObject
{
    [Header("Base values (Wave 1)")]
    public float baseEnemyHp = 15f;
    public float baseEnemyDamage = 5f;
    public int baseSpawnCount = 8;
    public float baseSpawnInterval = 0.6f; 

    [Header("Multipliers by wave (X = wave index, Y = multiplier)")]
    public AnimationCurve hpMultByWave = DefaultCurve(1f, 3f);
    public AnimationCurve damageMultByWave = DefaultCurve(1f, 2f);
    public AnimationCurve spawnCountMultByWave = DefaultCurve(1f, 2.5f);
    public AnimationCurve spawnIntervalMultByWave = DefaultCurve(1f, 0.6f);

    [Header("Clamp (safety)")]
    public float minHp = 1f;
    public float minDamage = 0.1f;
    public int minSpawnCount = 1;
    public float minSpawnInterval = 0.05f;

    public float GetEnemyHp(int wave)
    {
        float hp = baseEnemyHp * Eval(hpMultByWave, wave, 1f);
        return Mathf.Max(minHp, hp);
    }

    public float GetEnemyDamage(int wave)
    {
        float dmg = baseEnemyDamage * Eval(damageMultByWave, wave, 1f);
        return Mathf.Max(minDamage, dmg);
    }

    public int GetSpawnCount(int wave)
    {
        float mult = Eval(spawnCountMultByWave, wave, 1f);
        int count = Mathf.RoundToInt(baseSpawnCount * mult);
        return Mathf.Max(minSpawnCount, count);
    }

    public float GetSpawnInterval(int wave)
    {
        float mult = Eval(spawnIntervalMultByWave, wave, 1f);
        float interval = baseSpawnInterval * mult;
        return Mathf.Max(minSpawnInterval, interval);
    }

    static float Eval(AnimationCurve c, float x, float fallback)
    {
        if (c == null || c.length == 0) return fallback;
        return c.Evaluate(x);
    }

    static AnimationCurve DefaultCurve(float atWave1, float atWave20)
    {
        return new AnimationCurve(
            new Keyframe(1f, atWave1),
            new Keyframe(10f, Mathf.Lerp(atWave1, atWave20, 0.5f)),
            new Keyframe(20f, atWave20)
        );
    }
}