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

    public bool canDrop = true;

    [TextArea]
    public string effectDescription;

    public Sprite icon;

    [Header("Inventory Shape")]
    public List<Vector2Int> shape =
        new List<Vector2Int>();

    [Header("Equipment")]
    public EquipmentType equipmentType =
        EquipmentType.None;

    [Min(0)]
    public int maxDurability;

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

    public int SafeMaxDurability
    {
        get
        {
            return IsEquipment
                ? Mathf.Max(1, maxDurability)
                : 0;
        }
    }

    public void EnsureValidShape()
    {
        if (shape == null)
            shape = new List<Vector2Int>();

        if (shape.Count == 0)
            shape.Add(Vector2Int.zero);
    }
}