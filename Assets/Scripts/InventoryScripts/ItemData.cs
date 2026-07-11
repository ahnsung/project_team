using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    [Header("Basic")]
    public int id;
    public string itemName;
    public ItemCategory category;

    [Header("Consumable")]
    [Min(0)]
    public int maxUseCount;

    public bool consumeTurnOnUse;
    public bool canDrop;

    [TextArea]
    public string effectDescription;

    public Sprite icon;

    [Header("Inventory Shape")]
    [Tooltip("아이템이 차지하는 상대 좌표입니다. 반드시 (0,0)을 포함하는 것이 좋습니다.")]
    public List<Vector2Int> shape = new List<Vector2Int>();

    [Header("Equipment")]
    public EquipmentType equipmentType = EquipmentType.None;

    [Min(0)]
    public int maxDurability = 0;

    public EquipmentStatModifier statModifier =
        new EquipmentStatModifier();

    [TextArea]
    public string weaponSkillDescription;

    public bool IsEquipment
    {
        get
        {
            return category == ItemCategory.Equipment &&
                   equipmentType != EquipmentType.None;
        }
    }

    public bool IsWeapon
    {
        get
        {
            return IsEquipment &&
                   equipmentType == EquipmentType.Weapon;
        }
    }

    public int GetSafeMaxDurability()
    {
        if (!IsEquipment)
            return 0;

        return Mathf.Max(1, maxDurability);
    }

    public void EnsureValidShape()
    {
        if (shape == null)
            shape = new List<Vector2Int>();

        if (shape.Count == 0)
            shape.Add(Vector2Int.zero);
    }
}