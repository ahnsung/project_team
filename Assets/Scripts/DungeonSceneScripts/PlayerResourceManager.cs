using UnityEngine;

// 플레이어 자원(체력 / 정신력 / 배고픔)을 관리하는 스크립트
// 역할:
// 1) 현재 자원값 저장
// 2) 최대 자원값 저장
// 3) 턴이 지날 때 정신력/배고픔 자동 감소
// 4) PlayerPrefs 저장/불러오기
public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance;

    [Header("Max Resource")]
    [SerializeField] private int maxHealth = 200;
    [SerializeField] private int maxMental = 200;
    [SerializeField] private int maxHunger = 200;

    [Header("Current Resource")]
    [SerializeField] private int currentHealth = 200;
    [SerializeField] private int currentMental = 200;
    [SerializeField] private int currentHunger = 200;

    [Header("Turn Rule")]
    [SerializeField] private int mentalDecreaseTurnInterval = 50; // 정신력 감소 주기
    [SerializeField] private int mentalDecreaseAmount = 10;       // 정신력 감소량

    [SerializeField] private int hungerDecreaseTurnInterval = 10; // 배고픔 감소 주기
    [SerializeField] private int hungerDecreaseAmount = 10;       // 배고픔 감소량

    // 마지막으로 처리한 턴
    // 예:
    // 현재 저장된 턴이 23이면, 24~최신 턴까지 어떤 감소가 필요한지 계산하는 기준이 됨
    private int lastProcessedTurn = 0;

    // 저장 키
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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Load();
    }

    private void Start()
    {
        // DungeonManager가 있으면 턴 변경 이벤트 구독
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnTurnChanged += HandleTurnChanged;

            // 저장된 던전 턴이 이미 있을 수 있으니 동기화
            int currentTurn = DungeonManager.Instance.CurrentTurn;

            // lastProcessedTurn이 현재 턴보다 크면 이상한 상태이므로 맞춰줌
            if (lastProcessedTurn > currentTurn)
                lastProcessedTurn = currentTurn;
        }
    }

    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
            DungeonManager.Instance.OnTurnChanged -= HandleTurnChanged;
    }

    // 턴이 바뀔 때마다 호출
    private void HandleTurnChanged(int newTurn)
    {
        // 이미 처리한 턴 다음부터 이번 턴까지 하나씩 검사
        for (int t = lastProcessedTurn + 1; t <= newTurn; t++)
        {
            // 정신력은 50턴마다 10 감소
            if (mentalDecreaseTurnInterval > 0 && t % mentalDecreaseTurnInterval == 0)
            {
                ChangeMental(-mentalDecreaseAmount, $"정신력 감소 규칙 발동 ({t}턴)");
            }

            // 배고픔은 10턴마다 10 감소
            if (hungerDecreaseTurnInterval > 0 && t % hungerDecreaseTurnInterval == 0)
            {
                ChangeHunger(-hungerDecreaseAmount, $"배고픔 감소 규칙 발동 ({t}턴)");
            }
        }

        // 여기까지 처리 완료
        lastProcessedTurn = newTurn;

        Save();
    }

    // 체력 변경
    // amount가 음수면 감소, 양수면 회복
    public void ChangeHealth(int amount, string reason = "")
    {
        int before = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        Debug.Log($"[Resource] 체력: {before} -> {currentHealth} / 이유: {reason}");
        Save();
    }

    // 정신력 변경
    public void ChangeMental(int amount, string reason = "")
    {
        int before = currentMental;
        currentMental = Mathf.Clamp(currentMental + amount, 0, maxMental);

        Debug.Log($"[Resource] 정신력: {before} -> {currentMental} / 이유: {reason}");
        Save();
    }

    // 배고픔 변경
    public void ChangeHunger(int amount, string reason = "")
    {
        int before = currentHunger;
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);

        Debug.Log($"[Resource] 배고픔: {before} -> {currentHunger} / 이유: {reason}");
        Save();
    }

    // 새 게임 시작 시 자원도 초기화할 때 쓸 함수
    public void ResetResourceToMax()
    {
        currentHealth = maxHealth;
        currentMental = maxMental;
        currentHunger = maxHunger;
        lastProcessedTurn = 0;

        Save();

        Debug.Log("[Resource] 체력 / 정신력 / 배고픔 초기화 완료");
    }

    // 현재 자원 저장
    private void Save()
    {
        PlayerPrefs.SetInt(HEALTH_KEY, currentHealth);
        PlayerPrefs.SetInt(MENTAL_KEY, currentMental);
        PlayerPrefs.SetInt(HUNGER_KEY, currentHunger);
        PlayerPrefs.SetInt(LAST_PROCESSED_TURN_KEY, lastProcessedTurn);

        PlayerPrefs.Save();
    }

    // 저장 자원 불러오기
    private void Load()
    {
        currentHealth = PlayerPrefs.GetInt(HEALTH_KEY, maxHealth);
        currentMental = PlayerPrefs.GetInt(MENTAL_KEY, maxMental);
        currentHunger = PlayerPrefs.GetInt(HUNGER_KEY, maxHunger);
        lastProcessedTurn = PlayerPrefs.GetInt(LAST_PROCESSED_TURN_KEY, 0);
    }
}