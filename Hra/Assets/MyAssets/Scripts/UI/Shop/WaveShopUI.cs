using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class WaveShopUI : MonoBehaviour
{
    [Header("Refs")]
    public WaveManager waveManager;
    public CurrencyWallet wallet;
    public PlayerWeapons playerWeapons;
    public PassiveInventory passiveInventory;
    public WeaponInventoryUI weaponInventoryUI;

    [Header("UI")]
    public GameObject panelRoot;
    public List<ShopCardUI> cards = new();
    public Button continueButton;
    public TextMeshProUGUI currencyText;
    public string currencyFormat = "Gold: {0}";
    public bool showDebugLogs = true;

    [Header("Reroll UI")]
    public Button rerollButton;
    public TextMeshProUGUI rerollPriceText;
    public string rerollPriceFormat = "Reroll: {0}";

    [Header("Reroll Cost")]
    [Min(0)] public int baseRerollPrice = 50;
    [Range(0f, 500f)] public float rerollPriceIncreasePercent = 25f;

    [Header("Pools")]
    public List<WeaponDefinition> allWeapons = new();
    public List<PassiveItemDefinition> allPassives = new();
    [Range(0f, 1f)] public float passiveOfferChance = 0.5f;

    [Header("Offers")]
    [Min(1)] public int offersCount = 3;
    public bool preferUnownedWeapons = true;
    [Range(0f, 1f)] public float upgradeOfferChanceWhileUnownedExist = 0.3f;

    [Header("Pause while open")]
    public bool pauseGameWhileOpen = true;
    public MonoBehaviour[] disableWhileOpen;
    public bool stopNavMeshAgents = true;

    float prevTimeScale = 1f;
    int rerollCount = 0;
    readonly List<AgentState> stoppedAgents = new();
    List<Offer> currentOffers = new();

    struct AgentState
    {
        public NavMeshAgent agent;
        public bool wasStopped;
    }

    [System.Serializable]
    struct Offer
    {
        public bool isPassive;
        public WeaponDefinition weapon;
        public PassiveItemDefinition passive;
    }

    void Awake()
    {
        if (waveManager == null) waveManager = FindFirstObjectByType<WaveManager>();
        if (wallet == null) wallet = FindFirstObjectByType<CurrencyWallet>();
        if (playerWeapons == null) playerWeapons = FindFirstObjectByType<PlayerWeapons>();
        if (passiveInventory == null) passiveInventory = FindFirstObjectByType<PassiveInventory>();
        if (weaponInventoryUI == null) weaponInventoryUI = GetComponentInChildren<WeaponInventoryUI>(true);

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (weaponInventoryUI != null)
            weaponInventoryUI.gameObject.SetActive(false);

        if (rerollButton != null)
            rerollButton.onClick.AddListener(HandleRerollClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(CloseShopAndContinue);

        UpdateRerollUI();
        UpdateCurrencyUI();
    }

    void OnDestroy()
    {
        if (rerollButton != null)
            rerollButton.onClick.RemoveListener(HandleRerollClicked);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(CloseShopAndContinue);
    }

    void OnEnable()
    {
        if (waveManager != null)
        {
            waveManager.OnIntermissionStarted += HandleIntermissionStarted;
            waveManager.OnWaveStarted += HandleWaveStarted;
        }
    }

    void OnDisable()
    {
        if (waveManager != null)
        {
            waveManager.OnIntermissionStarted -= HandleIntermissionStarted;
            waveManager.OnWaveStarted -= HandleWaveStarted;
        }
    }

    void Update()
    {
        if (panelRoot != null && panelRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            CloseShopAndContinue();
    }

    void HandleIntermissionStarted(int nextWaveIndex)
    {
        if (waveManager != null && waveManager.testModeNoShop)
            return;

        rerollCount = 0;
        OpenShop();
    }

    void HandleWaveStarted(int currentWaveIndex)
    {
        CloseShopVisuals();
    }

    public void OpenShop()
    {
        if (panelRoot == null || wallet == null || playerWeapons == null)
            return;

        if (pauseGameWhileOpen)
            SetPaused(true);

        panelRoot.SetActive(true);

        if (weaponInventoryUI != null)
        {
            weaponInventoryUI.gameObject.SetActive(true);
            weaponInventoryUI.Refresh();
        }

        UpdateCurrencyUI();
        GenerateAndBindOffers();

        if (showDebugLogs)
            Debug.Log("[Shop] Opened");
    }

    void UpdateCurrencyUI()
    {
        if (currencyText != null && wallet != null)
            currencyText.text = string.Format(currencyFormat, wallet.GetCurrentAmount());
    }

    void GenerateAndBindOffers()
    {
        currentOffers = BuildOffers(offersCount, allWeapons, allPassives);
        RefreshShopDisplay();
    }

    void RefreshShopDisplay()
    {
        BindOffers(currentOffers);
        UpdateRerollUI();
        UpdateCurrencyUI();
    }

    void BindOffers(List<Offer> offers)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null)
                continue;

            if (i >= offers.Count || !IsValidOffer(offers[i]))
            {
                cards[i].gameObject.SetActive(false);
                continue;
            }

            cards[i].gameObject.SetActive(true);
            Offer off = offers[i];
            int slotIndex = i;

            if (off.isPassive)
            {
                PassiveItemDefinition def = off.passive;
                bool canBuy = def != null && passiveInventory != null && passiveInventory.CanAdd(def);
                bool canAfford = def != null && wallet != null && wallet.CanAfford(def.shopPrice);

                cards[i].Bind(
                    def.icon,
                    def.displayName,
                    GetDescription(def != null ? def.description : null),
                    def.shopPrice,
                    canAfford,
                    canBuy,
                    () => BuyPassive(def, def.shopPrice, slotIndex)
                );
            }
            else
            {
                WeaponDefinition def = off.weapon;
                bool owned = def != null && playerWeapons.HasWeapon(def.id);
                int currentLevel = def != null ? playerWeapons.GetLevel(def.id) : 0;
                int maxLevel = def != null ? playerWeapons.GetMaxLevel(def.id) : 1;

                bool canBuy = def != null && (!owned || currentLevel < maxLevel);
                bool canAfford = def != null && wallet != null && wallet.CanAfford(def.shopPrice);

                cards[i].Bind(
                    def.icon,
                    GetWeaponOfferLabel(def, owned, currentLevel, maxLevel),
                    GetDescription(def != null ? def.description : null),
                    def.shopPrice,
                    canAfford,
                    canBuy,
                    () => BuyWeapon(def, def.shopPrice, slotIndex)
                );
            }
        }
    }

    string GetDescription(string rawDescription)
    {
        if (string.IsNullOrWhiteSpace(rawDescription))
            return "-";

        return rawDescription.Trim();
    }

    bool IsValidOffer(Offer offer)
    {
        return offer.isPassive ? offer.passive != null : offer.weapon != null;
    }

    string GetWeaponOfferLabel(WeaponDefinition def, bool owned, int currentLevel, int maxLevel)
    {
        if (def == null) return "Unknown";
        if (!owned) return def.displayName;
        if (currentLevel >= maxLevel) return $"{def.displayName} (MAX)";
        return $"{def.displayName} Lv {currentLevel}->{currentLevel + 1}";
    }

    void HandleRerollClicked()
    {
        if (panelRoot == null || !panelRoot.activeSelf || wallet == null)
            return;

        int price = GetCurrentRerollPrice();
        if (!wallet.TrySpend(price))
            return;

        rerollCount++;
        GenerateAndBindOffers();

        if (showDebugLogs)
            Debug.Log($"[Shop] Reroll used. Paid={price}");
    }

    int GetCurrentRerollPrice()
    {
        float multiplier = 1f + (rerollCount * (rerollPriceIncreasePercent / 100f));
        return Mathf.Max(0, Mathf.RoundToInt(baseRerollPrice * multiplier));
    }

    void UpdateRerollUI()
    {
        int price = GetCurrentRerollPrice();

        if (rerollPriceText != null)
            rerollPriceText.text = string.Format(rerollPriceFormat, price);

        if (rerollButton != null)
            rerollButton.interactable = panelRoot != null && panelRoot.activeSelf && wallet != null && wallet.CanAfford(price);
    }

    void BuyWeapon(WeaponDefinition def, int price, int slotIndex)
    {
        if (def == null || playerWeapons == null || wallet == null)
            return;

        bool owned = playerWeapons.HasWeapon(def.id);
        int maxLevel = playerWeapons.GetMaxLevel(def.id);

        if (owned && playerWeapons.GetLevel(def.id) >= maxLevel)
            return;

        if (!wallet.TrySpend(price))
            return;

        if (!owned)
            playerWeapons.Unlock(def.id, fromShop: true);
        else
            playerWeapons.Upgrade(def.id, maxLevel, fromShop: true);

        if (weaponInventoryUI != null)
            weaponInventoryUI.Refresh();

        ReplaceOfferAt(slotIndex);
        RefreshShopDisplay();

        if (showDebugLogs)
            Debug.Log($"[Shop] Purchased {def.displayName}");
    }

    void BuyPassive(PassiveItemDefinition def, int price, int slotIndex)
    {
        if (def == null || passiveInventory == null || wallet == null)
            return;

        if (!passiveInventory.CanAdd(def))
            return;

        if (!wallet.TrySpend(price))
            return;

        passiveInventory.TryAdd(def, 1);

        ReplaceOfferAt(slotIndex);
        RefreshShopDisplay();

        if (showDebugLogs)
            Debug.Log($"[Shop] Purchased passive {def.displayName}");
    }

    void ReplaceOfferAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentOffers.Count)
        {
            GenerateAndBindOffers();
            return;
        }

        HashSet<WeaponDefinition> blockedWeapons = new();
        HashSet<PassiveItemDefinition> blockedPassives = new();

        for (int i = 0; i < currentOffers.Count; i++)
        {
            if (i == slotIndex)
                continue;

            Offer offer = currentOffers[i];
            if (!IsValidOffer(offer))
                continue;

            if (offer.isPassive)
            {
                if (offer.passive != null)
                    blockedPassives.Add(offer.passive);
            }
            else
            {
                if (offer.weapon != null)
                    blockedWeapons.Add(offer.weapon);
            }
        }

        List<WeaponDefinition> weaponPool = allWeapons
            .Where(CanOfferWeapon)
            .Where(w => w != null && !blockedWeapons.Contains(w))
            .Distinct()
            .ToList();

        List<PassiveItemDefinition> passivePool = allPassives
            .Where(CanOfferPassive)
            .Where(p => p != null && !blockedPassives.Contains(p))
            .Distinct()
            .ToList();

        if (TryBuildSingleOffer(weaponPool, passivePool, out Offer replacement))
            currentOffers[slotIndex] = replacement;
        else
            currentOffers[slotIndex] = default;
    }

    List<Offer> BuildOffers(int count, List<WeaponDefinition> weaponAll, List<PassiveItemDefinition> passiveAll)
    {
        List<Offer> result = new();

        List<WeaponDefinition> weaponPool = weaponAll
            .Where(CanOfferWeapon)
            .Distinct()
            .ToList();

        List<PassiveItemDefinition> passivePool = passiveAll
            .Where(CanOfferPassive)
            .Distinct()
            .ToList();

        while (result.Count < count)
        {
            if (!TryBuildSingleOffer(weaponPool, passivePool, out Offer offer))
                break;

            result.Add(offer);

            if (offer.isPassive)
                passivePool.Remove(offer.passive);
            else
                weaponPool.Remove(offer.weapon);
        }

        return result;
    }

    bool TryBuildSingleOffer(List<WeaponDefinition> weaponPool, List<PassiveItemDefinition> passivePool, out Offer offer)
    {
        offer = default;

        bool canPickPassive = passivePool != null && passivePool.Count > 0;
        bool canPickWeapon = weaponPool != null && weaponPool.Count > 0;

        if (!canPickPassive && !canPickWeapon)
            return false;

        bool tryPassiveFirst = canPickPassive && (!canPickWeapon || Random.value < passiveOfferChance);

        if (tryPassiveFirst)
        {
            int idx = Random.Range(0, passivePool.Count);
            offer = new Offer
            {
                isPassive = true,
                passive = passivePool[idx]
            };
            return true;
        }

        if (canPickWeapon)
        {
            WeaponDefinition weapon = PickWeapon(weaponPool);
            if (weapon != null)
            {
                offer = new Offer
                {
                    isPassive = false,
                    weapon = weapon
                };
                return true;
            }
        }

        if (canPickPassive)
        {
            int idx = Random.Range(0, passivePool.Count);
            offer = new Offer
            {
                isPassive = true,
                passive = passivePool[idx]
            };
            return true;
        }

        return false;
    }

    WeaponDefinition PickWeapon(List<WeaponDefinition> weaponPool)
    {
        if (weaponPool == null || weaponPool.Count == 0)
            return null;

        if (playerWeapons == null || !preferUnownedWeapons)
            return weaponPool[Random.Range(0, weaponPool.Count)];

        List<WeaponDefinition> unowned = weaponPool
            .Where(w => w != null && !playerWeapons.HasWeapon(w.id))
            .ToList();

        List<WeaponDefinition> upgrades = weaponPool
            .Where(w => w != null &&
                        playerWeapons.HasWeapon(w.id) &&
                        playerWeapons.GetLevel(w.id) < playerWeapons.GetMaxLevel(w.id))
            .ToList();

        if (unowned.Count == 0 && upgrades.Count == 0)
            return null;

        if (unowned.Count == 0)
            return upgrades[Random.Range(0, upgrades.Count)];

        if (upgrades.Count == 0)
            return unowned[Random.Range(0, unowned.Count)];

        bool pickUpgrade = Random.value < upgradeOfferChanceWhileUnownedExist;
        List<WeaponDefinition> source = pickUpgrade ? upgrades : unowned;

        return source[Random.Range(0, source.Count)];
    }

    bool CanOfferWeapon(WeaponDefinition def)
    {
        if (def == null || playerWeapons == null)
            return false;

        if (!playerWeapons.HasWeapon(def.id))
            return true;

        return playerWeapons.GetLevel(def.id) < playerWeapons.GetMaxLevel(def.id);
    }

    bool CanOfferPassive(PassiveItemDefinition def)
    {
        if (def == null)
            return false;

        return passiveInventory == null || passiveInventory.CanAdd(def);
    }

    public void CloseShopAndContinue()
    {
        CloseShopVisuals();

        if (pauseGameWhileOpen)
            SetPaused(false);

        if (waveManager != null)
            waveManager.StartNextWaveNow();
    }

    void CloseShopVisuals()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (weaponInventoryUI != null)
            weaponInventoryUI.gameObject.SetActive(false);
    }

    void SetPaused(bool paused)
    {
        if (paused)
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            foreach (var m in disableWhileOpen)
                if (m != null) m.enabled = false;

            if (stopNavMeshAgents)
                StopAgents();
        }
        else
        {
            Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            foreach (var m in disableWhileOpen)
                if (m != null) m.enabled = true;

            if (stopNavMeshAgents)
                RestoreAgents();
        }
    }

    void StopAgents()
    {
        stoppedAgents.Clear();
        NavMeshAgent[] agents = FindObjectsByType<NavMeshAgent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var agent in agents)
        {
            if (agent == null)
                continue;

            stoppedAgents.Add(new AgentState
            {
                agent = agent,
                wasStopped = agent.isStopped
            });

            agent.isStopped = true;
        }
    }

    void RestoreAgents()
    {
        foreach (var state in stoppedAgents)
            if (state.agent != null)
                state.agent.isStopped = state.wasStopped;

        stoppedAgents.Clear();
    }
}
