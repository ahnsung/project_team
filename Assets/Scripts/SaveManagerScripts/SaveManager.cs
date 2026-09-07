using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string GameplaySaveKey = "GameplaySaveData";

    [Header("Debug")]
    [SerializeField] private bool printSaveLog = true;

    private bool isLoading;
    public bool IsLoading => isLoading;


    // =====================================
    // Save Data
    // =====================================

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

        public List<string> usedChestTiles =
            new List<string>();
    }


    // =====================================
    // Unity
    // =====================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(
                gameObject
            );
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -=
                OnSceneLoaded;
        }
    }


    private void OnApplicationQuit()
    {
        if (HasSave())
        {
            SaveGameplayData();
        }
    }


    private void OnApplicationPause(
        bool pauseStatus)
    {
        if (pauseStatus &&
            HasSave())
        {
            SaveGameplayData();
        }
    }


    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (!HasGameplaySave())
            return;

        StartCoroutine(
            LoadGameplayDataAfterSceneReady()
        );
    }


    private IEnumerator
        LoadGameplayDataAfterSceneReady()
    {
        yield return null;
        yield return null;

        float timeout = 3f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            bool inventoryReady =
                InventoryManager.Instance != null;

            bool equipmentReady =
                EquipmentManager.Instance != null;

            bool itemDatabaseReady =
                ItemDatabase.Instance != null;

            if (inventoryReady &&
                equipmentReady &&
                itemDatabaseReady)
            {
                LoadGameplayData();

                yield break;
            }

            elapsed +=
                Time.unscaledDeltaTime;

            yield return null;
        }
    }


    // =====================================
    // New Save
    // =====================================

    public void CreateNewSave(
        int characterID,
        string playerName)
    {
        PlayerPrefs.SetInt(
            "HasSave",
            1
        );

        PlayerPrefs.SetInt(
            "CutscenePlayed",
            1
        );

        PlayerPrefs.SetInt(
            "SelectedCharacter",
            characterID
        );

        PlayerPrefs.SetString(
            "PlayerName",
            playerName
        );

        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );

        PlayerPrefs.Save();


        if (DungeonTileEventManager.Instance != null)
        {
            DungeonTileEventManager.Instance
                .ClearUsedChestTiles();
        }
    }


    // =====================================
    // Basic Save Info
    // =====================================

    public bool HasSave()
    {
        return PlayerPrefs.GetInt(
            "HasSave",
            0
        ) == 1;
    }


    public bool HasPlayedCutscene()
    {
        return PlayerPrefs.GetInt(
            "CutscenePlayed",
            0
        ) == 1;
    }


    public int GetSelectedCharacter()
    {
        return PlayerPrefs.GetInt(
            "SelectedCharacter",
            -1
        );
    }


    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(
            "PlayerName",
            ""
        );
    }


    public bool HasGameplaySave()
    {
        return PlayerPrefs.HasKey(
            GameplaySaveKey
        );
    }


    // =====================================
    // Save Gameplay
    // =====================================

    public void SaveGameplayData()
    {
        if (isLoading)
            return;

        InventoryManager inventory =
            InventoryManager.Instance;

        EquipmentManager equipment =
            EquipmentManager.Instance;

        if (inventory == null ||
            equipment == null)
        {
            return;
        }


        GameplaySaveData saveData =
            new GameplaySaveData();

        HashSet<string> savedUniqueIds =
            new HashSet<string>();


        // =====================================
        // Inventory
        // =====================================

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


        // =====================================
        // Equipment
        // =====================================

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


        // =====================================
        // Chest
        // =====================================

        DungeonTileEventManager tileEventManager =
            DungeonTileEventManager.Instance;

        if (tileEventManager != null)
        {
            saveData.usedChestTiles =
                tileEventManager
                    .GetUsedChestTilesForSave();
        }


        // =====================================
        // JSON
        // =====================================

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
                "[SaveManager] 저장 완료\n" +
                $"아이템: {saveData.items.Count}개\n" +
                $"사용한 상자: {saveData.usedChestTiles.Count}개"
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
                "[SaveManager] data가 없는 아이템은 저장하지 않습니다."
            );

            return;
        }

        if (item.data.id <= 0)
        {
            Debug.LogWarning(
                "[SaveManager] 잘못된 ItemID를 가진 아이템은 저장하지 않습니다.\n" +
                $"ItemID: {item.data.id}"
            );

            return;
        }


        if (string.IsNullOrEmpty(
            item.uniqueId))
        {
            item.uniqueId =
                Guid.NewGuid()
                    .ToString();
        }


        if (!savedUniqueIds.Add(
            item.uniqueId))
        {
            Debug.LogWarning(
                "[SaveManager] 중복 아이템 ID 발견: " +
                item.uniqueId
            );

            return;
        }


        InventoryItemSaveData itemData =
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
            itemData
        );
    }


    // =====================================
    // Load Gameplay
    // =====================================

    public void LoadGameplayData()
    {
        if (isLoading ||
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
                "[SaveManager] 아직 저장 데이터를 불러올 준비가 되지 않았습니다."
            );

            return;
        }


        string json =
            PlayerPrefs.GetString(
                GameplaySaveKey,
                ""
            );

        if (string.IsNullOrEmpty(json))
            return;


        GameplaySaveData saveData;


        try
        {
            saveData =
                JsonUtility.FromJson<GameplaySaveData>(
                    json
                );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[SaveManager] 저장 데이터 해석 실패: " +
                exception.Message
            );

            return;
        }


        if (saveData == null)
            return;


        if (saveData.items == null)
        {
            saveData.items =
                new List<InventoryItemSaveData>();
        }


        if (saveData.usedChestTiles == null)
        {
            saveData.usedChestTiles =
                new List<string>();
        }


        isLoading = true;


        try
        {
            // =====================================
            // Clear
            // =====================================

            inventory.ClearForLoad();

            equipment
                .ClearEquipmentForLoad();


            Dictionary<string, InventoryItem>
                restoredItems =
                    new Dictionary<string, InventoryItem>();


            int skippedInvalidItems = 0;


            // =====================================
            // Create InventoryItem objects
            // =====================================

            foreach (
                InventoryItemSaveData savedItem
                in saveData.items)
            {
                if (savedItem == null)
                    continue;


                // =================================
                // ItemID 0 방어
                // =================================

                if (savedItem.itemId <= 0)
                {
                    skippedInvalidItems++;

                    Debug.LogWarning(
                        "[SaveManager] 잘못된 저장 아이템을 건너뜁니다.\n" +
                        $"ItemID: {savedItem.itemId}"
                    );

                    continue;
                }


                ItemData itemData =
                    itemDatabase.GetItem(
                        savedItem.itemId
                    );


                if (itemData == null)
                {
                    skippedInvalidItems++;

                    Debug.LogWarning(
                        "[SaveManager] ItemDatabase에 존재하지 않는 " +
                        "저장 아이템을 건너뜁니다.\n" +
                        $"ItemID: {savedItem.itemId}"
                    );

                    continue;
                }


                InventoryItem restoredItem =
                    new InventoryItem(
                        itemData
                    );


                string restoredUniqueId =
                    string.IsNullOrEmpty(
                        savedItem.uniqueId
                    )
                    ? Guid.NewGuid()
                        .ToString()
                    : savedItem.uniqueId;


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


                restoredItems[
                    restoredUniqueId
                ] = restoredItem;
            }


            // =====================================
            // Inventory Restore
            // =====================================

            foreach (
                InventoryItemSaveData savedItem
                in saveData.items)
            {
                if (savedItem == null)
                    continue;

                if (savedItem.itemId <= 0)
                    continue;

                if (savedItem.isEquipped)
                    continue;


                if (!restoredItems.TryGetValue(
                    savedItem.uniqueId,
                    out InventoryItem restoredItem))
                {
                    continue;
                }


                bool restored =
                    inventory.AddRestoredItem(
                        restoredItem
                    );


                if (!restored)
                {
                    inventory
                        .AddRestoredItemToEmptySpace(
                            restoredItem
                        );
                }
            }


            // =====================================
            // Equipment Restore
            // =====================================

            foreach (
                InventoryItemSaveData savedItem
                in saveData.items)
            {
                if (savedItem == null)
                    continue;

                if (savedItem.itemId <= 0)
                    continue;

                if (!savedItem.isEquipped)
                    continue;


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


                if (!restored)
                {
                    inventory
                        .AddRestoredItemToEmptySpace(
                            restoredItem
                        );
                }
            }


            inventory.FinishLoad();

            equipment
                .FinishEquipmentLoad();


            // =====================================
            // Chest Restore
            // =====================================

            DungeonTileEventManager
                tileEventManager =
                    DungeonTileEventManager.Instance;


            if (tileEventManager != null)
            {
                tileEventManager
                    .RestoreUsedChestTiles(
                        saveData.usedChestTiles
                    );
            }


            // =====================================
            // Log
            // =====================================

            if (printSaveLog)
            {
                Debug.Log(
                    "[SaveManager] 불러오기 완료\n" +
                    $"저장 데이터 아이템: {saveData.items.Count}개\n" +
                    $"정상 복구 아이템: {restoredItems.Count}개\n" +
                    $"잘못된 아이템 건너뜀: {skippedInvalidItems}개\n" +
                    $"사용한 상자: {saveData.usedChestTiles.Count}개"
                );
            }
        }
        finally
        {
            isLoading = false;
        }


        // =====================================
        // 깨진 옛 세이브 자동 정리
        // =====================================

        SaveGameplayData();
    }


    // =====================================
    // Delete Gameplay Save
    // =====================================

    public void DeleteGameplaySave()
    {
        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );

        PlayerPrefs.Save();


        if (DungeonTileEventManager.Instance != null)
        {
            DungeonTileEventManager.Instance
                .ClearUsedChestTiles();
        }
    }


    // =====================================
    // Delete All Save
    // =====================================

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(
            "HasSave"
        );

        PlayerPrefs.DeleteKey(
            "CutscenePlayed"
        );

        PlayerPrefs.DeleteKey(
            "SelectedCharacter"
        );

        PlayerPrefs.DeleteKey(
            "PlayerName"
        );

        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );

        PlayerPrefs.Save();


        if (DungeonTileEventManager.Instance != null)
        {
            DungeonTileEventManager.Instance
                .ClearUsedChestTiles();
        }
    }


    // =====================================
    // Debug
    // =====================================

    [ContextMenu(
        "TEST - Save Gameplay"
    )]
    private void TestSaveGameplay()
    {
        SaveGameplayData();
    }


    [ContextMenu(
        "TEST - Load Gameplay"
    )]
    private void TestLoadGameplay()
    {
        LoadGameplayData();
    }
}