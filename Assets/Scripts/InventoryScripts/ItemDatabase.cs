using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [Header("Item Sprites")]
    public Sprite bandageIcon;
    public Sprite cannedFoodIcon;
    public Sprite sedativeIcon;

    [Header("Old Test Sprites")]
    public Sprite longBandageIcon;
    public Sprite medKitIcon;
    public Sprite scrapIcon;

    private Dictionary<int, ItemData> database = new Dictionary<int, ItemData>();

    private void Awake()
    {
        Instance = this;
        CreateItems();
    }

    private void CreateItems()
    {
        database.Clear();

        CreateRecoveryItem(
            1001,
            "붕대",
            "체력을 10 회복합니다.",
            bandageIcon
        );

        CreateRecoveryItem(
            1004,
            "통조림",
            "배고픔을 10 회복합니다.",
            cannedFoodIcon
        );

        CreateRecoveryItem(
            1007,
            "진정제",
            "정신력을 10 회복합니다.",
            sedativeIcon
        );

        ItemData longBandage = new ItemData();
        longBandage.id = 1002;
        longBandage.itemName = "긴 붕대";
        longBandage.category = ItemCategory.Recovery;
        longBandage.maxUseCount = 2;
        longBandage.consumeTurnOnUse = true;
        longBandage.canDrop = true;
        longBandage.effectDescription = "테스트용 2칸 아이템입니다.";
        longBandage.icon = longBandageIcon;
        longBandage.shape.Add(new Vector2Int(0, 0));
        longBandage.shape.Add(new Vector2Int(1, 0));
        database[longBandage.id] = longBandage;

        ItemData medKit = new ItemData();
        medKit.id = 1003;
        medKit.itemName = "의료 키트";
        medKit.category = ItemCategory.Recovery;
        medKit.maxUseCount = 2;
        medKit.consumeTurnOnUse = true;
        medKit.canDrop = true;
        medKit.effectDescription = "테스트용 2x2 아이템입니다.";
        medKit.icon = medKitIcon;
        medKit.shape.Add(new Vector2Int(0, 0));
        medKit.shape.Add(new Vector2Int(1, 0));
        medKit.shape.Add(new Vector2Int(0, 1));
        medKit.shape.Add(new Vector2Int(1, 1));
        database[medKit.id] = medKit;

        ItemData scrap = new ItemData();
        scrap.id = 1005;
        scrap.itemName = "고철 조각";
        scrap.category = ItemCategory.Etc;
        scrap.maxUseCount = 1;
        scrap.consumeTurnOnUse = false;
        scrap.canDrop = true;
        scrap.effectDescription = "ㄱ자 테스트 아이템입니다.";
        scrap.icon = scrapIcon;
        scrap.shape.Add(new Vector2Int(0, 0));
        scrap.shape.Add(new Vector2Int(0, 1));
        scrap.shape.Add(new Vector2Int(1, 1));
        database[scrap.id] = scrap;
    }

    private void CreateRecoveryItem(int id, string name, string desc, Sprite icon)
    {
        ItemData item = new ItemData();
        item.id = id;
        item.itemName = name;
        item.category = ItemCategory.Recovery;
        item.maxUseCount = 2;
        item.consumeTurnOnUse = true;
        item.canDrop = true;
        item.effectDescription = desc;
        item.icon = icon;
        item.shape.Add(new Vector2Int(0, 0));

        database[id] = item;
    }

    public ItemData GetItem(int id)
    {
        if (database.ContainsKey(id))
            return database[id];

        Debug.LogError("ItemDatabase에 없는 아이템 ID: " + id);
        return null;
    }
}