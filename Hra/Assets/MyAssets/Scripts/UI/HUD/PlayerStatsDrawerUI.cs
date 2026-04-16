using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsDrawerUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats;
    public RectTransform panel;
    public CanvasGroup panelCanvasGroup;
    public Button openButton;
    public Button closeButton;
    public TMP_Text statsText;

    [Header("Layout")]
    public bool startClosed = true;
    public bool autoComputeClosedPosition = true;
    public Vector2 openAnchoredPosition = new Vector2(20f, -20f);
    public Vector2 closedAnchoredPosition = new Vector2(-460f, -20f);
    public float hiddenOffset = 24f;
    public float slideDuration = 0.2f;

    [Header("Refresh")]
    public float refreshRate = 0.1f;

    Coroutine slideRoutine;
    float nextRefreshTime;
    bool isOpen;

    void Awake()
    {
        if (stats == null)
            stats = FindFirstObjectByType<PlayerStats>();

        if (panelCanvasGroup == null && panel != null)
            panelCanvasGroup = panel.GetComponent<CanvasGroup>();

        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (panel != null)
        {
            if (autoComputeClosedPosition)
            {
                float width = panel.rect.width;
                if (width > 0f)
                    closedAnchoredPosition = new Vector2(-(width + hiddenOffset), openAnchoredPosition.y);
            }

            ApplyStateImmediate(!startClosed);
        }
    }

    void OnDestroy()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
    }

    void Update()
    {
        if (stats == null || statsText == null)
            return;

        if (Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + refreshRate;
        statsText.text = BuildStatsText();
    }

    public void TogglePanel()
    {
        SetOpen(!isOpen, true);
    }

    public void OpenPanel()
    {
        SetOpen(true, true);
    }

    public void ClosePanel()
    {
        SetOpen(false, true);
    }

    void ApplyStateImmediate(bool open)
    {
        isOpen = open;

        if (panel != null)
            panel.anchoredPosition = open ? openAnchoredPosition : closedAnchoredPosition;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.interactable = open;
            panelCanvasGroup.blocksRaycasts = open;
        }

        if (openButton != null)
            openButton.gameObject.SetActive(!open);
    }

    void SetOpen(bool open, bool animated)
    {
        if (panel == null)
            return;

        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        isOpen = open;

        if (!animated || slideDuration <= 0f)
        {
            ApplyStateImmediate(open);
            return;
        }

        slideRoutine = StartCoroutine(SlideRoutine(open));
    }

    IEnumerator SlideRoutine(bool open)
    {
        Vector2 start = panel.anchoredPosition;
        Vector2 target = open ? openAnchoredPosition : closedAnchoredPosition;
        float time = 0f;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }

        if (openButton != null && open)
            openButton.gameObject.SetActive(false);

        while (time < slideDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / slideDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            panel.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        panel.anchoredPosition = target;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.interactable = open;
            panelCanvasGroup.blocksRaycasts = open;
        }

        if (openButton != null)
            openButton.gameObject.SetActive(!open);

        slideRoutine = null;
    }

    string BuildStatsText()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"HP: {stats.currentHP:0}/{stats.maxHp.Value:0}");
        sb.AppendLine($"Level: {stats.currentLevel}");
        sb.AppendLine($"XP: {stats.currentXP}/{stats.xpToNextLevel}");
        sb.AppendLine($"Damage: {stats.damage.Value:0.00}");
        sb.AppendLine($"Attack Speed: {stats.attackSpeed.Value:0.00}");
        sb.AppendLine($"Range: {stats.range.Value:0.00}");
        sb.AppendLine($"Move Speed: {stats.moveSpeed.Value:0.00}");
        sb.AppendLine($"Projectile Speed: {stats.projectileSpeed.Value:0.00}");
        sb.AppendLine($"Projectile Bonus: +{stats.GetProjectileBonus()}");
        sb.AppendLine($"Pickup Range: {stats.pickupRange.Value:0.00}");
        sb.AppendLine($"Money Gain: {stats.moneyGain.Value:0.00}");
        sb.AppendLine($"Luck: {stats.luck.Value:0.00}");
        sb.AppendLine($"Damage Taken: {stats.damageTaken.Value:0.00}");
        sb.AppendLine($"Crit Chance: {stats.critChance.Value:0.00}");
        sb.AppendLine($"Crit Damage: {stats.critDamage.Value:0.00}");
        sb.AppendLine($"Dodge: {stats.dodge.Value:0.00}");
        sb.AppendLine($"Item Price: {stats.itemPrice.Value:0.00}");
        return sb.ToString();
    }


}
