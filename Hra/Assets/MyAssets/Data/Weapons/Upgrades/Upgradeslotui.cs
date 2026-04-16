using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    [Header("Level Colors")]
    public Color colorLv1 = new Color(0.75f, 0.75f, 0.75f);
    public Color colorLv2 = new Color(0.30f, 0.85f, 0.30f);
    public Color colorLv3 = new Color(0.30f, 0.60f, 1.00f);
    public Color colorLvMax = new Color(1.00f, 0.80f, 0.10f);

    private WeaponUpgradeConfig _config;
    private int _currentLevel;

    // ✅ OPRAVA: bere WeaponUpgradeConfig
    public void Bind(WeaponUpgradeConfig config, int currentLevel)
    {
        _config = config;
        _currentLevel = currentLevel;
        Refresh();
    }

    public void Refresh()
    {
        if (_config == null) return;

        if (nameText)
            nameText.text = string.IsNullOrEmpty(_config.displayName)
                ? _config.id.ToString()
                : _config.displayName;

        if (levelText)
        {
            levelText.text = $"Lv {_currentLevel} / {_config.maxLevel}";
            levelText.color = GetLevelColor(_currentLevel, _config.maxLevel);
        }

        // 👉 Icon nemáš v configu → vypneme nebo necháme null
        if (iconImage)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    Color GetLevelColor(int level, int max)
    {
        if (level >= max) return colorLvMax;

        float t = (float)(level - 1) / Mathf.Max(1, max - 1);

        if (t < 0.5f)
            return Color.Lerp(colorLv1, colorLv2, t * 2f);

        return Color.Lerp(colorLv2, colorLv3, (t - 0.5f) * 2f);
    }
}