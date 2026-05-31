using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [Header("Test Sprites")]
    public Sprite bandageIcon;

    private Dictionary<int, ItemData> database = new Dictionary<int, ItemData>();

    private void Awake()
    {
        Instance = this;
        CreateTestItems();
    }

    private void CreateTestItems()
    {
        ItemData bandage = new ItemData();
        bandage.id = 1001;
        bandage.itemName = "붕대";
        bandage.category = ItemCategory.Recovery;
        bandage.maxUseCount = 2;
        bandage.consumeTurnOnUse = true;
        bandage.canDrop = true;
        bandage.effectDescription = "체력을 2 회복합니다.";
        bandage.icon = bandageIcon;

        // 붕대는 1칸 아이템
        bandage.shape.Add(new Vector2Int(0, 0));

        database[bandage.id] = bandage;
    }

    public ItemData GetItem(int id)
    {
        if (database.ContainsKey(id))
            return database[id];

        Debug.LogError("ItemDatabase에 없는 아이템 ID: " + id);
        return null;
    }
}