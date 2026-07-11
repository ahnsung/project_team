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

    public InventoryItem Head => head;
    public InventoryItem Armor => armor;
    public InventoryItem Shoes => shoes;
    public InventoryItem MainWeapon => mainWeapon;
    public InventoryItem SubWeapon => subWeapon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
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
            inventoryManager =
                InventoryManager.Instance;

        if (playerStats == null)
            playerStats =
                PlayerStats.Instance;

        if (inventoryManager == null)
        {
            Debug.LogError(
                "[EquipmentManager] " +
                "InventoryManager를 찾을 수 없습니다."
            );
        }

        if (playerStats == null)
        {
            Debug.LogError(
                "[EquipmentManager] " +
                "PlayerStats를 찾을 수 없습니다."
            );
        }
    }

    public bool CanChangeEquipment()
    {
        if (BattleManager.Instance == null)
            return true;

        return !BattleManager.Instance
            .IsBattleRunning();
    }

    public bool EquipFromInventory(
        InventoryItem newItem)
    {
        ResolveReferences();

        if (newItem == null ||
            newItem.data == null)
        {
            Debug.LogWarning(
                "[EquipmentManager] 장착할 아이템이 없습니다."
            );

            return false;
        }

        if (!newItem.data.IsEquipment)
        {
            Debug.LogWarning(
                "[EquipmentManager] 장비 아이템이 아닙니다."
            );

            return false;
        }

        if (!CanChangeEquipment())
        {
            Debug.Log(
                "전투 중에는 장비를 교체할 수 없습니다."
            );

            return false;
        }

        if (inventoryManager == null ||
            !inventoryManager.ContainsItem(newItem))
        {
            Debug.LogWarning(
                "[EquipmentManager] " +
                "해당 장비가 인벤토리에 없습니다."
            );

            return false;
        }

        EquipmentSlotType targetSlot =
            GetDefaultSlot(newItem.data);

        return EquipToSlot(newItem, targetSlot);
    }

    public bool EquipToSlot(
        InventoryItem newItem,
        EquipmentSlotType targetSlot)
    {
        ResolveReferences();

        if (newItem == null ||
            newItem.data == null)
        {
            return false;
        }

        if (!IsCompatible(
            newItem.data,
            targetSlot))
        {
            Debug.LogWarning(
                "[EquipmentManager] " +
                "장비 부위와 슬롯이 맞지 않습니다."
            );

            return false;
        }

        if (!CanChangeEquipment())
        {
            Debug.Log(
                "전투 중에는 장비를 교체할 수 없습니다."
            );

            return false;
        }

        InventoryItem oldItem =
            GetEquippedItem(targetSlot);

        Vector2Int newItemOriginalPosition =
            newItem.position;

        int newItemOriginalRotation =
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
                bool rollbackSucceeded =
                    inventoryManager
                        .TryRestoreItemAtOriginalPosition(
                            newItem,
                            newItemOriginalPosition,
                            newItemOriginalRotation
                        );

                if (!rollbackSucceeded)
                {
                    Debug.LogError(
                        "[EquipmentManager] " +
                        "장착 교체 롤백에 실패했습니다."
                    );
                }

                Debug.Log(
                    "기존 장비를 돌려놓을 " +
                    "인벤토리 공간이 없습니다."
                );

                return false;
            }
        }

        SetEquippedItem(targetSlot, newItem);

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

        InventoryItem equipped =
            GetEquippedItem(slot);

        if (equipped == null)
            return false;

        if (inventoryManager == null)
            return false;

        bool returned =
            inventoryManager
                .TryReturnEquipmentToInventory(
                    equipped
                );

        if (!returned)
        {
            Debug.Log(
                "인벤토리에 장비를 " +
                "해제할 공간이 없습니다."
            );

            return false;
        }

        SetEquippedItem(slot, null);

        RecalculateEquipmentStats();
        NotifyChanged();

        return true;
    }

    public bool SwapWeapons()
    {
        ResolveReferences();

        if (BattleManager.Instance != null &&
            BattleManager.Instance
                .IsBattleRunning() &&
            !BattleManager.Instance
                .CanPlayerUseItem())
        {
            Debug.Log(
                "현재는 무기를 교체할 수 없습니다."
            );

            return false;
        }

        InventoryItem temp = mainWeapon;
        mainWeapon = subWeapon;
        subWeapon = temp;

        RecalculateEquipmentStats();
        NotifyChanged();

        return true;
    }

    public void ConsumeMainWeaponDurability(
        int amount)
    {
        if (mainWeapon == null)
            return;

        mainWeapon.ReduceDurability(amount);

        if (mainWeapon.IsBroken)
        {
            Debug.Log(
                $"{mainWeapon.data.itemName}이(가) 파괴되었습니다."
            );

            mainWeapon = null;
            RecalculateEquipmentStats();
        }

        NotifyChanged();
    }

    public void ConsumeArmorDurability(
        int amount)
    {
        EquipmentSlotType targetSlot;
        InventoryItem targetItem;

        if (armor != null)
        {
            targetSlot =
                EquipmentSlotType.Armor;

            targetItem = armor;
        }
        else if (head != null)
        {
            targetSlot =
                EquipmentSlotType.Head;

            targetItem = head;
        }
        else if (shoes != null)
        {
            targetSlot =
                EquipmentSlotType.Shoes;

            targetItem = shoes;
        }
        else
        {
            return;
        }

        targetItem.ReduceDurability(amount);

        if (targetItem.IsBroken)
        {
            Debug.Log(
                $"{targetItem.data.itemName}이(가) 파괴되었습니다."
            );

            SetEquippedItem(targetSlot, null);
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

        switch (slot)
        {
            case EquipmentSlotType.Head:
                return data.equipmentType ==
                       EquipmentType.Head;

            case EquipmentSlotType.Armor:
                return data.equipmentType ==
                       EquipmentType.Armor;

            case EquipmentSlotType.Shoes:
                return data.equipmentType ==
                       EquipmentType.Shoes;

            case EquipmentSlotType.MainWeapon:
            case EquipmentSlotType.SubWeapon:
                return data.equipmentType ==
                       EquipmentType.Weapon;

            default:
                return false;
        }
    }

    private void RecalculateEquipmentStats()
    {
        if (playerStats == null)
            return;

        EquipmentStatModifier total =
            new EquipmentStatModifier();

        AddEquipmentModifier(total, head);
        AddEquipmentModifier(total, armor);
        AddEquipmentModifier(total, shoes);

        // 보조 무기는 보너스를 제공하지 않는다.
        AddEquipmentModifier(total, mainWeapon);

        playerStats.SetEquipmentBonuses(total);
    }

    private void AddEquipmentModifier(
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

        total.Add(item.data.statModifier);
    }

    private void NotifyChanged()
    {
        OnEquipmentChanged?.Invoke();

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.RefreshUI();
        }
    }

    // Unity Button 테스트용
    public void EquipFirstWeaponForTest()
    {
        ResolveReferences();

        if (inventoryManager == null)
            return;

        InventoryItem weapon =
            inventoryManager.FindFirstEquipment(
                EquipmentType.Weapon
            );

        if (weapon == null)
        {
            Debug.Log(
                "인벤토리에 무기 장비가 없습니다."
            );

            return;
        }

        EquipFromInventory(weapon);
    }

    public void EquipFirstArmorForTest()
    {
        ResolveReferences();

        if (inventoryManager == null)
            return;

        InventoryItem armorItem =
            inventoryManager.FindFirstEquipment(
                EquipmentType.Armor
            );

        if (armorItem == null)
        {
            Debug.Log(
                "인벤토리에 갑옷 장비가 없습니다."
            );

            return;
        }

        EquipFromInventory(armorItem);
    }

    public void UnequipMainWeaponForTest()
    {
        Unequip(EquipmentSlotType.MainWeapon);
    }

    public void UnequipArmorForTest()
    {
        Unequip(EquipmentSlotType.Armor);
    }
}