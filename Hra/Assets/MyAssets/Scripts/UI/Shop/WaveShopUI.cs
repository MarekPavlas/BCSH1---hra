using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class WaveShopUI : MonoBehaviour
{
    [Serializable]
    public struct RarityChanceConfig
    {
        public ItemRarity rarity;
        [Min(0f)] public float baseChance;
        public float luckBonusPerPoint;
    }

    [Header("Refs")]
    public WaveManager waveManager;
    public CurrencyWallet wallet;
    public PlayerWeapons playerWeapons;
    public PassiveInventory passiveInventory;
    public WeaponInventoryUI weaponInventoryUI;
    public PlayerStats playerStats;

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

    [Header("Rarity Chances")]
    public List<RarityChanceConfig> rarityChances = new();

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

    [Serializable]
    struct Offer
    {
        public bool isPassive;
        public WeaponDefinition weapon;
        public PassiveItemDefinition passive;
    }

    void Awake()
    {
        EnsureDefaultRarityChances();

        if (waveManager == null) waveManager = FindFirstObjectByType<WaveManager>();
        if (wallet == null) wallet = FindFirstObjectByType<CurrencyWallet>();
        if (playerWeapons == null) playerWeapons = FindFirstObjectByType<PlayerWeapons>();
        if (passiveInventory == null) passiveInventory = FindFirstObjectByType<PassiveInventory>();
        if (weaponInventoryUI == null) weaponInventoryUI = GetComponentInChildren<WeaponInventoryUI>(true);
        if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();

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

    void OnValidate()
    {
        EnsureDefaultRarityChances();
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
    def.rarity,
    () => BuyPassive(def, def.shopPrice, slotIndex)
);

            }
            else
            {
                WeaponDefinition def = off.weapon;
                bool owned = def != null && playerWeapons.HasWeapon(def.id);
                int currentLevel = def != null ? playerWeapons.GetLevel(def.id) : 0;
                int maxLevel = def != null ? playerWeapons.GetMaxLevel(def.id) : 1;

                bool canUnlockNewWeapon = playerWeapons != null && playerWeapons.CanUnlockMoreWeapons();
                bool canBuyMore = def != null && ((owned && currentLevel < maxLevel) || (!owned && canUnlockNewWeapon));
                bool canAfford = def != null && wallet != null && wallet.CanAfford(def.shopPrice);

                cards[i].Bind(
    def.icon,
    GetWeaponOfferLabel(def, owned, currentLevel, maxLevel),
    GetDescription(def != null ? def.description : null),
    def.shopPrice,
    canAfford,
    canBuyMore,
    def.rarity,
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

        if (!owned && !playerWeapons.CanUnlockMoreWeapons())
            return;

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

        bool tryPassiveFirst = canPickPassive && (!canPickWeapon || UnityEngine.Random.value < passiveOfferChance);

        if (tryPassiveFirst)
        {
            PassiveItemDefinition passive = PickPassiveByLuck(passivePool);
            if (passive != null)
            {
                offer = new Offer
                {
                    isPassive = true,
                    passive = passive
                };
                return true;
            }
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
            PassiveItemDefinition passive = PickPassiveByLuck(passivePool);
            if (passive != null)
            {
                offer = new Offer
                {
                    isPassive = true,
                    passive = passive
                };
                return true;
            }
        }

        return false;
    }

    WeaponDefinition PickWeapon(List<WeaponDefinition> weaponPool)
    {
        if (weaponPool == null || weaponPool.Count == 0)
            return null;

        if (playerWeapons == null || !preferUnownedWeapons)
            return PickWeaponByLuck(weaponPool);

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
            return PickWeaponByLuck(upgrades);

        if (upgrades.Count == 0)
            return PickWeaponByLuck(unowned);

        bool pickUpgrade = UnityEngine.Random.value < upgradeOfferChanceWhileUnownedExist;
        return PickWeaponByLuck(pickUpgrade ? upgrades : unowned);
    }

    WeaponDefinition PickWeaponByLuck(List<WeaponDefinition> pool)
    {
        return PickByLuck(pool, item => item.rarity);
    }

    PassiveItemDefinition PickPassiveByLuck(List<PassiveItemDefinition> pool)
    {
        return PickByLuck(pool, item => item.rarity);
    }

    T PickByLuck<T>(List<T> pool, Func<T, ItemRarity> getRarity) where T : class
    {
        if (pool == null || pool.Count == 0)
            return null;

        float luck = GetLuck();
        Dictionary<ItemRarity, List<T>> grouped = new();

        foreach (T item in pool)
        {
            if (item == null)
                continue;

            ItemRarity rarity = getRarity(item);

            if (!grouped.TryGetValue(rarity, out List<T> bucket))
            {
                bucket = new List<T>();
                grouped.Add(rarity, bucket);
            }

            bucket.Add(item);
        }

        if (grouped.Count == 0)
            return null;

        List<ItemRarity> activeRarities = new();
        List<float> activeWeights = new();
        float totalWeight = 0f;

        foreach (ItemRarity rarity in grouped.Keys)
        {
            float weight = GetRarityWeight(rarity, luck);
            if (weight <= 0f)
                continue;

            activeRarities.Add(rarity);
            activeWeights.Add(weight);
            totalWeight += weight;
        }

        if (totalWeight <= 0f)
        {
            List<T> allAvailable = grouped.Values.SelectMany(x => x).ToList();
            return allAvailable[UnityEngine.Random.Range(0, allAvailable.Count)];
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);

        for (int i = 0; i < activeRarities.Count; i++)
        {
            roll -= activeWeights[i];
            if (roll <= 0f)
            {
                List<T> bucket = grouped[activeRarities[i]];
                return bucket[UnityEngine.Random.Range(0, bucket.Count)];
            }
        }

        List<T> fallback = grouped.Values.SelectMany(x => x).ToList();
        return fallback[UnityEngine.Random.Range(0, fallback.Count)];
    }

    float GetRarityWeight(ItemRarity rarity, float luck)
    {
        for (int i = 0; i < rarityChances.Count; i++)
        {
            if (rarityChances[i].rarity == rarity)
                return Mathf.Max(0f, rarityChances[i].baseChance + rarityChances[i].luckBonusPerPoint * luck);
        }

        return 0f;
    }

    float GetLuck()
    {
        if (playerStats == null)
            return 0f;

        return Mathf.Max(0f, playerStats.Get(PlayerStatType.Luck));
    }

    void EnsureDefaultRarityChances()
    {
        if (rarityChances == null)
            rarityChances = new List<RarityChanceConfig>();

        AddMissingRarity(ItemRarity.COMMON, 70f, -4f);
        AddMissingRarity(ItemRarity.UNCOMMON, 20f, 2f);
        AddMissingRarity(ItemRarity.RARE, 5f, 1.2f);
        AddMissingRarity(ItemRarity.EPIC, 4f, 0.6f);
        AddMissingRarity(ItemRarity.LEGENDARY, 1f, 0.2f);
    }

    void AddMissingRarity(ItemRarity rarity, float baseChance, float luckBonusPerPoint)
    {
        for (int i = 0; i < rarityChances.Count; i++)
        {
            if (rarityChances[i].rarity == rarity)
                return;
        }

        rarityChances.Add(new RarityChanceConfig
        {
            rarity = rarity,
            baseChance = baseChance,
            luckBonusPerPoint = luckBonusPerPoint
        });
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
