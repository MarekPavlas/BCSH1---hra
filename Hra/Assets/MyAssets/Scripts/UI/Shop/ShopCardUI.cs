using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardUI : MonoBehaviour
{
    [Header("Refs")]
    public Image iconImage;
    public Image backgroundImage;
    public Image frameImage;
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

        ApplyRarityStyle(rarity);

        onBuy = onBuyCallback;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = canAfford && canBuyMore;
            buyButton.onClick.AddListener(() => onBuy?.Invoke());
        }
    }

    void ApplyRarityStyle(ItemRarity rarity)
    {
        Color bg = new Color(0.18f, 0.18f, 0.18f, 0.9f);
        Color frame = Color.white;
        Color text = Color.white;

        switch (rarity)
        {
            case ItemRarity.COMMON:
                bg = new Color(0.22f, 0.22f, 0.22f, 0.95f);
                frame = new Color(0.75f, 0.75f, 0.75f);
                break;

            case ItemRarity.UNCOMMON:
                bg = new Color(0.14f, 0.24f, 0.14f, 0.95f);
                frame = new Color(0.35f, 0.85f, 0.35f);
                break;

            case ItemRarity.RARE:
                bg = new Color(0.12f, 0.18f, 0.32f, 0.95f);
                frame = new Color(0.35f, 0.55f, 1f);
                break;

            case ItemRarity.EPIC:
                bg = new Color(0.26f, 0.12f, 0.32f, 0.95f);
                frame = new Color(0.8f, 0.35f, 1f);
                break;

            case ItemRarity.LEGENDARY:
                bg = new Color(0.36f, 0.22f, 0.08f, 0.95f);
                frame = new Color(1f, 0.72f, 0.15f);
                text = new Color(1f, 0.9f, 0.55f);
                break;
        }

        if (backgroundImage != null)
            backgroundImage.color = bg;

        if (frameImage != null)
            frameImage.color = frame;

        if (nameText != null)
            nameText.color = text;

        if (rarityText != null)
            rarityText.color = frame;
    }
}
