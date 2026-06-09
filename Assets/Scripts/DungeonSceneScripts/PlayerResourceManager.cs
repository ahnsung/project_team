using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance;

    [Header("Max Resource")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxMental = 75;
    [SerializeField] private int maxHunger = 125;

    [Header("Current Resource")]
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int currentMental = 75;
    [SerializeField] private int currentHunger = 125;

    [Header("Turn Rule")]
    [SerializeField] private int mentalDecreaseTurnInterval = 50;
    [SerializeField] private int mentalDecreaseAmount = 10;
    [SerializeField] private int hungerDecreaseTurnInterval = 10;
    [SerializeField] private int hungerDecreaseAmount = 10;

    private int lastProcessedTurn = 0;

    private const string HEALTH_KEY = "PLAYER_HEALTH";
    private const string MENTAL_KEY = "PLAYER_MENTAL";
    private const string HUNGER_KEY = "PLAYER_HUNGER";
    private const string LAST_PROCESSED_TURN_KEY = "PLAYER_LAST_PROCESSED_TURN";

    public int MaxHealth => maxHealth;
    public int MaxMental => maxMental;
    public int MaxHunger => maxHunger;

    public int CurrentHealth => currentHealth;
    public int CurrentMental => currentMental;
    public int CurrentHunger => currentHunger;

    private void Awake()
    {
        Instance = this;
        ApplyMaxResourceFromStats();
        Load();
        ClampAll();
    }

    private void Start()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnTurnChanged += HandleTurnChanged;

            int currentTurn = DungeonManager.Instance.CurrentTurn;
            if (lastProcessedTurn > currentTurn)
                lastProcessedTurn = currentTurn;
        }
    }

    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
            DungeonManager.Instance.OnTurnChanged -= HandleTurnChanged;
    }

    public void ApplyMaxResourceFromStats()
    {
        if (PlayerStats.Instance == null)
            return;

        maxHealth = PlayerStats.Instance.MaxHealth;
        maxHunger = PlayerStats.Instance.MaxHunger;
        maxMental = PlayerStats.Instance.MaxMental;

        ClampAll();
        Save();
    }

    private void HandleTurnChanged(int newTurn)
    {
        for (int t = lastProcessedTurn + 1; t <= newTurn; t++)
        {
            if (mentalDecreaseTurnInterval > 0 && t % mentalDecreaseTurnInterval == 0)
                ChangeMental(-ApplyDecreasePenalty(mentalDecreaseAmount), $"정신력 감소 규칙 발동 ({t}턴)");

            if (hungerDecreaseTurnInterval > 0 && t % hungerDecreaseTurnInterval == 0)
                ChangeHunger(-ApplyDecreasePenalty(hungerDecreaseAmount), $"배고픔 감소 규칙 발동 ({t}턴)");
        }

        lastProcessedTurn = newTurn;
        Save();
    }

    public void ChangeHealth(int amount, string reason = "")
    {
        int finalAmount = amount;

        if (amount < 0)
            finalAmount = -ApplyDecreasePenalty(Mathf.Abs(amount));

        if (amount > 0 && IsHungerHealthHealPenaltyActive())
            finalAmount = Mathf.RoundToInt(amount * 0.5f);

        int before = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + finalAmount, 0, maxHealth);

        Debug.Log($"[Resource] 체력: {before} -> {currentHealth} / 변화량: {finalAmount} / 이유: {reason}");
        Save();
    }

    public void ChangeMental(int amount, string reason = "")
    {
        int finalAmount = amount;

        if (amount < 0)
            finalAmount = -ApplyDecreasePenalty(Mathf.Abs(amount));

        if (amount > 0 && IsHungerMentalHealPenaltyActive())
            finalAmount = Mathf.RoundToInt(amount * 0.5f);

        int before = currentMental;
        currentMental = Mathf.Clamp(currentMental + finalAmount, 0, maxMental);

        Debug.Log($"[Resource] 정신력: {before} -> {currentMental} / 변화량: {finalAmount} / 이유: {reason}");
        Save();
    }

    public void ChangeHunger(int amount, string reason = "")
    {
        int finalAmount = amount;

        if (amount < 0)
            finalAmount = -ApplyDecreasePenalty(Mathf.Abs(amount));

        int before = currentHunger;
        currentHunger = Mathf.Clamp(currentHunger + finalAmount, 0, maxHunger);

        Debug.Log($"[Resource] 배고픔: {before} -> {currentHunger} / 변화량: {finalAmount} / 이유: {reason}");
        Save();
    }

    public void HealHealthItem(int amount)
    {
        ChangeHealth(amount, "아이템 체력 회복");
    }

    public void HealHungerItem(int amount)
    {
        ChangeHunger(amount, "아이템 배고픔 회복");
    }

    public void HealMentalItem(int amount)
    {
        ChangeMental(amount, "아이템 정신력 회복");
    }

    private int ApplyDecreasePenalty(int amount)
    {
        if (IsHungerAllDecreasePenaltyActive())
            return Mathf.RoundToInt(amount * 1.5f);

        return amount;
    }

    public int GetMentalAccuracyPenalty()
    {
        float mentalRatio = GetMentalRatio();

        if (mentalRatio > 0.75f)
            return 0;

        float lostRatio = 1f - mentalRatio;
        int lostTenPercentCount = Mathf.FloorToInt(lostRatio / 0.1f);

        return lostTenPercentCount * 5;
    }

    public bool IsMentalDamagePenaltyActive()
    {
        return GetMentalRatio() <= 0.25f;
    }

    public bool IsHungerHealthHealPenaltyActive()
    {
        return GetHungerRatio() <= 0.70f;
    }

    public bool IsHungerMentalHealPenaltyActive()
    {
        return GetHungerRatio() <= 0.50f;
    }

    public bool IsHungerAllDecreasePenaltyActive()
    {
        return GetHungerRatio() <= 0.25f;
    }

    private float GetMentalRatio()
    {
        if (maxMental <= 0) return 0f;
        return (float)currentMental / maxMental;
    }

    private float GetHungerRatio()
    {
        if (maxHunger <= 0) return 0f;
        return (float)currentHunger / maxHunger;
    }

    public void ResetResourceToMax()
    {
        ApplyMaxResourceFromStats();

        currentHealth = maxHealth;
        currentMental = maxMental;
        currentHunger = maxHunger;
        lastProcessedTurn = 0;

        Save();
        Debug.Log("[Resource] 체력 / 정신력 / 배고픔 초기화 완료");
    }

    private void ClampAll()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentMental = Mathf.Clamp(currentMental, 0, maxMental);
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(HEALTH_KEY, currentHealth);
        PlayerPrefs.SetInt(MENTAL_KEY, currentMental);
        PlayerPrefs.SetInt(HUNGER_KEY, currentHunger);
        PlayerPrefs.SetInt(LAST_PROCESSED_TURN_KEY, lastProcessedTurn);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        currentHealth = PlayerPrefs.GetInt(HEALTH_KEY, maxHealth);
        currentMental = PlayerPrefs.GetInt(MENTAL_KEY, maxMental);
        currentHunger = PlayerPrefs.GetInt(HUNGER_KEY, maxHunger);
        lastProcessedTurn = PlayerPrefs.GetInt(LAST_PROCESSED_TURN_KEY, 0);
    }
}