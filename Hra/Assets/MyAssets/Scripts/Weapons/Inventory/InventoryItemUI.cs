using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text levelText;

    public void Bind(WeaponId id, string weaponName, int currentLevel, int maxLevel, Sprite icon)
    {
        if (iconImage != null)
            iconImage.sprite = icon;

        if (nameText != null)
            nameText.text = weaponName;

        if (levelText != null)
            levelText.text = $"Lv {currentLevel}/{maxLevel}";
    }
}
