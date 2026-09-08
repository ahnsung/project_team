using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTileEventManager : MonoBehaviour
{
    public static DungeonTileEventManager Instance
    {
        get;
        private set;
    }


    // =========================================================
    // 사용 완료 타일
    // =========================================================

    // Chest:
    // 저장되는 영구 상태
    private readonly HashSet<Vector2Int>
        usedChestTiles =
            new HashSet<Vector2Int>();


    // Key:
    // 획득한 열쇠 타일.
    // SaveManager에 저장한다.
    private readonly HashSet<Vector2Int>
        usedKeyTiles =
            new HashSet<Vector2Int>();


    // Farming:
    // 현재 던전 입장 동안만 유지.
    private readonly HashSet<Vector2Int>
        usedFarmingTiles =
            new HashSet<Vector2Int>();


    // =========================================================
    // References
    // =========================================================

    [Header("References")]
    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private BattleManager battleManager;


    // =========================================================
    // General Battle
    // =========================================================

    [Header("General Battle")]

    [Range(0f, 100f)]
    [SerializeField]
    private float generalBattleStartChance = 10f;


    [Range(0f, 100f)]
    [SerializeField]
    private float generalBattleIncreaseAmount = 10f;


    private float currentGeneralBattleChance;


    public float CurrentGeneralBattleChance
    {
        get
        {
            return currentGeneralBattleChance;
        }
    }


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


        // General 전투 확률 초기값
        ResetGeneralBattleChance();
    }


    private void Start()
    {
        ResolveReferences();
    }


    private void ResolveReferences()
    {
        if (dungeonManager == null)
        {
            dungeonManager =
                DungeonManager.Instance;
        }


        if (battleManager == null)
        {
            battleManager =
                FindFirstObjectByType<BattleManager>();
        }
    }


    // =========================================================
    // 타일 진입 시 자동 이벤트
    // =========================================================

    public IEnumerator ExecuteEnterEvent()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            Debug.LogError(
                "[DungeonTileEventManager] " +
                "DungeonManager가 없습니다."
            );

            yield break;
        }


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonTileEventManager] " +
                "현재 타일 데이터가 없습니다."
            );

            yield break;
        }


        Debug.Log(
            "[DungeonTileEventManager] 타일 진입: " +
            $"({tile.X}, {tile.Y}) / " +
            $"{tile.TileType}"
        );


        switch (tile.TileType)
        {
            // General에서만 일반 랜덤 전투
            case DungeonTileType.General:

                yield return
                    HandleGeneralEnter(tile);

                break;


            // Trap은 진입 즉시 자동 판정
            case DungeonTileType.Trap:

                yield return
                    HandleTrapEnter(tile);

                break;


            // Teleport는 진입 즉시 자동 발동
            case DungeonTileType.Teleport:

                yield return
                    HandleTeleportEnter(tile);

                break;


            // 나머지는 E 상호작용
            default:

                break;
        }
    }


    // =========================================================
    // E 키 상호작용
    // =========================================================

    public IEnumerator ExecuteInteraction()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            Debug.LogError(
                "[DungeonTileEventManager] " +
                "DungeonManager가 없습니다."
            );

            yield break;
        }


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonTileEventManager] " +
                "현재 타일 데이터가 없습니다."
            );

            yield break;
        }


        Debug.Log(
            "[DungeonTileEventManager] E 상호작용: " +
            $"({tile.X}, {tile.Y}) / " +
            $"{tile.TileType}"
        );


        switch (tile.TileType)
        {
            case DungeonTileType.Farming:

                yield return
                    HandleFarming(tile);

                break;


            case DungeonTileType.Key:

                yield return
                    HandleKey(tile);

                break;


            case DungeonTileType.Chest:

                yield return
                    HandleChest(tile);

                break;


            case DungeonTileType.PuzzleLetter:

                yield return
                    HandlePuzzleLetter(tile);

                break;


            case DungeonTileType.EventHint:

                yield return
                    HandleEventHint(tile);

                break;


            case DungeonTileType.Rest:

                yield return
                    HandleRest(tile);

                break;


            case DungeonTileType.Boss:

                yield return
                    HandleBoss(tile);

                break;


            default:

                Debug.Log(
                    "[TileEvent] 현재 타일은 " +
                    "E 상호작용 대상이 아닙니다: " +
                    tile.TileType
                );

                break;
        }
    }


    // =========================================================
    // 현재 타일이 E 상호작용 가능한지
    // =========================================================

    public bool CanInteractCurrentTile()
    {
        ResolveReferences();


        if (dungeonManager == null)
            return false;


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        if (tile == null)
            return false;


        switch (tile.TileType)
        {
            case DungeonTileType.Farming:
            case DungeonTileType.Key:
            case DungeonTileType.Chest:
            case DungeonTileType.PuzzleLetter:
            case DungeonTileType.EventHint:
            case DungeonTileType.Rest:
            case DungeonTileType.Boss:

                return true;


            default:

                return false;
        }
    }


    // =========================================================
    // General
    // =========================================================

    private IEnumerator HandleGeneralEnter(
        DungeonTileData tile)
    {
        ResolveReferences();


        Debug.Log(
            "[General] 타일 진입\n" +
            $"좌표: ({tile.X}, {tile.Y})"
        );


        // -----------------------------------------------------
        // 현재 누적 확률로 전투 판정
        // -----------------------------------------------------

        float roll =
            Random.Range(
                0f,
                100f
            );


        bool battleOccurs =
            roll <
            currentGeneralBattleChance;


        Debug.Log(
            "[General] 전투 판정\n" +
            $"현재 전투 확률: " +
            $"{currentGeneralBattleChance:F0}%\n" +
            $"주사위 값: {roll:F2}"
        );


        // -----------------------------------------------------
        // 전투 미발생
        // -----------------------------------------------------

        if (!battleOccurs)
        {
            float oldChance =
                currentGeneralBattleChance;


            currentGeneralBattleChance =
                Mathf.Clamp(
                    currentGeneralBattleChance +
                    generalBattleIncreaseAmount,
                    0f,
                    100f
                );


            Debug.Log(
                "[General] 전투가 발생하지 않았습니다.\n" +
                $"이번 확률: {oldChance:F0}%\n" +
                $"다음 General 전투 확률: " +
                $"{currentGeneralBattleChance:F0}%"
            );


            yield break;
        }


        // -----------------------------------------------------
        // 전투 판정 성공
        // -----------------------------------------------------

        if (battleManager == null)
        {
            Debug.LogWarning(
                "[General] 전투 판정에는 성공했지만 " +
                "BattleManager가 없습니다.\n" +
                "확률은 초기화하지 않습니다."
            );


            yield break;
        }


        Debug.Log(
            "[General] 전투 발생!\n" +
            $"전투 발생 확률: " +
            $"{currentGeneralBattleChance:F0}%\n" +
            $"주사위 값: {roll:F2}"
        );


        // -----------------------------------------------------
        // 실제 전투가 발생하므로
        // 다음 확률을 10%로 초기화
        // -----------------------------------------------------

        ResetGeneralBattleChance();


        Debug.Log(
            "[General] 전투 확률 초기화\n" +
            $"다음 General 전투 확률: " +
            $"{currentGeneralBattleChance:F0}%"
        );


        // -----------------------------------------------------
        // 기존 전투 시스템 실행
        // -----------------------------------------------------

        yield return StartCoroutine(
            battleManager.StartBattleEncounter()
        );


        // 전투가 끝날 때까지 대기
        while (
            battleManager.IsBattleRunning())
        {
            yield return null;
        }


        Debug.Log(
            "[General] 전투 종료"
        );
    }


    // =========================================================
    // General Battle Chance
    // =========================================================

    public void ResetGeneralBattleChance()
    {
        currentGeneralBattleChance =
            Mathf.Clamp(
                generalBattleStartChance,
                0f,
                100f
            );
    }


    // =========================================================
    // Farming
    // =========================================================

    private IEnumerator HandleFarming(
        DungeonTileData tile)
    {
        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        // -----------------------------------------------------
        // 이미 사용한 Farming 타일
        // -----------------------------------------------------

        if (usedFarmingTiles.Contains(
            position))
        {
            Debug.Log(
                "[Farming] 이미 파밍한 장소입니다.\n" +
                "던전을 나갔다 다시 들어오면 " +
                "다시 사용할 수 있습니다."
            );

            yield break;
        }


        // -----------------------------------------------------
        // Farming Data
        // -----------------------------------------------------

        FarmingDataLoader farmingLoader =
            FarmingDataLoader.Instance;


        if (farmingLoader == null)
        {
            Debug.LogError(
                "[Farming] FarmingDataLoader를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        FarmingTileData farmingData =
            farmingLoader.GetData(
                tile.X,
                tile.Y
            );


        if (farmingData == null)
        {
            Debug.LogWarning(
                "[Farming] Farming 타일인데 " +
                "Farming_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }


        // -----------------------------------------------------
        // ItemGroup Database
        // -----------------------------------------------------

        FarmingItemGroupDatabase
            itemGroupDatabase =
                FarmingItemGroupDatabase.Instance;


        if (itemGroupDatabase == null)
        {
            Debug.LogError(
                "[Farming] " +
                "FarmingItemGroupDatabase를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        List<int> groupItemIDs =
            itemGroupDatabase.GetItemIDs(
                farmingData.itemGroup
            );


        if (groupItemIDs == null ||
            groupItemIDs.Count == 0)
        {
            Debug.LogWarning(
                "[Farming] ItemGroup에 " +
                "아이템이 없습니다.\n" +
                $"ItemGroup: " +
                $"{farmingData.itemGroup}"
            );

            yield break;
        }


        // -----------------------------------------------------
        // 수량 추첨
        // -----------------------------------------------------

        int amount =
            Random.Range(
                farmingData.minItemQuantity,
                farmingData.maxItemQuantity + 1
            );


        // 보상창 최대 6개
        amount =
            Mathf.Clamp(
                amount,
                1,
                6
            );


        Debug.Log(
            "[Farming] 파밍 시작\n" +
            $"좌표: ({tile.X}, {tile.Y})\n" +
            $"ItemGroup: " +
            $"{farmingData.itemGroup}\n" +
            $"수량 범위: " +
            $"{farmingData.minItemQuantity}" +
            "~" +
            $"{farmingData.maxItemQuantity}\n" +
            $"이번 추첨 수량: {amount}"
        );


        // -----------------------------------------------------
        // Reward 생성
        // -----------------------------------------------------

        List<ChestItemData> rewards =
            new List<ChestItemData>();


        for (
            int i = 0;
            i < amount;
            i++)
        {
            int randomIndex =
                Random.Range(
                    0,
                    groupItemIDs.Count
                );


            int itemID =
                groupItemIDs[
                    randomIndex
                ];


            if (itemID <= 0)
            {
                Debug.LogWarning(
                    "[Farming] 잘못된 ItemID를 " +
                    "건너뜁니다: " +
                    itemID
                );

                continue;
            }


            ChestItemData reward =
                new ChestItemData(
                    itemID,
                    1
                );


            rewards.Add(
                reward
            );
        }


        if (rewards.Count == 0)
        {
            Debug.LogWarning(
                "[Farming] 생성된 보상이 없습니다."
            );

            yield break;
        }


        // -----------------------------------------------------
        // Reward UI
        // -----------------------------------------------------

        DungeonRewardUI rewardUI =
            DungeonRewardUI.Instance;


        if (rewardUI == null)
        {
            Debug.LogError(
                "[Farming] DungeonRewardUI를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        yield return
            rewardUI.ShowChestRewards(
                rewards
            );


        // -----------------------------------------------------
        // 실제 획득 여부
        // -----------------------------------------------------

        if (rewardUI.AnyItemAcquired)
        {
            usedFarmingTiles.Add(
                position
            );


            Debug.Log(
                "[Farming] 파밍 완료: " +
                position
            );
        }
        else
        {
            Debug.Log(
                "[Farming] 획득한 아이템 없음: " +
                position
            );
        }
    }


    // =========================================================
    // Trap
    // =========================================================

    private IEnumerator HandleTrapEnter(
        DungeonTileData tile)
    {
        TrapDataLoader trapLoader =
            TrapDataLoader.Instance;


        if (trapLoader == null)
        {
            Debug.LogError(
                "[TileEvent] TrapDataLoader를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        TrapTileData trapData =
            trapLoader.GetData(
                tile.X,
                tile.Y
            );


        if (trapData == null)
        {
            Debug.LogWarning(
                "[TileEvent] Trap 타일인데 " +
                "Trap_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }


        float roll =
            Random.Range(
                0f,
                100f
            );


        bool triggered =
            roll <
            trapData.trapPossibility;


        Debug.Log(
            "[Trap] 함정 판정\n" +
            $"좌표: " +
            $"({trapData.x}, {trapData.y})\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"발동 확률: " +
            $"{trapData.trapPossibility}%\n" +
            $"주사위 값: {roll:F2}"
        );


        if (!triggered)
        {
            Debug.Log(
                "[Trap] 함정을 회피했습니다."
            );

            yield break;
        }


        Debug.Log(
            "[Trap] 함정 발동!\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"지속 턴: {trapData.trapAmount}"
        );


        /*
         * TrapType 1~6 실제 효과는
         * 기획 데이터 확정 후 연결.
         */

        yield break;
    }


    // =========================================================
    // Teleport
    // =========================================================

    private IEnumerator HandleTeleportEnter(
        DungeonTileData tile)
    {
        Debug.Log(
            "[TileEvent] Teleport 자동 이벤트: " +
            $"({tile.X}, {tile.Y})"
        );


        /*
         * Teleport는 다음 단계에서 구현.
         */

        yield break;
    }


    // =========================================================
    // Key
    // =========================================================

    private IEnumerator HandleKey(
        DungeonTileData tile)
    {
        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        // -----------------------------------------------------
        // 이미 획득한 열쇠
        // -----------------------------------------------------

        if (usedKeyTiles.Contains(
            position))
        {
            Debug.Log(
                "[Key] 이미 획득한 열쇠입니다: " +
                position
            );

            yield break;
        }


        // -----------------------------------------------------
        // Key Data
        // -----------------------------------------------------

        KeyDataLoader keyLoader =
            KeyDataLoader.Instance;


        if (keyLoader == null)
        {
            Debug.LogError(
                "[Key] KeyDataLoader를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        KeyTileData keyData =
            keyLoader.GetData(
                tile.X,
                tile.Y
            );


        if (keyData == null)
        {
            Debug.LogWarning(
                "[Key] Key 타일인데 " +
                "Key_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }


        // -----------------------------------------------------
        // KeyID -> ItemID
        // -----------------------------------------------------

        KeyRewardDatabase
            keyRewardDatabase =
                KeyRewardDatabase.Instance;


        if (keyRewardDatabase == null)
        {
            Debug.LogError(
                "[Key] KeyRewardDatabase를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        if (!keyRewardDatabase.TryGetItemID(
            keyData.keyID,
            out int itemID))
        {
            Debug.LogError(
                "[Key] KeyID에 대응하는 " +
                "ItemID가 없습니다.\n" +
                $"KeyID: {keyData.keyID}"
            );

            yield break;
        }


        // -----------------------------------------------------
        // ItemDatabase 검증
        // -----------------------------------------------------

        if (ItemDatabase.Instance == null)
        {
            Debug.LogError(
                "[Key] ItemDatabase를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        ItemData itemData =
            ItemDatabase.Instance.GetItem(
                itemID
            );


        if (itemData == null)
        {
            Debug.LogError(
                "[Key] ItemDatabase에 " +
                "열쇠 아이템이 없습니다.\n" +
                $"ItemID: {itemID}"
            );

            yield break;
        }


        // -----------------------------------------------------
        // Reward UI
        // -----------------------------------------------------

        DungeonRewardUI rewardUI =
            DungeonRewardUI.Instance;


        if (rewardUI == null)
        {
            Debug.LogError(
                "[Key] DungeonRewardUI를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        List<ChestItemData> rewards =
            new List<ChestItemData>();


        rewards.Add(
            new ChestItemData(
                itemID,
                1
            )
        );


        Debug.Log(
            "[Key] 열쇠 발견\n" +
            $"좌표: ({tile.X}, {tile.Y})\n" +
            $"KeyID: {keyData.keyID}\n" +
            $"ItemID: {itemID}\n" +
            $"ItemName: {itemData.itemName}"
        );


        // 기존 Chest/Farming 보상창 재사용
        yield return
            rewardUI.ShowChestRewards(
                rewards
            );


        // -----------------------------------------------------
        // 실제 획득 성공
        // -----------------------------------------------------

        if (rewardUI.AnyItemAcquired)
        {
            usedKeyTiles.Add(
                position
            );


            Debug.Log(
                "[Key] 열쇠 획득 완료\n" +
                $"좌표: {position}\n" +
                $"KeyID: {keyData.keyID}\n" +
                $"ItemID: {itemID}"
            );


            if (SaveManager.Instance != null)
            {
                SaveManager.Instance
                    .SaveGameplayData();
            }
        }
        else
        {
            Debug.Log(
                "[Key] 열쇠를 획득하지 않았습니다: " +
                position
            );
        }
    }


    // =========================================================
    // Chest
    // =========================================================

    private IEnumerator HandleChest(
        DungeonTileData tile)
    {
        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        if (usedChestTiles.Contains(
            position))
        {
            Debug.Log(
                "[Chest] 이미 사용한 상자입니다: " +
                position
            );

            yield break;
        }


        ChestDataLoader chestLoader =
            ChestDataLoader.Instance;


        if (chestLoader == null)
        {
            Debug.LogError(
                "[TileEvent] ChestDataLoader를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        ChestTileData chestData =
            chestLoader.GetData(
                tile.X,
                tile.Y
            );


        if (chestData == null)
        {
            Debug.LogWarning(
                "[TileEvent] Chest 타일인데 " +
                "Chest_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }


        if (DungeonRewardUI.Instance == null)
        {
            Debug.LogError(
                "[Chest] DungeonRewardUI를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        Debug.Log(
            "[Chest] 상자 열기\n" +
            $"좌표: ({tile.X}, {tile.Y})\n" +
            $"보상 종류: " +
            $"{chestData.items.Count}개"
        );


        yield return
            DungeonRewardUI.Instance
                .ShowChestRewards(
                    chestData.items
                );


        if (DungeonRewardUI.Instance
            .AnyItemAcquired)
        {
            usedChestTiles.Add(
                position
            );


            Debug.Log(
                "[Chest] 상자 사용 완료: " +
                position
            );


            if (SaveManager.Instance != null)
            {
                SaveManager.Instance
                    .SaveGameplayData();
            }
        }
        else
        {
            Debug.Log(
                "[Chest] 획득한 아이템 없음: " +
                position
            );
        }
    }


    // =========================================================
    // Puzzle / Letter
    // =========================================================

    private IEnumerator HandlePuzzleLetter(
        DungeonTileData tile)
    {
        PuzzleLetterUI puzzleUI =
            PuzzleLetterUI.Instance;


        if (puzzleUI == null)
        {
            Debug.LogError(
                "[TileEvent] PuzzleLetterUI를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        string message =
            "단서를 발견했습니다.\n\n" +
            $"좌표: ({tile.X}, {tile.Y})";


        Debug.Log(
            "[TileEvent] Puzzle/Letter 상호작용: " +
            $"({tile.X}, {tile.Y})"
        );


        yield return StartCoroutine(
            puzzleUI.ShowMessage(
                message
            )
        );
    }


    // =========================================================
    // Event / Hint
    // =========================================================

    private IEnumerator HandleEventHint(
        DungeonTileData tile)
    {
        Debug.Log(
            "[TileEvent] Event/Hint 상호작용 예정: " +
            $"({tile.X}, {tile.Y})"
        );


        yield break;
    }


    // =========================================================
    // Rest
    // =========================================================

    private IEnumerator HandleRest(
        DungeonTileData tile)
    {
        RestTileManager restManager =
            RestTileManager.Instance;


        if (restManager == null)
        {
            Debug.LogError(
                "[TileEvent] RestTileManager를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        if (!restManager.CanRest(
            position))
        {
            Debug.Log(
                "[TileEvent] 이 장소에서는 " +
                "이미 휴식했습니다.\n" +
                "던전을 나갔다 다시 들어와야 " +
                "다시 휴식할 수 있습니다."
            );

            yield break;
        }


        RestConfirmUI confirmUI =
            RestConfirmUI.Instance;


        if (confirmUI == null)
        {
            Debug.LogError(
                "[TileEvent] RestConfirmUI를 " +
                "찾을 수 없습니다."
            );

            yield break;
        }


        yield return StartCoroutine(
            confirmUI.ShowConfirm()
        );


        if (!confirmUI.GetResult())
        {
            Debug.Log(
                "[TileEvent] 휴식을 취소했습니다."
            );

            yield break;
        }


        bool success =
            restManager.Rest(
                position
            );


        if (success)
        {
            Debug.Log(
                "[TileEvent] Rest 완료: " +
                $"({tile.X}, {tile.Y})"
            );
        }
    }


    // =========================================================
    // Boss
    // =========================================================

    private IEnumerator HandleBoss(
        DungeonTileData tile)
    {
        Debug.Log(
            "[TileEvent] Boss 상호작용 예정: " +
            $"({tile.X}, {tile.Y})"
        );


        yield break;
    }


    // =========================================================
    // Chest Save
    // =========================================================

    public List<string>
        GetUsedChestTilesForSave()
    {
        List<string> result =
            new List<string>();


        foreach (
            Vector2Int position
            in usedChestTiles)
        {
            result.Add(
                $"{position.x},{position.y}"
            );
        }


        return result;
    }


    public void RestoreUsedChestTiles(
        List<string> savedTiles)
    {
        usedChestTiles.Clear();


        if (savedTiles == null)
            return;


        foreach (
            string entry
            in savedTiles)
        {
            if (string.IsNullOrWhiteSpace(
                entry))
            {
                continue;
            }


            string[] parts =
                entry.Split(',');


            if (parts.Length != 2)
                continue;


            if (!int.TryParse(
                parts[0],
                out int x))
            {
                continue;
            }


            if (!int.TryParse(
                parts[1],
                out int y))
            {
                continue;
            }


            usedChestTiles.Add(
                new Vector2Int(
                    x,
                    y
                )
            );
        }


        Debug.Log(
            "[Chest] 사용 완료 상자 복구: " +
            usedChestTiles.Count +
            "개"
        );
    }


    public void ClearUsedChestTiles()
    {
        usedChestTiles.Clear();
    }


    // =========================================================
    // Key Save
    // =========================================================

    public List<string>
        GetUsedKeyTilesForSave()
    {
        List<string> result =
            new List<string>();


        foreach (
            Vector2Int position
            in usedKeyTiles)
        {
            result.Add(
                $"{position.x},{position.y}"
            );
        }


        return result;
    }


    public void RestoreUsedKeyTiles(
        List<string> savedTiles)
    {
        usedKeyTiles.Clear();


        if (savedTiles == null)
            return;


        foreach (
            string entry
            in savedTiles)
        {
            if (string.IsNullOrWhiteSpace(
                entry))
            {
                continue;
            }


            string[] parts =
                entry.Split(',');


            if (parts.Length != 2)
                continue;


            if (!int.TryParse(
                parts[0],
                out int x))
            {
                continue;
            }


            if (!int.TryParse(
                parts[1],
                out int y))
            {
                continue;
            }


            usedKeyTiles.Add(
                new Vector2Int(
                    x,
                    y
                )
            );
        }


        Debug.Log(
            "[Key] 사용 완료 열쇠 타일 복구: " +
            usedKeyTiles.Count +
            "개"
        );
    }


    public void ClearUsedKeyTiles()
    {
        usedKeyTiles.Clear();

        Debug.Log(
            "[Key] 사용 기록 초기화"
        );
    }


    // =========================================================
    // Farming Reset
    // =========================================================

    public void ClearUsedFarmingTiles()
    {
        usedFarmingTiles.Clear();

        Debug.Log(
            "[Farming] 사용 기록 초기화"
        );
    }
}