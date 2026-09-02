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
        /*
         * TrapType의 실제 효과가 아직 확정되지 않았으므로
         * 지금은 자동 이벤트 진입점만 구축한다.
         */

        Debug.Log(
            $"[TileEvent] Trap 자동 이벤트: " +
            $"({tile.X}, {tile.Y})"
        );

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
        Debug.Log(
            $"[TileEvent] Chest 상호작용 예정: " +
            $"({tile.X}, {tile.Y})"
        );

        yield break;
    }

    // =========================================================
    // Puzzle / Letter
    // =========================================================

    private IEnumerator HandlePuzzleLetter(
        DungeonTileData tile)
    {
        Debug.Log(
            $"[TileEvent] Puzzle/Letter 상호작용 예정: " +
            $"({tile.X}, {tile.Y})"
        );

        yield break;
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
        Debug.Log(
            $"[TileEvent] Rest 상호작용 예정: " +
            $"({tile.X}, {tile.Y})"
        );

        yield break;
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