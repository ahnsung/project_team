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
    // Used Tiles
    // =========================================================

    private readonly HashSet<Vector2Int>
        usedChestTiles =
            new HashSet<Vector2Int>();


    private readonly HashSet<Vector2Int>
        usedKeyTiles =
            new HashSet<Vector2Int>();


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

    [SerializeField]
    private FadeController fadeController;


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
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


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
                FindFirstObjectByType<
                    BattleManager
                >();
        }


        if (fadeController == null)
        {
            fadeController =
                FindFirstObjectByType<
                    FadeController
                >();
        }
    }


    // =========================================================
    // Enter Event
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
            case DungeonTileType.General:

                yield return
                    HandleGeneralEnter(
                        tile
                    );

                break;


            case DungeonTileType.Trap:

                yield return
                    HandleTrapEnter(
                        tile
                    );

                break;


            case DungeonTileType.Teleport:

                yield return
                    HandleTeleportEnter(
                        tile
                    );

                break;


            default:

                break;
        }
    }


    // =========================================================
    // E Interaction
    // =========================================================

    public IEnumerator ExecuteInteraction()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            yield break;
        }


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        if (tile == null)
        {
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
                    HandleFarming(
                        tile
                    );

                break;


            case DungeonTileType.Key:

                yield return
                    HandleKey(
                        tile
                    );

                break;


            case DungeonTileType.Chest:

                yield return
                    HandleChest(
                        tile
                    );

                break;


            case DungeonTileType.PuzzleLetter:

                yield return
                    HandlePuzzleLetter(
                        tile
                    );

                break;


            case DungeonTileType.EventHint:

                yield return
                    HandleEventHint(
                        tile
                    );

                break;


            case DungeonTileType.Rest:

                yield return
                    HandleRest(
                        tile
                    );

                break;


            case DungeonTileType.Boss:

                yield return
                    HandleBoss(
                        tile
                    );

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
    // Can Interact
    // =========================================================

    public bool CanInteractCurrentTile()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            return false;
        }


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        if (tile == null)
        {
            return false;
        }


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


        if (battleManager == null)
        {
            Debug.LogWarning(
                "[General] 전투 판정 성공했지만 " +
                "BattleManager가 없습니다."
            );

            yield break;
        }


        Debug.Log(
            "[General] 전투 발생!\n" +
            $"확률: " +
            $"{currentGeneralBattleChance:F0}%"
        );


        ResetGeneralBattleChance();


        yield return StartCoroutine(
            battleManager
                .StartBattleEncounter()
        );


        while (
            battleManager.IsBattleRunning()
        )
        {
            yield return null;
        }


        Debug.Log(
            "[General] 전투 종료"
        );
    }


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


        if (
            usedFarmingTiles.Contains(
                position
            )
        )
        {
            Debug.Log(
                "[Farming] 이미 파밍한 장소입니다."
            );

            yield break;
        }


        FarmingDataLoader loader =
            FarmingDataLoader.Instance;


        if (loader == null)
        {
            Debug.LogError(
                "[Farming] FarmingDataLoader가 없습니다."
            );

            yield break;
        }


        FarmingTileData data =
            loader.GetData(
                tile.X,
                tile.Y
            );


        if (data == null)
        {
            Debug.LogWarning(
                "[Farming] Farming_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }


        FarmingItemGroupDatabase
            groupDatabase =
                FarmingItemGroupDatabase.Instance;


        if (groupDatabase == null)
        {
            yield break;
        }


        List<int> itemIDs =
            groupDatabase.GetItemIDs(
                data.itemGroup
            );


        if (
            itemIDs == null ||
            itemIDs.Count == 0
        )
        {
            yield break;
        }


        int amount =
            Random.Range(
                data.minItemQuantity,
                data.maxItemQuantity + 1
            );


        amount =
            Mathf.Clamp(
                amount,
                1,
                6
            );


        List<ChestItemData> rewards =
            new List<ChestItemData>();


        for (
            int i = 0;
            i < amount;
            i++
        )
        {
            int itemID =
                itemIDs[
                    Random.Range(
                        0,
                        itemIDs.Count
                    )
                ];


            if (itemID <= 0)
            {
                continue;
            }


            rewards.Add(
                new ChestItemData(
                    itemID,
                    1
                )
            );
        }


        if (rewards.Count == 0)
        {
            yield break;
        }


        DungeonRewardUI rewardUI =
            DungeonRewardUI.Instance;


        if (rewardUI == null)
        {
            yield break;
        }


        yield return
            rewardUI.ShowChestRewards(
                rewards
            );


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
    }


    // =========================================================
    // Trap
    // =========================================================

    private IEnumerator HandleTrapEnter(
        DungeonTileData tile)
    {
        TrapDataLoader loader =
            TrapDataLoader.Instance;


        if (loader == null)
        {
            Debug.LogError(
                "[Trap] TrapDataLoader가 없습니다."
            );

            yield break;
        }


        TrapTileData data =
            loader.GetData(
                tile.X,
                tile.Y
            );


        if (data == null)
        {
            Debug.LogWarning(
                "[Trap] Trap_Data가 없습니다.\n" +
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
            data.trapPossibility;


        Debug.Log(
            "[Trap] 함정 판정\n" +
            $"좌표: ({tile.X}, {tile.Y})\n" +
            $"TrapType: {data.trapType}\n" +
            $"확률: {data.trapPossibility}%\n" +
            $"Roll: {roll:F2}"
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
            $"TrapType: {data.trapType}\n" +
            $"지속 턴: {data.trapAmount}"
        );


        yield break;
    }


    // =========================================================
    // TELEPORT
    // =========================================================

    private IEnumerator HandleTeleportEnter(
        DungeonTileData tile)
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            Debug.LogError(
                "[Teleport] DungeonManager가 없습니다."
            );

            yield break;
        }


        TeleportDataLoader loader =
            TeleportDataLoader.Instance;


        if (loader == null)
        {
            Debug.LogError(
                "[Teleport] " +
                "TeleportDataLoader가 없습니다."
            );

            yield break;
        }


        Vector2Int currentPosition =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        TeleportTileData teleportData =
            loader.GetData(
                currentPosition
            );


        if (teleportData == null)
        {
            Debug.LogWarning(
                "[Teleport] Teleport 타일인데 " +
                "Teleport_Data가 없습니다.\n" +
                $"좌표: {currentPosition}"
            );

            yield break;
        }


        if (
            !loader.TryGetDestination(
                currentPosition,
                out Vector2Int destination
            )
        )
        {
            Debug.LogError(
                "[Teleport] 연결된 목적지를 찾지 못했습니다.\n" +
                $"현재 위치: {currentPosition}\n" +
                $"ID: {teleportData.connectedTeleportID}"
            );

            yield break;
        }


        Debug.Log(
            "[Teleport] 위치 변이기 발동\n" +
            $"ID: {teleportData.connectedTeleportID}\n" +
            $"출발: {currentPosition}\n" +
            $"도착: {destination}"
        );


        // Fade Out
        if (fadeController != null)
        {
            yield return
                fadeController.FadeOut();
        }


        bool moved =
            dungeonManager.TeleportToRoom(
                destination
            );


        if (!moved)
        {
            Debug.LogError(
                "[Teleport] 텔레포트 이동 실패"
            );


            if (fadeController != null)
            {
                yield return
                    fadeController.FadeIn();
            }


            yield break;
        }


        yield return null;


        // Fade In
        if (fadeController != null)
        {
            yield return
                fadeController.FadeIn();
        }


        /*
         * 여기서 ExecuteEnterEvent()를
         * 다시 호출하지 않는다.
         *
         * 도착지 역시 Teleport 타일이라
         * 재호출하면
         *
         * A -> B -> A -> B...
         *
         * 무한 왕복하기 때문.
         */


        Debug.Log(
            "[Teleport] 이동 완료\n" +
            $"현재 위치: " +
            $"{dungeonManager.CurrentRoom}"
        );
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


        if (
            usedKeyTiles.Contains(
                position
            )
        )
        {
            Debug.Log(
                "[Key] 이미 획득한 열쇠입니다: " +
                position
            );

            yield break;
        }


        KeyDataLoader loader =
            KeyDataLoader.Instance;


        if (loader == null)
        {
            yield break;
        }


        KeyTileData keyData =
            loader.GetData(
                tile.X,
                tile.Y
            );


        if (keyData == null)
        {
            yield break;
        }


        KeyRewardDatabase
            rewardDatabase =
                KeyRewardDatabase.Instance;


        if (rewardDatabase == null)
        {
            yield break;
        }


        if (
            !rewardDatabase.TryGetItemID(
                keyData.keyID,
                out int itemID
            )
        )
        {
            yield break;
        }


        if (ItemDatabase.Instance == null)
        {
            yield break;
        }


        ItemData itemData =
            ItemDatabase.Instance.GetItem(
                itemID
            );


        if (itemData == null)
        {
            yield break;
        }


        DungeonRewardUI rewardUI =
            DungeonRewardUI.Instance;


        if (rewardUI == null)
        {
            yield break;
        }


        List<ChestItemData> rewards =
            new List<ChestItemData>
            {
                new ChestItemData(
                    itemID,
                    1
                )
            };


        yield return
            rewardUI.ShowChestRewards(
                rewards
            );


        if (rewardUI.AnyItemAcquired)
        {
            usedKeyTiles.Add(
                position
            );


            Debug.Log(
                "[Key] 열쇠 획득 완료\n" +
                $"KeyID: {keyData.keyID}\n" +
                $"ItemID: {itemID}"
            );


            if (SaveManager.Instance != null)
            {
                SaveManager.Instance
                    .SaveGameplayData();
            }
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


        if (
            usedChestTiles.Contains(
                position
            )
        )
        {
            Debug.Log(
                "[Chest] 이미 사용한 상자입니다: " +
                position
            );

            yield break;
        }


        ChestDataLoader loader =
            ChestDataLoader.Instance;


        if (loader == null)
        {
            yield break;
        }


        ChestTileData data =
            loader.GetData(
                tile.X,
                tile.Y
            );


        if (data == null)
        {
            yield break;
        }


        DungeonRewardUI rewardUI =
            DungeonRewardUI.Instance;


        if (rewardUI == null)
        {
            yield break;
        }


        yield return
            rewardUI.ShowChestRewards(
                data.items
            );


        if (rewardUI.AnyItemAcquired)
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
            yield break;
        }


        string message =
            "단서를 발견했습니다.\n\n" +
            $"좌표: ({tile.X}, {tile.Y})";


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
            "[TileEvent] Event/Hint 예정: " +
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
            yield break;
        }


        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        if (
            !restManager.CanRest(
                position
            )
        )
        {
            Debug.Log(
                "[Rest] 이미 휴식한 장소입니다."
            );

            yield break;
        }


        RestConfirmUI confirmUI =
            RestConfirmUI.Instance;


        if (confirmUI == null)
        {
            yield break;
        }


        yield return StartCoroutine(
            confirmUI.ShowConfirm()
        );


        if (!confirmUI.GetResult())
        {
            yield break;
        }


        restManager.Rest(
            position
        );
    }


    // =========================================================
    // Boss
    // =========================================================

    private IEnumerator HandleBoss(
        DungeonTileData tile)
    {
        Debug.Log(
            "[Boss] 구현 예정: " +
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
            in usedChestTiles
        )
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


        RestorePositionList(
            savedTiles,
            usedChestTiles
        );


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
            in usedKeyTiles
        )
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


        RestorePositionList(
            savedTiles,
            usedKeyTiles
        );


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
    // Farming
    // =========================================================

    public void ClearUsedFarmingTiles()
    {
        usedFarmingTiles.Clear();


        Debug.Log(
            "[Farming] 사용 기록 초기화"
        );
    }


    // =========================================================
    // Save Parse Helper
    // =========================================================

    private void RestorePositionList(
        List<string> savedTiles,
        HashSet<Vector2Int> target)
    {
        if (
            savedTiles == null ||
            target == null
        )
        {
            return;
        }


        foreach (
            string entry
            in savedTiles
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    entry
                )
            )
            {
                continue;
            }


            string[] parts =
                entry.Split(',');


            if (parts.Length != 2)
            {
                continue;
            }


            if (
                !int.TryParse(
                    parts[0],
                    out int x
                )
            )
            {
                continue;
            }


            if (
                !int.TryParse(
                    parts[1],
                    out int y
                )
            )
            {
                continue;
            }


            target.Add(
                new Vector2Int(
                    x,
                    y
                )
            );
        }
    }
}