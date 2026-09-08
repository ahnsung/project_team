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

    [Header("Equipment Test Sprites")]
    public Sprite testWeaponIcon;
    public Sprite testArmorIcon;

    [Header("Key Sprites")]
    [Tooltip("테스트용 열쇠 공용 아이콘. 나중에 개별 아이콘이 생기면 분리 가능")]
    public Sprite keyIcon;


    private readonly Dictionary<int, ItemData> database =
        new Dictionary<int, ItemData>();


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateItems();
    }


    // =========================================================
    // Item Creation
    // =========================================================

    private void CreateItems()
    {
        database.Clear();


        // =====================================================
        // 회복 아이템
        // =====================================================

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


        // =====================================================
        // 긴 붕대
        // =====================================================

        ItemData longBandage =
            NewBaseItem(
                1002,
                "긴 붕대",
                ItemCategory.Recovery,
                "테스트용 2칸 회복 아이템입니다.",
                longBandageIcon
            );

        longBandage.maxUseCount = 2;

        longBandage.consumeTurnOnUse = true;


        longBandage.shape.Add(
            new Vector2Int(
                0,
                0
            )
        );

        longBandage.shape.Add(
            new Vector2Int(
                1,
                0
            )
        );


        database[
            longBandage.id
        ] = longBandage;


        // =====================================================
        // 의료 키트
        // =====================================================

        ItemData medKit =
            NewBaseItem(
                1003,
                "의료 키트",
                ItemCategory.Recovery,
                "테스트용 2x2 회복 아이템입니다.",
                medKitIcon
            );

        medKit.maxUseCount = 2;

        medKit.consumeTurnOnUse = true;


        medKit.shape.Add(
            new Vector2Int(
                0,
                0
            )
        );

        medKit.shape.Add(
            new Vector2Int(
                1,
                0
            )
        );

        medKit.shape.Add(
            new Vector2Int(
                0,
                1
            )
        );

        medKit.shape.Add(
            new Vector2Int(
                1,
                1
            )
        );


        database[
            medKit.id
        ] = medKit;


        // =====================================================
        // 고철 조각
        // =====================================================

        ItemData scrap =
            NewBaseItem(
                1005,
                "고철 조각",
                ItemCategory.Etc,
                "ㄱ자 테스트 아이템입니다.",
                scrapIcon
            );

        scrap.maxUseCount = 1;


        scrap.shape.Add(
            new Vector2Int(
                0,
                0
            )
        );

        scrap.shape.Add(
            new Vector2Int(
                0,
                1
            )
        );

        scrap.shape.Add(
            new Vector2Int(
                1,
                1
            )
        );


        database[
            scrap.id
        ] = scrap;


        // =====================================================
        // 테스트 무기
        // =====================================================

        ItemData testWeapon =
            NewBaseItem(
                2001,
                "Test Rifle",
                ItemCategory.Equipment,
                "공격력 +5, DEX +1을 제공하는 테스트 무기입니다.",
                testWeaponIcon
            );


        testWeapon.equipmentType =
            EquipmentType.Weapon;

        testWeapon.maxDurability =
            50;

        testWeapon.statModifier.dex =
            1;

        testWeapon.statModifier.attackPower =
            5;

        testWeapon.weaponSkillDescription =
            "무기 스킬은 현재 보류 상태입니다.";


        testWeapon.shape.Add(
            new Vector2Int(
                0,
                0
            )
        );

        testWeapon.shape.Add(
            new Vector2Int(
                1,
                0
            )
        );

        testWeapon.shape.Add(
            new Vector2Int(
                2,
                0
            )
        );


        database[
            testWeapon.id
        ] = testWeapon;


        // =====================================================
        // 테스트 갑옷
        // =====================================================

        ItemData testArmor =
            NewBaseItem(
                2002,
                "Test Armor",
                ItemCategory.Equipment,
                "명중률 +10, INT +1을 제공하는 테스트 갑옷입니다.",
                testArmorIcon
            );


        testArmor.equipmentType =
            EquipmentType.Armor;

        testArmor.maxDurability =
            50;

        testArmor.statModifier.intelligence =
            1;

        testArmor.statModifier.accuracyBonus =
            10;


        testArmor.shape.Add(
            new Vector2Int(
                0,
                0
            )
        );

        testArmor.shape.Add(
            new Vector2Int(
                0,
                1
            )
        );

        testArmor.shape.Add(
            new Vector2Int(
                1,
                1
            )
        );


        database[
            testArmor.id
        ] = testArmor;


        // =====================================================
        // 테스트 열쇠
        //
        // 기획자 최종 ItemID가 아직 없으므로
        // 3001~3004를 임시 ID로 사용한다.
        //
        // 열쇠는:
        // - 인벤토리 아이템
        // - 버리기 불가
        // - 일반 사용 불가
        // - 잠긴 문을 열 때만 소비
        // =====================================================

        CreateKeyItem(
            3001,
            "열쇠 K_1",
            "잠긴 문 K_1을 열기 위한 열쇠입니다.",
            keyIcon
        );


        CreateKeyItem(
            3002,
            "열쇠 K_2",
            "잠긴 문 K_2를 열기 위한 열쇠입니다.",
            keyIcon
        );


        CreateKeyItem(
            3003,
            "열쇠 K_3",
            "잠긴 문 K_3을 열기 위한 열쇠입니다.",
            keyIcon
        );


        CreateKeyItem(
            3004,
            "열쇠 K_4",
            "잠긴 문 K_4를 열기 위한 열쇠입니다.",
            keyIcon
        );


        Debug.Log(
            "[ItemDatabase] 아이템 등록 완료: " +
            database.Count +
            "개"
        );
    }


    // =========================================================
    // Base Item
    // =========================================================

    private ItemData NewBaseItem(
        int id,
        string itemName,
        ItemCategory category,
        string description,
        Sprite icon)
    {
        ItemData item =
            new ItemData
            {
                id = id,

                itemName =
                    itemName,

                category =
                    category,

                maxUseCount =
                    0,

                consumeTurnOnUse =
                    false,

                canDrop =
                    true,

                effectDescription =
                    description,

                icon =
                    icon,

                shape =
                    new List<Vector2Int>(),

                statModifier =
                    new EquipmentStatModifier()
            };


        return item;
    }


    // =========================================================
    // Recovery
    // =========================================================

    private void CreateRecoveryItem(
        int id,
        string itemName,
        string description,
        Sprite icon)
    {
        ItemData item =
            NewBaseItem(
                id,
                itemName,
                ItemCategory.Recovery,
                description,
                icon
            );


        item.maxUseCount =
            2;

        item.consumeTurnOnUse =
            true;


        item.shape.Add(
            Vector2Int.zero
        );


        database[
            id
        ] = item;
    }


    // =========================================================
    // Key
    // =========================================================

    private void CreateKeyItem(
        int id,
        string itemName,
        string description,
        Sprite icon)
    {
        ItemData key =
            NewBaseItem(
                id,
                itemName,
                ItemCategory.Etc,
                description,
                icon
            );


        // 열쇠는 사용 아이템이 아님
        key.maxUseCount =
            0;

        key.consumeTurnOnUse =
            false;


        // 중요:
        // 열쇠는 플레이어가 버릴 수 없다.
        key.canDrop =
            false;


        // 인벤토리 1칸
        key.shape.Add(
            Vector2Int.zero
        );


        database[
            id
        ] = key;
    }


    // =========================================================
    // Get Item
    // =========================================================

    public ItemData GetItem(
        int id)
    {
        if (database.TryGetValue(
            id,
            out ItemData item))
        {
            return item;
        }


        Debug.LogError(
            "ItemDatabase에 없는 아이템 ID: " +
            id
        );


        return null;
    }


    // =========================================================
    // Utility
    // =========================================================

    public bool HasItem(
        int id)
    {
        return database.ContainsKey(
            id
        );
    }
}