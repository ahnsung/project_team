using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string GameplaySaveKey = "GameplaySaveData";
    private const string DungeonSceneName = "DungeonScene";

    // 기본 세이브
    private const string HasSaveKey = "HasSave";
    private const string CutscenePlayedKey = "CutscenePlayed";
    private const string SelectedCharacterKey = "SelectedCharacter";
    private const string PlayerNameKey = "PlayerName";
    private const string AfterCutsceneKey =
        "AfterCutsceneGoToCharacterSelect";

    // 던전 세이브
    private const string DungeonRoomXKey = "ROOM_X";
    private const string DungeonRoomYKey = "ROOM_Y";
    private const string DungeonVisitedKey = "VISITED";
    private const string DungeonTurnKey = "DUNGEON_TURN";
    private const string DungeonEnvironmentKey =
        "DUNGEON_ENVIRONMENT";

    // 플레이어 리소스 세이브
    private const string PlayerHealthKey = "PLAYER_HEALTH";
    private const string PlayerMentalKey = "PLAYER_MENTAL";
    private const string PlayerHungerKey = "PLAYER_HUNGER";
    private const string PlayerLastProcessedTurnKey =
        "PLAYER_LAST_PROCESSED_TURN";

    [Header("Debug")]
    [SerializeField]
    private bool printSaveLog = true;

    private bool isLoading;
    private bool isDeletingSave;

    public bool IsLoading => isLoading;

    [Serializable]
    public class InventoryItemSaveData
    {
        public string uniqueId;

        public int itemId;

        public int positionX;
        public int positionY;

        public int rotation;
        public int remainUseCount;
        public int currentDurability;

        public bool isEquipped;
        public EquipmentSlotType equippedSlot;
    }

    [Serializable]
    public class GameplaySaveData
    {
        public List<InventoryItemSaveData> items =
            new List<InventoryItemSaveData>();
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        Instance = null;
    }

    private void OnApplicationQuit()
    {
        if (isDeletingSave)
            return;

        if (!HasSave())
            return;

        SaveGameplayData();
    }

    private void OnApplicationPause(
        bool pauseStatus)
    {
        if (!pauseStatus)
            return;

        if (isDeletingSave)
            return;

        if (!HasSave())
            return;

        SaveGameplayData();
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (isDeletingSave)
            return;

        /*
         * 로비나 메뉴에서는 인벤토리 매니저가 없기 때문에
         * 던전 씬에서만 인벤토리와 장비를 복구한다.
         */
        if (scene.name != DungeonSceneName)
            return;

        if (!HasSave())
            return;

        if (!HasGameplaySave())
            return;

        StartCoroutine(
            LoadGameplayDataAfterSceneReady()
        );
    }

    private IEnumerator
        LoadGameplayDataAfterSceneReady()
    {
        /*
         * DungeonScene의 각 매니저가 Awake와 Start를
         * 완료할 시간을 준다.
         */
        yield return null;
        yield return null;

        const float timeout = 5f;

        float elapsed = 0f;

        while (elapsed < timeout)
        {
            bool ready =
                InventoryManager.Instance != null &&
                EquipmentManager.Instance != null &&
                ItemDatabase.Instance != null;

            if (ready)
            {
                LoadGameplayData();
                yield break;
            }

            elapsed +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        Debug.LogError(
            "[SaveManager] 저장 데이터를 불러올 필수 매니저를 " +
            "찾지 못했습니다.\n" +
            $"InventoryManager: " +
            $"{(InventoryManager.Instance != null ? "있음" : "없음")}\n" +
            $"EquipmentManager: " +
            $"{(EquipmentManager.Instance != null ? "있음" : "없음")}\n" +
            $"ItemDatabase: " +
            $"{(ItemDatabase.Instance != null ? "있음" : "없음")}"
        );
    }

    // =========================================================
    // 기본 세이브
    // =========================================================

    public void CreateNewSave(
        int characterID,
        string playerName)
    {
        /*
         * CharacterSelectScene을 직접 실행했거나,
         * MenuManager에서 삭제 호출이 누락된 경우까지 대비한다.
         */
        DeleteSave();

        PlayerPrefs.SetInt(
            HasSaveKey,
            1
        );

        PlayerPrefs.SetInt(
            CutscenePlayedKey,
            1
        );

        PlayerPrefs.SetInt(
            SelectedCharacterKey,
            characterID
        );

        PlayerPrefs.SetString(
            PlayerNameKey,
            playerName ?? string.Empty
        );

        PlayerPrefs.Save();

        isDeletingSave = false;

        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 새 게임 저장 생성 완료\n" +
                $"캐릭터 ID: {characterID}\n" +
                $"플레이어 이름: {playerName}"
            );
        }
    }

    public bool HasSave()
    {
        return PlayerPrefs.GetInt(
            HasSaveKey,
            0
        ) == 1;
    }

    public bool HasPlayedCutscene()
    {
        return PlayerPrefs.GetInt(
            CutscenePlayedKey,
            0
        ) == 1;
    }

    public int GetSelectedCharacter()
    {
        return PlayerPrefs.GetInt(
            SelectedCharacterKey,
            -1
        );
    }

    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(
            PlayerNameKey,
            string.Empty
        );
    }

    // =========================================================
    // 인벤토리 및 장비 저장
    // =========================================================

    public bool HasGameplaySave()
    {
        return PlayerPrefs.HasKey(
            GameplaySaveKey
        );
    }

    public void SaveGameplayData()
    {
        if (isLoading ||
            isDeletingSave ||
            !HasSave())
        {
            return;
        }

        InventoryManager inventory =
            InventoryManager.Instance;

        EquipmentManager equipment =
            EquipmentManager.Instance;

        if (inventory == null ||
            equipment == null)
        {
            Debug.LogWarning(
                "[SaveManager] 인벤토리 또는 장비 매니저가 없어 " +
                "게임플레이 데이터를 저장하지 못했습니다.\n" +
                $"InventoryManager: " +
                $"{(inventory != null ? "있음" : "없음")}\n" +
                $"EquipmentManager: " +
                $"{(equipment != null ? "있음" : "없음")}"
            );

            return;
        }

        GameplaySaveData saveData =
            new GameplaySaveData();

        HashSet<string> savedUniqueIds =
            new HashSet<string>();

        if (inventory.items != null)
        {
            foreach (
                InventoryItem item
                in inventory.items)
            {
                AddItemToSaveData(
                    saveData,
                    savedUniqueIds,
                    item,
                    false,
                    EquipmentSlotType.MainWeapon
                );
            }
        }

        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.Head,
            true,
            EquipmentSlotType.Head
        );

        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.Armor,
            true,
            EquipmentSlotType.Armor
        );

        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.Shoes,
            true,
            EquipmentSlotType.Shoes
        );

        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.MainWeapon,
            true,
            EquipmentSlotType.MainWeapon
        );

        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.SubWeapon,
            true,
            EquipmentSlotType.SubWeapon
        );

        string json =
            JsonUtility.ToJson(
                saveData
            );

        PlayerPrefs.SetString(
            GameplaySaveKey,
            json
        );

        PlayerPrefs.Save();

        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 저장 완료: " +
                saveData.items.Count +
                "개 아이템"
            );
        }
    }

    private void AddItemToSaveData(
        GameplaySaveData saveData,
        HashSet<string> savedUniqueIds,
        InventoryItem item,
        bool isEquipped,
        EquipmentSlotType equippedSlot)
    {
        if (item == null)
            return;

        if (item.data == null)
        {
            Debug.LogWarning(
                "[SaveManager] ItemData가 없는 아이템은 " +
                "저장하지 않습니다.\n" +
                $"UniqueId: {item.uniqueId}\n" +
                $"장착 여부: {isEquipped}\n" +
                $"장착 슬롯: {equippedSlot}"
            );

            return;
        }

        /*
         * ItemDatabase에 등록된 실제 아이템 ID는
         * 1001 이상 또는 2001 이상이다.
         * 0은 잘못 생성된 ItemData의 기본값이다.
         */
        if (item.data.id <= 0)
        {
            Debug.LogError(
                "[SaveManager] 잘못된 아이템 ID라 저장하지 않습니다.\n" +
                $"아이템 이름: {item.data.itemName}\n" +
                $"아이템 ID: {item.data.id}\n" +
                $"UniqueId: {item.uniqueId}\n" +
                $"장착 여부: {isEquipped}\n" +
                $"장착 슬롯: {equippedSlot}"
            );

            return;
        }

        if (string.IsNullOrEmpty(
            item.uniqueId))
        {
            item.uniqueId =
                Guid.NewGuid().ToString();
        }

        if (!savedUniqueIds.Add(
            item.uniqueId))
        {
            Debug.LogWarning(
                "[SaveManager] 동일 아이템이 중복 저장되는 것을 " +
                "막았습니다.\n" +
                $"아이템: {item.data.itemName}\n" +
                $"UniqueId: {item.uniqueId}"
            );

            return;
        }

        InventoryItemSaveData savedItem =
            new InventoryItemSaveData
            {
                uniqueId =
                    item.uniqueId,

                itemId =
                    item.data.id,

                positionX =
                    item.position.x,

                positionY =
                    item.position.y,

                rotation =
                    item.rotation,

                remainUseCount =
                    item.remainUseCount,

                currentDurability =
                    item.currentDurability,

                isEquipped =
                    isEquipped,

                equippedSlot =
                    equippedSlot
            };

        saveData.items.Add(
            savedItem
        );

        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 아이템 저장\n" +
                $"이름: {item.data.itemName}\n" +
                $"ID: {item.data.id}\n" +
                $"장착: {isEquipped}\n" +
                $"슬롯: {equippedSlot}\n" +
                $"내구도: {item.currentDurability}"
            );
        }
    }

    // =========================================================
    // 인벤토리 및 장비 불러오기
    // =========================================================

    public void LoadGameplayData()
    {
        if (isLoading ||
            isDeletingSave ||
            !HasSave() ||
            !HasGameplaySave())
        {
            return;
        }

        InventoryManager inventory =
            InventoryManager.Instance;

        EquipmentManager equipment =
            EquipmentManager.Instance;

        ItemDatabase itemDatabase =
            ItemDatabase.Instance;

        if (inventory == null ||
            equipment == null ||
            itemDatabase == null)
        {
            Debug.LogWarning(
                "[SaveManager] 아직 저장 데이터를 불러올 " +
                "준비가 되지 않았습니다."
            );

            return;
        }

        string json =
            PlayerPrefs.GetString(
                GameplaySaveKey,
                string.Empty
            );

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning(
                "[SaveManager] GameplaySaveData JSON이 비어 있습니다."
            );

            return;
        }

        GameplaySaveData saveData;

        try
        {
            saveData =
                JsonUtility
                    .FromJson<GameplaySaveData>(
                        json
                    );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[SaveManager] 저장 데이터 해석 실패\n" +
                exception
            );

            return;
        }

        if (saveData == null)
        {
            Debug.LogError(
                "[SaveManager] 저장 데이터를 생성하지 못했습니다."
            );

            return;
        }

        if (saveData.items == null)
        {
            saveData.items =
                new List<InventoryItemSaveData>();
        }

        isLoading = true;

        int restoredInventoryCount = 0;
        int restoredEquipmentCount = 0;
        int skippedCount = 0;

        try
        {
            inventory.ClearForLoad();
            equipment.ClearEquipmentForLoad();

            Dictionary<string, InventoryItem>
                restoredItems =
                    new Dictionary<
                        string,
                        InventoryItem
                    >();

            /*
             * 1단계:
             * 저장 데이터를 InventoryItem 객체로 복구한다.
             */
            foreach (
                InventoryItemSaveData savedItem
                in saveData.items)
            {
                if (savedItem == null)
                {
                    skippedCount++;
                    continue;
                }

                if (savedItem.itemId <= 0)
                {
                    Debug.LogWarning(
                        "[SaveManager] ID가 잘못된 기존 저장 아이템을 " +
                        "건너뜁니다.\n" +
                        $"아이템 ID: {savedItem.itemId}\n" +
                        $"UniqueId: {savedItem.uniqueId}\n" +
                        $"장착 여부: {savedItem.isEquipped}\n" +
                        $"장착 슬롯: {savedItem.equippedSlot}"
                    );

                    skippedCount++;
                    continue;
                }

                ItemData itemData =
                    itemDatabase.GetItem(
                        savedItem.itemId
                    );

                if (itemData == null)
                {
                    Debug.LogWarning(
                        "[SaveManager] ItemDatabase에서 아이템을 " +
                        "찾지 못했습니다.\n" +
                        $"아이템 ID: {savedItem.itemId}\n" +
                        $"UniqueId: {savedItem.uniqueId}"
                    );

                    skippedCount++;
                    continue;
                }

                InventoryItem restoredItem =
                    new InventoryItem(
                        itemData
                    );

                string restoredUniqueId =
                    string.IsNullOrEmpty(
                        savedItem.uniqueId)
                        ? Guid.NewGuid().ToString()
                        : savedItem.uniqueId;

                /*
                 * 이후 루프에서도 같은 키를 사용하도록
                 * 저장 데이터에도 생성된 ID를 반영한다.
                 */
                savedItem.uniqueId =
                    restoredUniqueId;

                restoredItem.uniqueId =
                    restoredUniqueId;

                restoredItem.position =
                    new Vector2Int(
                        savedItem.positionX,
                        savedItem.positionY
                    );

                restoredItem.SetRotation(
                    savedItem.rotation
                );

                restoredItem.remainUseCount =
                    Mathf.Max(
                        0,
                        savedItem.remainUseCount
                    );

                if (itemData.IsEquipment)
                {
                    restoredItem.currentDurability =
                        Mathf.Clamp(
                            savedItem.currentDurability,
                            0,
                            itemData.SafeMaxDurability
                        );
                }
                else
                {
                    restoredItem.currentDurability =
                        0;
                }

                if (restoredItems.ContainsKey(
                    restoredUniqueId))
                {
                    Debug.LogWarning(
                        "[SaveManager] 중복 UniqueId 저장 항목을 " +
                        "건너뜁니다.\n" +
                        $"UniqueId: {restoredUniqueId}\n" +
                        $"아이템: {itemData.itemName}"
                    );

                    skippedCount++;
                    continue;
                }

                restoredItems.Add(
                    restoredUniqueId,
                    restoredItem
                );
            }

            /*
             * 2단계:
             * 장착되지 않은 아이템을 인벤토리에 배치한다.
             */
            foreach (
                InventoryItemSaveData savedItem
                in saveData.items)
            {
                if (savedItem == null ||
                    savedItem.isEquipped ||
                    savedItem.itemId <= 0)
                {
                    continue;
                }

                if (!restoredItems.TryGetValue(
                    savedItem.uniqueId,
                    out InventoryItem restoredItem))
                {
                    continue;
                }

                bool restoredAtSavedPosition =
                    inventory.AddRestoredItem(
                        restoredItem
                    );

                if (!restoredAtSavedPosition)
                {
                    bool restoredAtEmptySpace =
                        inventory
                            .AddRestoredItemToEmptySpace(
                                restoredItem
                            );

                    if (!restoredAtEmptySpace)
                    {
                        Debug.LogWarning(
                            "[SaveManager] 인벤토리 공간이 없어 " +
                            "아이템을 복구하지 못했습니다.\n" +
                            $"아이템: {restoredItem.data.itemName}\n" +
                            $"ID: {restoredItem.data.id}"
                        );

                        skippedCount++;
                        continue;
                    }
                }

                restoredInventoryCount++;
            }

            /*
             * 3단계:
             * 장착된 아이템을 장비 슬롯에 연결한다.
             */
            foreach (
                InventoryItemSaveData savedItem
                in saveData.items)
            {
                if (savedItem == null ||
                    !savedItem.isEquipped ||
                    savedItem.itemId <= 0)
                {
                    continue;
                }

                if (!restoredItems.TryGetValue(
                    savedItem.uniqueId,
                    out InventoryItem restoredItem))
                {
                    continue;
                }

                bool restored =
                    equipment
                        .RestoreEquipmentForLoad(
                            savedItem.equippedSlot,
                            restoredItem
                        );

                if (restored)
                {
                    restoredEquipmentCount++;
                    continue;
                }

                Debug.LogWarning(
                    "[SaveManager] 장비 슬롯 복구에 실패하여 " +
                    "인벤토리로 반환합니다.\n" +
                    $"아이템: {restoredItem.data.itemName}\n" +
                    $"저장 슬롯: {savedItem.equippedSlot}"
                );

                bool returned =
                    inventory
                        .AddRestoredItemToEmptySpace(
                            restoredItem
                        );

                if (!returned)
                {
                    Debug.LogError(
                        "[SaveManager] 장비 복구와 인벤토리 반환이 " +
                        "모두 실패했습니다.\n" +
                        $"아이템: {restoredItem.data.itemName}"
                    );

                    skippedCount++;
                }
                else
                {
                    restoredInventoryCount++;
                }
            }

            inventory.FinishLoad();
            equipment.FinishEquipmentLoad();

            if (printSaveLog)
            {
                Debug.Log(
                    "[SaveManager] 불러오기 완료\n" +
                    $"저장 항목: {saveData.items.Count}\n" +
                    $"인벤토리 복구: {restoredInventoryCount}\n" +
                    $"장비 복구: {restoredEquipmentCount}\n" +
                    $"건너뛴 항목: {skippedCount}"
                );
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[SaveManager] 게임플레이 데이터 복구 중 오류\n" +
                exception
            );
        }
        finally
        {
            isLoading = false;
        }
    }

    // =========================================================
    // 저장 삭제 및 새 게임 초기화
    // =========================================================

    public void DeleteGameplaySave()
    {
        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );

        PlayerPrefs.Save();

        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 인벤토리 및 장비 저장 삭제 완료"
            );
        }
    }

    public void DeleteSave()
    {
        isDeletingSave = true;
        isLoading = false;

        StopAllCoroutines();

        DeleteBasicSaveKeys();
        DeleteDungeonKeys();
        DeletePlayerResourceKeys();

        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );

        /*
         * 현재 씬에 매니저가 살아 있으면
         * PlayerPrefs뿐 아니라 런타임 데이터도 초기화한다.
         */
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .ResetForNewGame(false);
        }

        if (PlayerResourceManager.Instance != null)
        {
            PlayerResourceManager.Instance
                .ResetForNewGame(false);
        }

        /*
         * 장비를 먼저 비워야 장비 능력치가 제거된다.
         */
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance
                .ClearEquipmentForLoad();

            EquipmentManager.Instance
                .FinishEquipmentLoad();
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance
                .ClearForLoad();

            InventoryManager.Instance
                .FinishLoad();
        }

        /*
         * 런타임 초기화 과정에서 다른 매니저가 다시 저장했을
         * 가능성을 차단하기 위해 키를 한 번 더 삭제한다.
         */
        DeleteBasicSaveKeys();
        DeleteDungeonKeys();
        DeletePlayerResourceKeys();

        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );

        PlayerPrefs.Save();

        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 기존 게임 데이터 전체 삭제 완료"
            );
        }

        isDeletingSave = false;
    }

    private void DeleteBasicSaveKeys()
    {
        PlayerPrefs.DeleteKey(
            HasSaveKey
        );

        PlayerPrefs.DeleteKey(
            CutscenePlayedKey
        );

        PlayerPrefs.DeleteKey(
            SelectedCharacterKey
        );

        PlayerPrefs.DeleteKey(
            PlayerNameKey
        );

        PlayerPrefs.DeleteKey(
            AfterCutsceneKey
        );
    }

    private void DeleteDungeonKeys()
    {
        PlayerPrefs.DeleteKey(
            DungeonRoomXKey
        );

        PlayerPrefs.DeleteKey(
            DungeonRoomYKey
        );

        PlayerPrefs.DeleteKey(
            DungeonVisitedKey
        );

        PlayerPrefs.DeleteKey(
            DungeonTurnKey
        );

        PlayerPrefs.DeleteKey(
            DungeonEnvironmentKey
        );
    }

    private void DeletePlayerResourceKeys()
    {
        PlayerPrefs.DeleteKey(
            PlayerHealthKey
        );

        PlayerPrefs.DeleteKey(
            PlayerMentalKey
        );

        PlayerPrefs.DeleteKey(
            PlayerHungerKey
        );

        PlayerPrefs.DeleteKey(
            PlayerLastProcessedTurnKey
        );
    }

    // =========================================================
    // 테스트용 Context Menu
    // =========================================================

    [ContextMenu("TEST - Save Gameplay")]
    private void TestSaveGameplay()
    {
        SaveGameplayData();
    }

    [ContextMenu("TEST - Load Gameplay")]
    private void TestLoadGameplay()
    {
        LoadGameplayData();
    }

    [ContextMenu("TEST - Delete All Save")]
    private void TestDeleteSave()
    {
        DeleteSave();
    }

    [ContextMenu("TEST - Print Gameplay JSON")]
    private void TestPrintGameplayJson()
    {
        string json =
            PlayerPrefs.GetString(
                GameplaySaveKey,
                string.Empty
            );

        Debug.Log(
            "[SaveManager] 현재 GameplaySaveData\n" +
            (string.IsNullOrEmpty(json)
                ? "저장 데이터 없음"
                : json)
        );
    }
}