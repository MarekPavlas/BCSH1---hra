using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    [Header("Level Colors")]
    public Color colorLvl1 = new Color(0.8f, 0.8f, 0.8f);
    public Color colorLvl2 = new Color(0.3f, 0.8f, 0.3f);
    public Color colorLvl3 = new Color(0.3f, 0.6f, 1.0f);
    public Color colorLvlMax = new Color(1.0f, 0.8f, 0.1f);

    public void Bind(WeaponId weaponId, string weaponName, int level, int maxLevel, Sprite icon = null)
    {
        if (nameText) nameText.text = weaponName;
        if (levelText)
        {
            levelText.text = $"Lv {level}/{maxLevel}";
            levelText.color = GetLevelColor(level, maxLevel);
        }

        if (iconImage)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
    }

    Color GetLevelColor(int level, int maxLevel)
    {
        if (level >= maxLevel) return colorLvlMax;
        float t = (float)(level - 1) / Mathf.Max(1, maxLevel - 1);
        if (t < 0.5f) return Color.Lerp(colorLvl1, colorLvl2, t * 2f);
        return Color.Lerp(colorLvl2, colorLvl3, (t - 0.5f) * 2f);
    }
}