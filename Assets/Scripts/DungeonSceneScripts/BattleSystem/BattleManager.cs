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
    [SerializeField]
    private bool printSaveLog = true;

    private bool isLoading;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnApplicationQuit()
    {
        if (HasSave())
        {
            SaveGameplayData();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && HasSave())
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
            if (InventoryManager.Instance != null &&
                EquipmentManager.Instance != null &&
                ItemDatabase.Instance != null)
            {
                LoadGameplayData();
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // =============================
    // 새 게임
    // =============================

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

        // 새 게임에서는 이전 인벤토리와 장비 데이터를 제거한다.
        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );

        PlayerPrefs.Save();
    }

    // =============================
    // 기본 저장 정보
    // =============================

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

    // =============================
    // 게임플레이 저장
    // =============================

    public bool HasGameplaySave()
    {
        return PlayerPrefs.HasKey(
            GameplaySaveKey
        );
    }

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
                $"[SaveManager] 저장 완료: " +
                $"{saveData.items.Count}개 아이템"
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
        if (item == null ||
            item.data == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(
                item.uniqueId))
        {
            item.uniqueId =
                Guid.NewGuid().ToString();
        }

        if (savedUniqueIds.Contains(
                item.uniqueId))
        {
            Debug.LogWarning(
                "[SaveManager] 중복 아이템 ID 발견: " +
                item.uniqueId
            );

            return;
        }

        savedUniqueIds.Add(
            item.uniqueId
        );

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

    // =============================
    // 게임플레이 불러오기
    // =============================

    public void LoadGameplayData()
    {
        if (isLoading)
            return;

        if (!HasGameplaySave())
            return;

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
                "[SaveManager] 아직 저장 데이터를 " +
                "불러올 준비가 되지 않았습니다."
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
                JsonUtility.FromJson<
                    GameplaySaveData
                >(json);
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

        isLoading = true;

        inventory.ClearForLoad();
        equipment.ClearEquipmentForLoad();

        Dictionary<string, InventoryItem>
            restoredItems =
                new Dictionary<
                    string,
                    InventoryItem
                >();

        foreach (
            InventoryItemSaveData savedItem
            in saveData.items)
        {
            if (savedItem == null)
                continue;

            ItemData itemData =
                itemDatabase.GetItem(
                    savedItem.itemId
                );

            if (itemData == null)
            {
                Debug.LogWarning(
                    "[SaveManager] 존재하지 않는 " +
                    "아이템 ID를 건너뜁니다: " +
                    savedItem.itemId
                );

                continue;
            }

            InventoryItem restoredItem =
                new InventoryItem(
                    itemData
                );

            restoredItem.uniqueId =
                string.IsNullOrEmpty(
                    savedItem.uniqueId
                )
                    ? Guid.NewGuid().ToString()
                    : savedItem.uniqueId;

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
                restoredItem.currentDurability = 0;
            }

            restoredItems[
                restoredItem.uniqueId
            ] = restoredItem;
        }

        // 먼저 인벤토리에 있는 아이템만 배치한다.
        foreach (
            InventoryItemSaveData savedItem
            in saveData.items)
        {
            if (savedItem == null ||
                savedItem.isEquipped)
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
                inventory.AddRestoredItem(
                    restoredItem
                );

            if (!restored)
            {
                Debug.LogWarning(
                    "[SaveManager] 인벤토리 복구 실패: " +
                    restoredItem.data.itemName
                );
            }
        }

        // 그다음 장착 아이템을 슬롯에 직접 연결한다.
        foreach (
            InventoryItemSaveData savedItem
            in saveData.items)
        {
            if (savedItem == null ||
                !savedItem.isEquipped)
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
                equipment.RestoreEquipmentForLoad(
                    savedItem.equippedSlot,
                    restoredItem
                );

            if (!restored)
            {
                Debug.LogWarning(
                    "[SaveManager] 장비 복구 실패: " +
                    restoredItem.data.itemName
                );

                inventory
                    .AddRestoredItemToEmptySpace(
                        restoredItem
                    );
            }
        }

        inventory.FinishLoad();
        equipment.FinishEquipmentLoad();

        isLoading = false;

        if (printSaveLog)
        {
            Debug.Log(
                $"[SaveManager] 불러오기 완료: " +
                $"{saveData.items.Count}개 아이템"
            );
        }
    }

    // =============================
    // 저장 삭제
    // =============================

    public void DeleteGameplaySave()
    {
        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );

        PlayerPrefs.Save();
    }

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
    }
}