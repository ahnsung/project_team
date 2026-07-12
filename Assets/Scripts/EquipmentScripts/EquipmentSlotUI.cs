using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField]
    private EquipmentSlotType slotType;

    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI durabilityText;

    [SerializeField]
    private Button slotButton;

    public EquipmentSlotType SlotType
    {
        get
        {
            return slotType;
        }
    }

    private void Awake()
    {
        if (slotButton == null)
            slotButton = GetComponent<Button>();
    }

    public void Refresh(InventoryItem item)
    {
        bool hasItem =
            item != null &&
            item.data != null;

        if (iconImage != null)
        {
            iconImage.enabled =
                hasItem &&
                item.data.icon != null;

            iconImage.sprite =
                hasItem
                    ? item.data.icon
                    : null;
        }

        if (nameText != null)
        {
            nameText.text =
                hasItem
                    ? item.data.itemName
                    : GetEmptySlotName();
        }

        if (durabilityText != null)
        {
            durabilityText.text =
                hasItem
                    ? "내구도: " +
                      item.currentDurability
                    : string.Empty;
        }
    }

    public void OnClickSlot()
    {
        if (EquipmentPanelUI.Instance != null)
        {
            EquipmentPanelUI.Instance
                .OpenSlotPopup(slotType);
        }
    }

    private string GetEmptySlotName()
    {
        switch (slotType)
        {
            case EquipmentSlotType.Head:
                return "머리";

            case EquipmentSlotType.Armor:
                return "갑옷";

            case EquipmentSlotType.Shoes:
                return "신발";

            case EquipmentSlotType.MainWeapon:
                return "주 무기";

            case EquipmentSlotType.SubWeapon:
                return "보조 무기";

            default:
                return "장비";
        }
    }
}