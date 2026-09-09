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
    // Teleport
    // =========================================================

    [Header("Teleport")]

    [Tooltip(
        "Teleport 방에 진입한 뒤 실제 이동하기 전까지 기다리는 시간입니다."
    )]
    [Min(0f)]
    [SerializeField]
    private float teleportDelay = 1f;

    private bool teleportRunning = false;


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
        TrapDataLoader trapLoader =
            TrapDataLoader.Instance;

        if (trapLoader == null)
        {
            Debug.LogError(
                "[Trap] TrapDataLoader를 찾을 수 없습니다."
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
                "[Trap] Trap 데이터가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }

        int possibility =
            Mathf.Clamp(
                trapData.trapPossibility,
                0,
                100
            );

        int roll =
            Random.Range(
                0,
                100
            );

        bool triggered =
            roll < possibility;

        Debug.Log(
            "[Trap] 함정 판정\n" +
            $"좌표: ({trapData.x}, {trapData.y})\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"발동 확률: {possibility}%\n" +
            $"Roll: {roll}\n" +
            $"Amount: {trapData.trapAmount}"
        );

        if (!triggered)
        {
            Debug.Log(
                "[Trap] 함정을 피했습니다."
            );

            yield break;
        }

        Debug.Log(
            "[Trap] 함정 발동!\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"Amount: {trapData.trapAmount}"
        );

        ApplyTrapEffect(
            trapData
        );

        yield break;
    }


    // =========================================================
    // Trap Status Effect
    // =========================================================

    private void ApplyTrapStatusEffect(
        TrapTileData trapData)
    {
        if (trapData == null)
        {
            return;
        }

        StatusEffectData statusData =
            TrapStatusEffectFactory.Create(
                trapData.trapType,
                trapData.trapAmount
            );

        if (statusData == null)
        {
            Debug.LogWarning(
                "[Trap] 아직 실제 효과가 연결되지 않은 함정입니다.\n" +
                $"TrapType: {trapData.trapType}"
            );

            return;
        }

        StatusEffectController controller =
            null;

        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null
        )
        {
            controller =
                BattleManager.Instance
                    .playerUnit
                    .GetComponent<
                        StatusEffectController
                    >();

            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInParent<
                            StatusEffectController
                        >();
            }

            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInChildren<
                            StatusEffectController
                        >();
            }
        }

        if (controller == null)
        {
            controller =
                FindFirstObjectByType<
                    StatusEffectController
                >();
        }

        if (controller == null)
        {
            Debug.LogError(
                "[Trap] 플레이어의 " +
                "StatusEffectController를 찾을 수 없습니다."
            );

            return;
        }

        bool added =
            controller.AddStatusEffect(
                statusData
            );

        if (!added)
        {
            Debug.LogWarning(
                "[Trap] 상태이상 적용 실패\n" +
                $"TrapType: {trapData.trapType}"
            );

            return;
        }

        Debug.Log(
            "[Trap] 상태이상 적용 완료\n" +
            $"이름: {statusData.buffName}\n" +
            $"EffectType: {statusData.effectType}\n" +
            $"EffectPower: {statusData.effectPower}\n" +
            $"지속시간: {statusData.buffDuration}"
        );
    }


    // =========================================================
    // Trap Effect
    // =========================================================

    private void ApplyTrapEffect(
        TrapTileData trapData)
    {
        if (trapData == null)
        {
            Debug.LogWarning(
                "[Trap] TrapTileData가 null입니다."
            );

            return;
        }

        Debug.Log(
            "[Trap] 효과 적용 시도\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"Amount: {trapData.trapAmount}"
        );

        ApplyTrapStatusEffect(
            trapData
        );
    }


    // =========================================================
    // TELEPORT
    // =========================================================

    private IEnumerator HandleTeleportEnter(
        DungeonTileData tile)
    {
        ResolveReferences();

        // -----------------------------------------------------
        // 중복 텔레포트 실행 방지
        // -----------------------------------------------------

        if (teleportRunning)
        {
            Debug.Log(
                "[Teleport] 이미 텔레포트가 진행 중입니다."
            );

            yield break;
        }


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


        teleportRunning = true;


        Debug.Log(
            "[Teleport] 텔레포트 방 진입\n" +
            $"ID: {teleportData.connectedTeleportID}\n" +
            $"출발: {currentPosition}\n" +
            $"도착 예정: {destination}\n" +
            $"대기 시간: {teleportDelay:F1}초"
        );


        // =====================================================
        // 중요
        //
        // 방에 먼저 도착한 상태이므로
        // RoomTileVisualController가 Teleport 오브젝트를
        // 이미 활성화한 상태다.
        //
        // 이 상태를 1초간 보여준 뒤 실제 텔레포트를 실행한다.
        // =====================================================

        if (teleportDelay > 0f)
        {
            yield return
                new WaitForSeconds(
                    teleportDelay
                );
        }


        /*
         * 1초 사이에 다른 코드가 플레이어 위치를
         * 바꿨다면 텔레포트를 취소한다.
         *
         * 정상 플레이에서는 거의 발생하지 않지만
         * 디버그/씬 전환/다른 이벤트와 충돌하는 것을 방지한다.
         */
        if (
            dungeonManager.CurrentRoom !=
            currentPosition
        )
        {
            Debug.LogWarning(
                "[Teleport] 대기 중 현재 위치가 변경되어 " +
                "텔레포트를 취소합니다.\n" +
                $"원래 위치: {currentPosition}\n" +
                $"현재 위치: {dungeonManager.CurrentRoom}"
            );

            teleportRunning = false;

            yield break;
        }


        Debug.Log(
            "[Teleport] 위치 변이기 발동\n" +
            $"ID: {teleportData.connectedTeleportID}\n" +
            $"출발: {currentPosition}\n" +
            $"도착: {destination}"
        );


        // =====================================================
        // Fade Out
        // =====================================================

        if (fadeController != null)
        {
            yield return
                fadeController.FadeOut();
        }


        // =====================================================
        // 실제 텔레포트
        // =====================================================

        bool moved =
            dungeonManager.TeleportToRoom(
                destination
            );


        if (!moved)
        {
            Debug.LogError(
                "[Teleport] 텔레포트 이동 실패\n" +
                $"출발: {currentPosition}\n" +
                $"목적지: {destination}"
            );


            if (fadeController != null)
            {
                yield return
                    fadeController.FadeIn();
            }


            teleportRunning = false;

            yield break;
        }


        /*
         * DungeonManager가 위치 변경 후
         * 방 비주얼 / 미니맵 등을 갱신할 시간을
         * 한 프레임 준다.
         */
        yield return null;


        // =====================================================
        // Fade In
        // =====================================================

        if (fadeController != null)
        {
            yield return
                fadeController.FadeIn();
        }


        /*
         * 매우 중요:
         *
         * 여기서 ExecuteEnterEvent()를 다시 호출하지 않는다.
         *
         * Teleport 목적지도 Teleport 타일이므로
         * 호출하면
         *
         * A -> B -> 1초 -> A -> B ...
         *
         * 무한 왕복이 발생할 수 있다.
         *
         * TeleportToRoom()의 방 갱신만 사용하고
         * 도착지 EnterEvent는 의도적으로 실행하지 않는다.
         */


        Debug.Log(
            "[Teleport] 이동 완료\n" +
            $"출발: {currentPosition}\n" +
            $"도착: {destination}\n" +
            $"현재 위치: {dungeonManager.CurrentRoom}"
        );


        teleportRunning = false;
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


    // =========================================================
    // Farming Visual State
    // =========================================================

    public bool IsFarmingUsed(
        Vector2Int position)
    {
        return
            usedFarmingTiles.Contains(
                position
            );
    }


    // =========================================================
    // Visual State Query
    // =========================================================

    public bool IsFarmingTileUsed(
        Vector2Int position)
    {
        return
            usedFarmingTiles.Contains(
                position
            );
    }


    public bool IsChestTileUsed(
        Vector2Int position)
    {
        return
            usedChestTiles.Contains(
                position
            );
    }


    public bool IsKeyTileUsed(
        Vector2Int position)
    {
        return
            usedKeyTiles.Contains(
                position
            );
    }
}