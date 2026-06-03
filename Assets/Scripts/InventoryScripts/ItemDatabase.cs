using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [Header("Test Sprites")]
    public Sprite bandageIcon;
    public Sprite longBandageIcon;
    public Sprite medKitIcon;
    public Sprite scrapIcon;

    private Dictionary<int, ItemData> database = new Dictionary<int, ItemData>();

    private void Awake()
    {
        Instance = this;
        CreateTestItems();
    }

    private void CreateTestItems()
    {
        database.Clear();

        ItemData bandage = new ItemData();
        bandage.id = 1001;
        bandage.itemName = "붕대";
        bandage.category = ItemCategory.Recovery;
        bandage.maxUseCount = 2;
        bandage.consumeTurnOnUse = true;
        bandage.canDrop = true;
        bandage.effectDescription = "체력을 2 회복합니다.";
        bandage.icon = bandageIcon;
        bandage.shape.Add(new Vector2Int(0, 0));
        database[bandage.id] = bandage;

        ItemData longBandage = new ItemData();
        longBandage.id = 1002;
        longBandage.itemName = "긴 붕대";
        longBandage.category = ItemCategory.Recovery;
        longBandage.maxUseCount = 3;
        longBandage.consumeTurnOnUse = true;
        longBandage.canDrop = true;
        longBandage.effectDescription = "체력을 3 회복합니다. 가로 2칸 아이템입니다.";
        longBandage.icon = longBandageIcon;
        longBandage.shape.Add(new Vector2Int(0, 0));
        longBandage.shape.Add(new Vector2Int(1, 0));
        database[longBandage.id] = longBandage;

        ItemData medKit = new ItemData();
        medKit.id = 1003;
        medKit.itemName = "의료 키트";
        medKit.category = ItemCategory.Recovery;
        medKit.maxUseCount = 1;
        medKit.consumeTurnOnUse = true;
        medKit.canDrop = true;
        medKit.effectDescription = "체력을 크게 회복합니다. 2x2 아이템입니다.";
        medKit.icon = medKitIcon;
        medKit.shape.Add(new Vector2Int(0, 0));
        medKit.shape.Add(new Vector2Int(1, 0));
        medKit.shape.Add(new Vector2Int(0, 1));
        medKit.shape.Add(new Vector2Int(1, 1));
        database[medKit.id] = medKit;

        ItemData scrap = new ItemData();
        scrap.id = 1004;
        scrap.itemName = "고철 조각";
        scrap.category = ItemCategory.Etc;
        scrap.maxUseCount = 1;
        scrap.consumeTurnOnUse = false;
        scrap.canDrop = true;
        scrap.effectDescription = "ㄱ자 형태의 테스트 아이템입니다.";
        scrap.icon = scrapIcon;
        scrap.shape.Add(new Vector2Int(0, 0));
        scrap.shape.Add(new Vector2Int(0, 1));
        scrap.shape.Add(new Vector2Int(1, 1));
        database[scrap.id] = scrap;
    }

    public ItemData GetItem(int id)
    {
        if (database.ContainsKey(id))
            return database[id];

        Debug.LogError("ItemDatabase에 없는 아이템 ID: " + id);
        return null;
    }
}