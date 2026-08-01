using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("References")]
    [SerializeField]
    private InventoryManager inventoryManager;

    [SerializeField]
    private PlayerStats playerStats;

    [Header("Equipped Items")]
    [SerializeField]
    private InventoryItem head;

    [SerializeField]
    private InventoryItem armor;

    [SerializeField]
    private InventoryItem shoes;

    [SerializeField]
    private InventoryItem mainWeapon;

    [SerializeField]
    private InventoryItem subWeapon;

    public event Action OnEquipmentChanged;

    public InventoryItem Head
    {
        get
        {
            return head;
        }
    }

    public InventoryItem Armor
    {
        get
        {
            return armor;
        }
    }

    public InventoryItem Shoes
    {
        get
        {
            return shoes;
        }
    }

    public InventoryItem MainWeapon
    {
        get
        {
            return mainWeapon;
        }
    }

    public InventoryItem SubWeapon
    {
        get
        {
            return subWeapon;
        }
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
    }

    private void Start()
    {
        ResolveReferences();
        RecalculateEquipmentStats();
    }

    private void ResolveReferences()
    {
        if (inventoryManager == null)
        {
            inventoryManager =
                InventoryManager.Instance;
        }

        if (playerStats == null)
        {
            playerStats =
                PlayerStats.Instance;
        }
    }

    public bool CanChangeEquipment()
    {
        return
            BattleManager.Instance == null ||
            !BattleManager.Instance
                .IsBattleRunning();
    }

    public bool CanSwapWeapons()
    {
        if (BattleManager.Instance == null ||
            !BattleManager.Instance
                .IsBattleRunning())
        {
            return true;
        }

        return BattleManager.Instance
            .CanPlayerUseItem();
    }

    public bool EquipFromInventory(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null ||
            !item.data.IsEquipment)
        {
            return false;
        }

        EquipmentSlotType slot =
            GetDefaultSlot(
                item.data
            );

        return EquipToSlot(
            item,
            slot
        );
    }

    public bool EquipToSlot(
        InventoryItem newItem,
        EquipmentSlotType targetSlot)
    {
        ResolveReferences();

        if (inventoryManager == null ||
            playerStats == null)
        {
            Debug.LogError(
                "EquipmentManager 필수 참조가 없습니다."
            );

            return false;
        }

        if (newItem == null ||
            newItem.data == null ||
            !IsCompatible(
                newItem.data,
                targetSlot))
        {
            return false;
        }

        if (!CanChangeEquipment())
        {
            Debug.Log(
                "전투 중에는 장비를 장착하거나 교체할 수 없습니다."
            );

            return false;
        }

        if (!inventoryManager
            .ContainsItem(newItem))
        {
            return false;
        }

        InventoryItem oldItem =
            GetEquippedItem(
                targetSlot
            );

        Vector2Int originalPosition =
            newItem.position;

        int originalRotation =
            newItem.rotation;

        if (!inventoryManager
            .TryTakeItemForEquipment(
                newItem))
        {
            return false;
        }

        if (oldItem != null)
        {
            bool returned =
                inventoryManager
                    .TryReturnEquipmentToInventory(
                        oldItem
                    );

            if (!returned)
            {
                bool restored =
                    inventoryManager
                        .TryRestoreItem(
                            newItem,
                            originalPosition,
                            originalRotation
                        );

                if (!restored)
                {
                    Debug.LogError(
                        "장비 교체 실패 후 " +
                        "새 장비 복구에도 실패했습니다."
                    );
                }

                Debug.Log(
                    "기존 장비를 돌려놓을 " +
                    "인벤토리 공간이 없습니다."
                );

                return false;
            }
        }

        SetEquippedItem(
            targetSlot,
            newItem
        );

        RecalculateEquipmentStats();
        NotifyChanged();

        return true;
    }

    public bool Unequip(
        EquipmentSlotType slot)
    {
        ResolveReferences();

        if (!CanChangeEquipment())
        {
            Debug.Log(
                "전투 중에는 장비를 해제할 수 없습니다."
            );

            return false;
        }

        InventoryItem equippedItem =
            GetEquippedItem(slot);

        if (equippedItem == null ||
            inventoryManager == null)
        {
            return false;
        }

        bool returned =
            inventoryManager
                .TryReturnEquipmentToInventory(
                    equippedItem
                );

        if (!returned)
        {
            Debug.Log(
                "인벤토리에 장비를 해제할 공간이 없습니다."
            );

            return false;
        }

        SetEquippedItem(
            slot,
            null
        );

        RecalculateEquipmentStats();
        NotifyChanged();

        return true;
    }

    public bool SwapWeapons()
    {
        if (!CanSwapWeapons())
        {
            Debug.Log(
                "현재는 주 무기와 보조 무기를 교체할 수 없습니다."
            );

            return false;
        }

        InventoryItem temp =
            mainWeapon;

        mainWeapon =
            subWeapon;

        subWeapon =
            temp;

        RecalculateEquipmentStats();
        NotifyChanged();

        return true;
    }

    public void ConsumeMainWeaponDurability(
        int amount)
    {
        if (mainWeapon == null ||
            amount <= 0)
        {
            return;
        }

        mainWeapon.ReduceDurability(
            amount
        );

        if (mainWeapon.IsBroken)
        {
            Debug.Log(
                mainWeapon.data.itemName +
                "이(가) 파괴되었습니다."
            );

            mainWeapon = null;

            RecalculateEquipmentStats();
        }

        NotifyChanged();
    }

    public void ConsumeArmorDurability(
        int amount)
    {
        if (amount <= 0)
            return;

        EquipmentSlotType targetSlot;
        InventoryItem targetItem;

        if (armor != null)
        {
            targetSlot =
                EquipmentSlotType.Armor;

            targetItem =
                armor;
        }
        else if (head != null)
        {
            targetSlot =
                EquipmentSlotType.Head;

            targetItem =
                head;
        }
        else if (shoes != null)
        {
            targetSlot =
                EquipmentSlotType.Shoes;

            targetItem =
                shoes;
        }
        else
        {
            return;
        }

        targetItem.ReduceDurability(
            amount
        );

        if (targetItem.IsBroken)
        {
            Debug.Log(
                targetItem.data.itemName +
                "이(가) 파괴되었습니다."
            );

            SetEquippedItem(
                targetSlot,
                null
            );

            RecalculateEquipmentStats();
        }

        NotifyChanged();
    }

    public InventoryItem GetEquippedItem(
        EquipmentSlotType slot)
    {
        switch (slot)
        {
            case EquipmentSlotType.Head:
                return head;

            case EquipmentSlotType.Armor:
                return armor;

            case EquipmentSlotType.Shoes:
                return shoes;

            case EquipmentSlotType.MainWeapon:
                return mainWeapon;

            case EquipmentSlotType.SubWeapon:
                return subWeapon;

            default:
                return null;
        }
    }

    public EquipmentType GetEquipmentTypeForSlot(
        EquipmentSlotType slot)
    {
        switch (slot)
        {
            case EquipmentSlotType.Head:
                return EquipmentType.Head;

            case EquipmentSlotType.Armor:
                return EquipmentType.Armor;

            case EquipmentSlotType.Shoes:
                return EquipmentType.Shoes;

            case EquipmentSlotType.MainWeapon:
            case EquipmentSlotType.SubWeapon:
                return EquipmentType.Weapon;

            default:
                return EquipmentType.None;
        }
    }

    private EquipmentSlotType GetDefaultSlot(
        ItemData data)
    {
        switch (data.equipmentType)
        {
            case EquipmentType.Head:
                return EquipmentSlotType.Head;

            case EquipmentType.Armor:
                return EquipmentSlotType.Armor;

            case EquipmentType.Shoes:
                return EquipmentSlotType.Shoes;

            case EquipmentType.Weapon:
                return EquipmentSlotType.MainWeapon;

            default:
                throw new InvalidOperationException(
                    "장비 부위를 결정할 수 없습니다."
                );
        }
    }

    private bool IsCompatible(
        ItemData data,
        EquipmentSlotType slot)
    {
        if (data == null ||
            !data.IsEquipment)
        {
            return false;
        }

        return
            data.equipmentType ==
            GetEquipmentTypeForSlot(
                slot
            );
    }

    private void SetEquippedItem(
        EquipmentSlotType slot,
        InventoryItem item)
    {
        switch (slot)
        {
            case EquipmentSlotType.Head:
                head = item;
                break;

            case EquipmentSlotType.Armor:
                armor = item;
                break;

            case EquipmentSlotType.Shoes:
                shoes = item;
                break;

            case EquipmentSlotType.MainWeapon:
                mainWeapon = item;
                break;

            case EquipmentSlotType.SubWeapon:
                subWeapon = item;
                break;
        }
    }

    private void RecalculateEquipmentStats()
    {
        ResolveReferences();

        if (playerStats == null)
            return;

        EquipmentStatModifier total =
            new EquipmentStatModifier();

        AddModifier(
            total,
            head
        );

        AddModifier(
            total,
            armor
        );

        AddModifier(
            total,
            shoes
        );

        // 보조 무기는 능력치를 적용하지 않는다.
        AddModifier(
            total,
            mainWeapon
        );

        playerStats.SetEquipmentBonuses(
            total
        );
    }

    private void AddModifier(
        EquipmentStatModifier total,
        InventoryItem item)
    {
        if (item == null ||
            item.data == null ||
            item.IsBroken)
        {
            return;
        }

        total.Add(
            item.data.statModifier
        );
    }

    private void NotifyChanged()
    {
        OnEquipmentChanged?.Invoke();

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance
                .RefreshUI();
        }
    }


    // =============================
    // Save / Load
    // =============================

    public void ClearEquipmentForLoad()
    {
        head = null;
        armor = null;
        shoes = null;
        mainWeapon = null;
        subWeapon = null;

        RecalculateEquipmentStats();
    }

    public bool RestoreEquipmentForLoad(
        EquipmentSlotType slot,
        InventoryItem item)
    {
        if (item == null ||
            item.data == null ||
            !item.data.IsEquipment)
        {
            return false;
        }

        if (!IsCompatible(item.data, slot))
        {
            Debug.LogWarning(
                "[EquipmentManager] 장비 부위가 일치하지 않습니다: " +
                item.data.itemName +
                " / " +
                slot
            ); using System;
            using UnityEngine;

public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance;

        [Header("References")]
        [SerializeField]
        private InventoryManager inventoryManager;

        [SerializeField]
        private PlayerStats playerStats;

        [Header("Equipped Items")]
        [SerializeField]
        private InventoryItem head;

        [SerializeField]
        private InventoryItem armor;

        [SerializeField]
        private InventoryItem shoes;

        [SerializeField]
        private InventoryItem mainWeapon;

        [SerializeField]
        private InventoryItem subWeapon;

        [Header("Auto Save")]
        [Tooltip("장비 상태가 바뀔 때 인벤토리와 장비 상태를 자동 저장합니다.")]
        [SerializeField]
        private bool autoSaveEquipment = true;

        public event Action OnEquipmentChanged;

        public InventoryItem Head => head;
        public InventoryItem Armor => armor;
        public InventoryItem Shoes => shoes;
        public InventoryItem MainWeapon => mainWeapon;
        public InventoryItem SubWeapon => subWeapon;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ResolveReferences();
            RecalculateEquipmentStats();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ResolveReferences()
        {
            if (inventoryManager == null)
            {
                inventoryManager =
                    InventoryManager.Instance;
            }

            if (playerStats == null)
            {
                playerStats =
                    PlayerStats.Instance;
            }
        }

        public bool CanChangeEquipment()
        {
            return
                BattleManager.Instance == null ||
                !BattleManager.Instance
                    .IsBattleRunning();
        }

        public bool CanSwapWeapons()
        {
            if (BattleManager.Instance == null ||
                !BattleManager.Instance
                    .IsBattleRunning())
            {
                return true;
            }

            return BattleManager.Instance
                .CanPlayerUseItem();
        }

        public bool EquipFromInventory(
            InventoryItem item)
        {
            if (item == null ||
                item.data == null ||
                !item.data.IsEquipment)
            {
                return false;
            }

            EquipmentSlotType slot =
                GetDefaultSlot(item.data);

            return EquipToSlot(
                item,
                slot
            );
        }

        public bool EquipToSlot(
            InventoryItem newItem,
            EquipmentSlotType targetSlot)
        {
            ResolveReferences();

            if (inventoryManager == null ||
                playerStats == null)
            {
                Debug.LogError(
                    "[EquipmentManager] 필수 참조가 없습니다."
                );

                return false;
            }

            if (newItem == null ||
                newItem.data == null ||
                !IsCompatible(
                    newItem.data,
                    targetSlot))
            {
                return false;
            }

            if (newItem.data.id <= 0)
            {
                Debug.LogError(
                    "[EquipmentManager] ID가 잘못된 장비는 " +
                    "장착할 수 없습니다."
                );

                return false;
            }

            if (!CanChangeEquipment())
            {
                Debug.Log(
                    "[EquipmentManager] 전투 중에는 장비를 " +
                    "장착하거나 교체할 수 없습니다."
                );

                return false;
            }

            if (!inventoryManager.ContainsItem(
                    newItem))
            {
                return false;
            }

            InventoryItem oldItem =
                GetEquippedItem(targetSlot);

            Vector2Int originalPosition =
                newItem.position;

            int originalRotation =
                newItem.rotation;

            if (!inventoryManager
                .TryTakeItemForEquipment(newItem))
            {
                return false;
            }

            if (oldItem != null)
            {
                bool returned =
                    inventoryManager
                        .TryReturnEquipmentToInventory(
                            oldItem
                        );

                if (!returned)
                {
                    bool restored =
                        inventoryManager.TryRestoreItem(
                            newItem,
                            originalPosition,
                            originalRotation
                        );

                    if (!restored)
                    {
                        Debug.LogError(
                            "[EquipmentManager] 장비 교체 실패 후 " +
                            "새 장비 복구에도 실패했습니다."
                        );
                    }

                    Debug.Log(
                        "[EquipmentManager] 기존 장비를 돌려놓을 " +
                        "인벤토리 공간이 없습니다."
                    );

                    return false;
                }
            }

            SetEquippedItem(
                targetSlot,
                newItem
            );

            RecalculateEquipmentStats();
            NotifyChanged();
            AutoSaveEquipment();

            Debug.Log(
                "[EquipmentManager] 장착 완료: " +
                newItem.data.itemName +
                " / 슬롯: " +
                targetSlot
            );

            return true;
        }

        public bool Unequip(
            EquipmentSlotType slot)
        {
            ResolveReferences();

            if (!CanChangeEquipment())
            {
                Debug.Log(
                    "[EquipmentManager] 전투 중에는 장비를 " +
                    "해제할 수 없습니다."
                );

                return false;
            }

            InventoryItem equippedItem =
                GetEquippedItem(slot);

            if (equippedItem == null ||
                inventoryManager == null)
            {
                return false;
            }

            bool returned =
                inventoryManager
                    .TryReturnEquipmentToInventory(
                        equippedItem
                    );

            if (!returned)
            {
                Debug.Log(
                    "[EquipmentManager] 인벤토리에 장비를 " +
                    "해제할 공간이 없습니다."
                );

                return false;
            }

            SetEquippedItem(
                slot,
                null
            );

            RecalculateEquipmentStats();
            NotifyChanged();
            AutoSaveEquipment();

            Debug.Log(
                "[EquipmentManager] 장비 해제 완료: " +
                equippedItem.data.itemName +
                " / 슬롯: " +
                slot
            );

            return true;
        }

        public bool SwapWeapons()
        {
            if (!CanSwapWeapons())
            {
                Debug.Log(
                    "[EquipmentManager] 현재는 주 무기와 " +
                    "보조 무기를 교체할 수 없습니다."
                );

                return false;
            }

            if (mainWeapon == null &&
                subWeapon == null)
            {
                return false;
            }

            InventoryItem temporaryWeapon =
                mainWeapon;

            mainWeapon =
                subWeapon;

            subWeapon =
                temporaryWeapon;

            RecalculateEquipmentStats();
            NotifyChanged();
            AutoSaveEquipment();

            Debug.Log(
                "[EquipmentManager] 주 무기와 보조 무기 교체 완료"
            );

            return true;
        }

        public void ConsumeMainWeaponDurability(
            int amount)
        {
            if (mainWeapon == null ||
                mainWeapon.data == null ||
                amount <= 0)
            {
                return;
            }

            int durabilityBefore =
                mainWeapon.currentDurability;

            string weaponName =
                mainWeapon.data.itemName;

            mainWeapon.ReduceDurability(
                amount
            );

            Debug.Log(
                "[EquipmentManager] 무기 내구도 감소: " +
                weaponName +
                " / " +
                durabilityBefore +
                " -> " +
                mainWeapon.currentDurability
            );

            if (mainWeapon.IsBroken)
            {
                Debug.Log(
                    "[EquipmentManager] " +
                    weaponName +
                    "이(가) 파괴되었습니다."
                );

                mainWeapon = null;

                RecalculateEquipmentStats();
            }

            NotifyChanged();
            AutoSaveEquipment();
        }

        public void ConsumeArmorDurability(
            int amount)
        {
            if (amount <= 0)
                return;

            EquipmentSlotType targetSlot;
            InventoryItem targetItem;

            if (armor != null)
            {
                targetSlot =
                    EquipmentSlotType.Armor;

                targetItem =
                    armor;
            }
            else if (head != null)
            {
                targetSlot =
                    EquipmentSlotType.Head;

                targetItem =
                    head;
            }
            else if (shoes != null)
            {
                targetSlot =
                    EquipmentSlotType.Shoes;

                targetItem =
                    shoes;
            }
            else
            {
                return;
            }

            if (targetItem.data == null)
                return;

            int durabilityBefore =
                targetItem.currentDurability;

            string equipmentName =
                targetItem.data.itemName;

            targetItem.ReduceDurability(
                amount
            );

            Debug.Log(
                "[EquipmentManager] 방어 장비 내구도 감소: " +
                equipmentName +
                " / " +
                durabilityBefore +
                " -> " +
                targetItem.currentDurability
            );

            if (targetItem.IsBroken)
            {
                Debug.Log(
                    "[EquipmentManager] " +
                    equipmentName +
                    "이(가) 파괴되었습니다."
                );

                SetEquippedItem(
                    targetSlot,
                    null
                );

                RecalculateEquipmentStats();
            }

            NotifyChanged();
            AutoSaveEquipment();
        }

        public InventoryItem GetEquippedItem(
            EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Head:
                    return head;

                case EquipmentSlotType.Armor:
                    return armor;

                case EquipmentSlotType.Shoes:
                    return shoes;

                case EquipmentSlotType.MainWeapon:
                    return mainWeapon;

                case EquipmentSlotType.SubWeapon:
                    return subWeapon;

                default:
                    return null;
            }
        }

        public EquipmentType GetEquipmentTypeForSlot(
            EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Head:
                    return EquipmentType.Head;

                case EquipmentSlotType.Armor:
                    return EquipmentType.Armor;

                case EquipmentSlotType.Shoes:
                    return EquipmentType.Shoes;

                case EquipmentSlotType.MainWeapon:
                case EquipmentSlotType.SubWeapon:
                    return EquipmentType.Weapon;

                default:
                    return EquipmentType.None;
            }
        }

        private EquipmentSlotType GetDefaultSlot(
            ItemData data)
        {
            switch (data.equipmentType)
            {
                case EquipmentType.Head:
                    return EquipmentSlotType.Head;

                case EquipmentType.Armor:
                    return EquipmentSlotType.Armor;

                case EquipmentType.Shoes:
                    return EquipmentSlotType.Shoes;

                case EquipmentType.Weapon:
                    return EquipmentSlotType.MainWeapon;

                default:
                    throw new InvalidOperationException(
                        "장비 부위를 결정할 수 없습니다."
                    );
            }
        }

        private bool IsCompatible(
            ItemData data,
            EquipmentSlotType slot)
        {
            if (data == null ||
                !data.IsEquipment)
            {
                return false;
            }

            return
                data.equipmentType ==
                GetEquipmentTypeForSlot(slot);
        }

        private void SetEquippedItem(
            EquipmentSlotType slot,
            InventoryItem item)
        {
            switch (slot)
            {
                case EquipmentSlotType.Head:
                    head = item;
                    break;

                case EquipmentSlotType.Armor:
                    armor = item;
                    break;

                case EquipmentSlotType.Shoes:
                    shoes = item;
                    break;

                case EquipmentSlotType.MainWeapon:
                    mainWeapon = item;
                    break;

                case EquipmentSlotType.SubWeapon:
                    subWeapon = item;
                    break;
            }
        }

        private void RecalculateEquipmentStats()
        {
            ResolveReferences();

            if (playerStats == null)
                return;

            EquipmentStatModifier total =
                new EquipmentStatModifier();

            AddModifier(
                total,
                head
            );

            AddModifier(
                total,
                armor
            );

            AddModifier(
                total,
                shoes
            );

            // 보조 무기의 능력치는 적용하지 않는다.
            AddModifier(
                total,
                mainWeapon
            );

            playerStats.SetEquipmentBonuses(
                total
            );
        }

        private void AddModifier(
            EquipmentStatModifier total,
            InventoryItem item)
        {
            if (total == null ||
                item == null ||
                item.data == null ||
                item.IsBroken)
            {
                return;
            }

            if (item.data.statModifier == null)
                return;

            total.Add(
                item.data.statModifier
            );
        }

        private void NotifyChanged()
        {
            OnEquipmentChanged?.Invoke();

            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance
                    .RefreshUI();
            }
        }

        private void AutoSaveEquipment()
        {
            if (!autoSaveEquipment)
                return;

            SaveManager saveManager =
                SaveManager.Instance;

            if (saveManager == null)
            {
                Debug.LogWarning(
                    "[EquipmentManager] SaveManager가 없어 " +
                    "장비 상태를 자동 저장하지 못했습니다."
                );

                return;
            }

            /*
             * 저장 데이터를 불러오는 도중에는
             * 불완전한 중간 상태가 다시 저장되는 것을 막는다.
             */
            if (saveManager.IsLoading)
                return;

            saveManager.SaveGameplayData();
        }

        // =========================================================
        // Save / Load
        // =========================================================

        public void ClearEquipmentForLoad()
        {
            head = null;
            armor = null;
            shoes = null;
            mainWeapon = null;
            subWeapon = null;

            RecalculateEquipmentStats();
        }

        public bool RestoreEquipmentForLoad(
            EquipmentSlotType slot,
            InventoryItem item)
        {
            if (item == null ||
                item.data == null ||
                !item.data.IsEquipment ||
                item.data.id <= 0)
            {
                return false;
            }

            if (!IsCompatible(
                    item.data,
                    slot))
            {
                Debug.LogWarning(
                    "[EquipmentManager] 장비 부위가 " +
                    "일치하지 않습니다: " +
                    item.data.itemName +
                    " / " +
                    slot
                );

                return false;
            }

            if (GetEquippedItem(slot) != null)
            {
                Debug.LogWarning(
                    "[EquipmentManager] 동일 슬롯에 " +
                    "중복 장비 저장 데이터가 있습니다: " +
                    slot
                );

                return false;
            }

            SetEquippedItem(
                slot,
                item
            );

            /*
             * 로드 과정에서는 여기서 자동 저장하지 않는다.
             * 모든 장비 복구가 끝난 뒤 FinishEquipmentLoad가 호출된다.
             */
            return true;
        }

        public void FinishEquipmentLoad()
        {
            RecalculateEquipmentStats();
            NotifyChanged();
        }
    

            return false;
        }

        if (GetEquippedItem(slot) != null)
        {
            Debug.LogWarning(
                "[EquipmentManager] 동일 슬롯에 중복 장비 데이터가 있습니다: " +
                slot
            );

            return false;
        }

        SetEquippedItem(slot, item);
        return true;
    }

    public void FinishEquipmentLoad()
    {
        RecalculateEquipmentStats();
        NotifyChanged();
    }
}