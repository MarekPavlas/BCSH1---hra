using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopCardUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;
    public Button buyButton;

    private Action onBuy;

    public void Bind(Sprite icon, string title, string description, int price, bool canAfford, bool canBuyMore, Action onBuyCallback)
    {
        if (iconImage != null)
            iconImage.sprite = icon;

        if (nameText != null)
            nameText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        if (priceText != null)
            priceText.text = price.ToString();

        onBuy = onBuyCallback;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = canAfford && canBuyMore;
            buyButton.onClick.AddListener(() => onBuy?.Invoke());
        }
    }
}
