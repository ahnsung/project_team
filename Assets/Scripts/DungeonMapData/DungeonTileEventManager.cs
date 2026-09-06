using System.Collections;
using UnityEngine;

public class DungeonTileEventManager : MonoBehaviour
{
    public static DungeonTileEventManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private BattleManager battleManager;

    [Header("General Tile")]
    [Range(0f, 100f)]
    [SerializeField] private float monsterBattleChance = 100f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (dungeonManager == null)
            dungeonManager = DungeonManager.Instance;

        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleManager>();
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
                "[DungeonTileEventManager] DungeonManager가 없습니다."
            );

            yield break;
        }

        DungeonTileData tile =
            dungeonManager.GetCurrentTile();

        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonTileEventManager] 현재 타일 데이터가 없습니다."
            );

            yield break;
        }

        Debug.Log(
            $"[DungeonTileEventManager] 타일 진입: " +
            $"({tile.X}, {tile.Y}) / {tile.TileType}"
        );

        switch (tile.TileType)
        {
            /*
             * 기획서 기준:
             * General에서만 전투가 발생할 수 있다.
             */
            case DungeonTileType.General:
                yield return HandleGeneralEnter(tile);
                break;

            /*
             * 함정은 상호작용 없이
             * 타일 진입 즉시 자동 판정.
             */
            case DungeonTileType.Trap:
                yield return HandleTrapEnter(tile);
                break;

            /*
             * 위치 변이도 타일 진입 즉시 자동 발동.
             * 실제 이동은 Teleport 데이터 완성 후 구현.
             */
            case DungeonTileType.Teleport:
                yield return HandleTeleportEnter(tile);
                break;

            /*
             * 나머지는 E 상호작용 타입이므로
             * 방 진입 시 아무것도 하지 않는다.
             */
            default:
                break;
        }
    }

    // =========================================================
    // E 키 상호작용 이벤트
    // =========================================================

    public IEnumerator ExecuteInteraction()
    {
        ResolveReferences();

        if (dungeonManager == null)
        {
            Debug.LogError(
                "[DungeonTileEventManager] DungeonManager가 없습니다."
            );

            yield break;
        }

        DungeonTileData tile =
            dungeonManager.GetCurrentTile();

        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonTileEventManager] 현재 타일 데이터가 없습니다."
            );

            yield break;
        }

        Debug.Log(
            $"[DungeonTileEventManager] E 상호작용: " +
            $"({tile.X}, {tile.Y}) / {tile.TileType}"
        );

        switch (tile.TileType)
        {
            case DungeonTileType.Farming:
                yield return HandleFarming(tile);
                break;

            case DungeonTileType.Key:
                yield return HandleKey(tile);
                break;

            case DungeonTileType.Chest:
                yield return HandleChest(tile);
                break;

            case DungeonTileType.PuzzleLetter:
                yield return HandlePuzzleLetter(tile);
                break;

            case DungeonTileType.EventHint:
                yield return HandleEventHint(tile);
                break;

            case DungeonTileType.Rest:
                yield return HandleRest(tile);
                break;

            case DungeonTileType.Boss:
                yield return HandleBoss(tile);
                break;

            default:
                Debug.Log(
                    $"[TileEvent] 현재 타일은 E 상호작용 대상이 아닙니다: " +
                    $"{tile.TileType}"
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
        Debug.Log(
            $"[TileEvent] General 진입: ({tile.X}, {tile.Y})"
        );

        bool battleOccurs =
            Random.Range(0f, 100f) <
            monsterBattleChance;

        if (!battleOccurs)
        {
            Debug.Log(
                "[TileEvent] General 전투가 발생하지 않았습니다."
            );

            yield break;
        }

        if (battleManager == null)
        {
            Debug.LogWarning(
                "[TileEvent] 전투가 발생했지만 BattleManager가 없습니다."
            );

            yield break;
        }

        Debug.Log(
            "[TileEvent] General 전투 발생"
        );

        yield return StartCoroutine(
            battleManager.StartBattleEncounter()
        );

        while (battleManager.IsBattleRunning())
            yield return null;
    }

    // =========================================================
    // Farming
    // =========================================================

    private IEnumerator HandleFarming(
        DungeonTileData tile)
    {
        if (FarmingDataLoader.Instance == null)
        {
            Debug.LogError(
                "[TileEvent] FarmingDataLoader를 찾을 수 없습니다."
            );

            yield break;
        }

        FarmingTileData farmingData =
            FarmingDataLoader.Instance.GetData(
                tile.X,
                tile.Y
            );

        if (farmingData == null)
        {
            Debug.LogWarning(
                "[TileEvent] Farming 타일인데 Farming_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }

        /*
         * Min~Max는 양 끝을 포함한 균등 랜덤.
         *
         * 예:
         * 2~6이면
         * 2,3,4,5,6 각각 동일 확률.
         */
        int amount =
            Random.Range(
                farmingData.minItemQuantity,
                farmingData.maxItemQuantity + 1
            );

        Debug.Log(
            "[TileEvent] Farming 데이터 확인 완료\n" +
            $"좌표: ({farmingData.x}, {farmingData.y})\n" +
            $"ItemGroup: {farmingData.itemGroup}\n" +
            $"범위: {farmingData.minItemQuantity}" +
            $"~{farmingData.maxItemQuantity}\n" +
            $"이번 파밍 추첨 수량: {amount}"
        );

        /*
         * ItemGroup의 실제 아이템 목록이 들어오면
         * 여기서 획득 후보 UI를 생성한다.
         */

        yield break;
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
                "[TileEvent] TrapDataLoader를 찾을 수 없습니다."
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
                "[TileEvent] Trap 타일인데 Trap_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }

        /*
         * 0 이상 100 미만의 실수를 뽑는다.
         *
         * 예:
         * TrapPossibility = 70
         * → 70% 확률로 함정 발동.
         */
        float roll =
            Random.Range(0f, 100f);

        bool triggered =
            roll < trapData.trapPossibility;

        Debug.Log(
            "[Trap] 함정 판정\n" +
            $"좌표: ({trapData.x}, {trapData.y})\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"발동 확률: {trapData.trapPossibility}%\n" +
            $"주사위 값: {roll:F2}"
        );

        // =====================================================
        // 함정 회피
        // =====================================================

        if (!triggered)
        {
            Debug.Log(
                "[Trap] 함정을 회피했습니다."
            );

            /*
             * 나중에 알림 UI를 붙이면
             * 여기서:
             *
             * "함정을 회피했습니다."
             *
             * 메시지를 출력하면 된다.
             */

            yield break;
        }

        // =====================================================
        // 함정 발동
        // =====================================================

        Debug.Log(
            "[Trap] 함정 발동!\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"지속 턴: {trapData.trapAmount}"
        );

        /*
         * TrapType 1~6의 실제 상태이상 효과는
         * 아직 기획 데이터가 확정되지 않았으므로
         * 여기서는 적용하지 않는다.
         *
         * 추후:
         *
         * ApplyTrapStatusEffect(
         *     trapData.trapType,
         *     trapData.trapAmount
         * );
         *
         * 같은 식으로 연결하면 된다.
         */

        yield break;
    }

    // =========================================================
    // Teleport
    // =========================================================

    private IEnumerator HandleTeleportEnter(
        DungeonTileData tile)
    {
        /*
         * Teleport 전체 연결 데이터가 완성되면
         * 여기서 반대 좌표로 이동한다.
         */

        Debug.Log(
            $"[TileEvent] Teleport 자동 이벤트: " +
            $"({tile.X}, {tile.Y})"
        );

        yield break;
    }

    // =========================================================
    // Key
    // =========================================================

    private IEnumerator HandleKey(
        DungeonTileData tile)
    {
        Debug.Log(
            $"[TileEvent] Key 상호작용 예정: " +
            $"({tile.X}, {tile.Y})"
        );

        yield break;
    }

    // =========================================================
    // Chest
    // =========================================================

    private IEnumerator HandleChest(
        DungeonTileData tile)
    {
        ChestDataLoader chestLoader =
            ChestDataLoader.Instance;

        if (chestLoader == null)
        {
            Debug.LogError(
                "[TileEvent] ChestDataLoader를 찾을 수 없습니다."
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
                "[TileEvent] Chest 타일인데 Chest_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }

        Debug.Log(
            $"[Chest] 상자 데이터 확인 완료\n" +
            $"좌표: ({chestData.x}, {chestData.y})\n" +
            $"보상 종류: {chestData.items.Count}개"
        );

        for (int i = 0; i < chestData.items.Count; i++)
        {
            ChestItemData item =
                chestData.items[i];

            Debug.Log(
                $"[Chest] 보상 {i + 1}\n" +
                $"ItemID: {item.itemID}\n" +
                $"수량: {item.amount}"
            );
        }

        /*
         * 다음 단계에서:
         *
         * 1. 상자 보상 UI 표시
         * 2. 각 아이템 V / X 선택
         * 3. 실제 Inventory에 추가
         * 4. 하나라도 획득했다면 상자 사용 완료 처리
         *
         * 를 붙인다.
         */

        yield break;
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
                "[TileEvent] PuzzleLetterUI를 찾을 수 없습니다."
            );

            yield break;
        }

        string message =
            $"단서를 발견했습니다.\n\n" +
            $"좌표: ({tile.X}, {tile.Y})";

        Debug.Log(
            $"[TileEvent] Puzzle/Letter 상호작용: " +
            $"({tile.X}, {tile.Y})"
        );

        yield return StartCoroutine(
            puzzleUI.ShowMessage(message)
        );
    }

    // =========================================================
    // Event / Hint
    // =========================================================

    private IEnumerator HandleEventHint(
        DungeonTileData tile)
    {
        Debug.Log(
            $"[TileEvent] Event/Hint 상호작용 예정: " +
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
                "[TileEvent] RestTileManager를 찾을 수 없습니다."
            );

            yield break;
        }

        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );

        // 이미 사용한 휴식 장소
        if (!restManager.CanRest(position))
        {
            Debug.Log(
                "[TileEvent] 이 장소에서는 이미 휴식했습니다.\n" +
                "던전을 나갔다 다시 들어와야 다시 휴식할 수 있습니다."
            );

            yield break;
        }

        RestConfirmUI confirmUI =
            RestConfirmUI.Instance;

        if (confirmUI == null)
        {
            Debug.LogError(
                "[TileEvent] RestConfirmUI를 찾을 수 없습니다."
            );

            yield break;
        }

        // 확인창 표시 후 선택을 기다림
        yield return StartCoroutine(
            confirmUI.ShowConfirm()
        );

        // 아니요
        if (!confirmUI.GetResult())
        {
            Debug.Log(
                "[TileEvent] 휴식을 취소했습니다."
            );

            yield break;
        }

        // 네
        bool success =
            restManager.Rest(position);

        if (success)
        {
            Debug.Log(
                $"[TileEvent] Rest 완료: ({tile.X}, {tile.Y})"
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
            $"[TileEvent] Boss 상호작용 예정: " +
            $"({tile.X}, {tile.Y})"
        );

        yield break;
    }
}