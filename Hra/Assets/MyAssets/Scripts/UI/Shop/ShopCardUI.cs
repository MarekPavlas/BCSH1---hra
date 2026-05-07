using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardUI : MonoBehaviour
{
    [Header("Refs")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;
    public TMP_Text rarityText;
    public Button buyButton;

    Action onBuy;

    public void Bind(
        Sprite icon,
        string title,
        string description,
        int price,
        bool canAfford,
        bool canBuyMore,
        ItemRarity rarity,
        Action onBuyCallback)
    {
        if (iconImage != null)
            iconImage.sprite = icon;

        if (nameText != null)
            nameText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        if (priceText != null)
            priceText.text = price.ToString();

        if (rarityText != null)
            rarityText.text = rarity.ToString();

        ApplyRarityTextStyle(rarity);

        onBuy = onBuyCallback;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = canAfford && canBuyMore;
            buyButton.onClick.AddListener(() => onBuy?.Invoke());
        }
    }

    void ApplyRarityTextStyle(ItemRarity rarity)
    {
        Color rarityColor = Color.white;
        Color nameColor = Color.white;

        switch (rarity)
        {
            case ItemRarity.COMMON:
                rarityColor = new Color(0.75f, 0.75f, 0.75f);
                break;

            case ItemRarity.UNCOMMON:
                rarityColor = new Color(0.35f, 0.85f, 0.35f);
                break;

            case ItemRarity.RARE:
                rarityColor = new Color(0.35f, 0.55f, 1f);
                break;

            case ItemRarity.EPIC:
                rarityColor = new Color(0.8f, 0.35f, 1f);
                break;

            case ItemRarity.LEGENDARY:
                rarityColor = new Color(1f, 0.72f, 0.15f);
                nameColor = new Color(1f, 0.9f, 0.55f);
                break;
        }

        if (nameText != null)
            nameText.color = nameColor;

        if (rarityText != null)
            rarityText.color = rarityColor;
    }
}