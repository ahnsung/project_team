using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanelUI : MonoBehaviour
{
    public static EquipmentPanelUI Instance;

    [Header("Slots")]
    [SerializeField]
    private EquipmentSlotUI headSlot;

    [SerializeField]
    private EquipmentSlotUI armorSlot;

    [SerializeField]
    private EquipmentSlotUI shoesSlot;

    [SerializeField]
    private EquipmentSlotUI mainWeaponSlot;

    [SerializeField]
    private EquipmentSlotUI subWeaponSlot;

    [Header("Popup")]
    [SerializeField]
    private GameObject popupRoot;

    [SerializeField]
    private TextMeshProUGUI popupTitleText;

    [SerializeField]
    private TextMeshProUGUI currentEquipmentText;

    [SerializeField]
    private RectTransform candidateRoot;

    [SerializeField]
    private Button candidateButtonPrefab;

    [SerializeField]
    private Button unequipButton;

    [SerializeField]
    private Button closeButton;

    private EquipmentSlotType selectedSlot;

    private readonly List<GameObject>
        candidateObjects =
            new List<GameObject>();

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
        if (popupRoot != null)
            popupRoot.SetActive(false);

        if (unequipButton != null)
        {
            unequipButton.onClick
                .RemoveAllListeners();

            unequipButton.onClick
                .AddListener(OnClickUnequip);
        }

        if (closeButton != null)
        {
            closeButton.onClick
                .RemoveAllListeners();

            closeButton.onClick
                .AddListener(ClosePopup);
        }

        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance
                .OnEquipmentChanged += RefreshAll;
        }

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance
                .OnEquipmentChanged -= RefreshAll;
        }

        if (Instance == this)
            Instance = null;
    }

    public void RefreshAll()
    {
        if (EquipmentManager.Instance == null)
            return;

        if (headSlot != null)
        {
            headSlot.Refresh(
                EquipmentManager.Instance.Head
            );
        }

        if (armorSlot != null)
        {
            armorSlot.Refresh(
                EquipmentManager.Instance.Armor
            );
        }

        if (shoesSlot != null)
        {
            shoesSlot.Refresh(
                EquipmentManager.Instance.Shoes
            );
        }

        if (mainWeaponSlot != null)
        {
            mainWeaponSlot.Refresh(
                EquipmentManager.Instance.MainWeapon
            );
        }

        if (subWeaponSlot != null)
        {
            subWeaponSlot.Refresh(
                EquipmentManager.Instance.SubWeapon
            );
        }

        if (popupRoot != null &&
            popupRoot.activeSelf)
        {
            RefreshPopup();
        }
    }

    public void OpenSlotPopup(
        EquipmentSlotType slot)
    {
        selectedSlot = slot;

        if (popupRoot != null)
            popupRoot.SetActive(true);

        RefreshPopup();
    }

    public void ClosePopup()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);

        ClearCandidates();
    }

    public void OnClickSwapWeapons()
    {
        if (EquipmentManager.Instance == null)
            return;

        bool swapped =
            EquipmentManager.Instance
                .SwapWeapons();

        if (!swapped &&
            InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance
                .ShowNotice(
                    "현재는 무기를 스왑할 수 없습니다."
                );
        }
    }

    private void RefreshPopup()
    {
        if (EquipmentManager.Instance == null ||
            InventoryManager.Instance == null)
        {
            return;
        }

        InventoryItem current =
            EquipmentManager.Instance
                .GetEquippedItem(selectedSlot);

        if (popupTitleText != null)
        {
            popupTitleText.text =
                GetSlotName(selectedSlot) +
                " 장비";
        }

        if (currentEquipmentText != null)
        {
            if (current == null)
            {
                currentEquipmentText.text =
                    "현재 장비: 없음";
            }
            else
            {
                currentEquipmentText.text =
                    "현재 장비: " +
                    current.data.itemName +
                    " | 내구도: " +
                    current.currentDurability;
            }
        }

        if (unequipButton != null)
        {
            unequipButton.interactable =
                current != null;
        }

        RebuildCandidates();
    }

    private void RebuildCandidates()
    {
        ClearCandidates();

        if (candidateRoot == null ||
            candidateButtonPrefab == null ||
            EquipmentManager.Instance == null ||
            InventoryManager.Instance == null)
        {
            return;
        }

        EquipmentType equipmentType =
            EquipmentManager.Instance
                .GetEquipmentTypeForSlot(
                    selectedSlot
                );

        List<InventoryItem> candidates =
            InventoryManager.Instance
                .FindEquipmentByType(
                    equipmentType
                );

        foreach (InventoryItem candidate in candidates)
        {
            if (candidate == null ||
                candidate.data == null)
            {
                continue;
            }

            Button button =
                Instantiate(
                    candidateButtonPrefab,
                    candidateRoot
                );

            candidateObjects.Add(
                button.gameObject
            );

            TextMeshProUGUI text =
                button.GetComponentInChildren
                    <TextMeshProUGUI>();

            if (text != null)
            {
                text.text =
                    candidate.data.itemName +
                    " | 내구도: " +
                    candidate.currentDurability;
            }

            InventoryItem capturedItem =
                candidate;

            button.onClick
                .RemoveAllListeners();

            button.onClick.AddListener(
                () =>
                    OnClickCandidate(
                        capturedItem
                    )
            );
        }
    }

    private void OnClickCandidate(
        InventoryItem item)
    {
        if (EquipmentManager.Instance == null)
            return;

        bool equipped =
            EquipmentManager.Instance
                .EquipToSlot(
                    item,
                    selectedSlot
                );

        if (!equipped)
        {
            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance
                    .ShowNotice(
                        "해당 장비로 교체할 수 없습니다."
                    );
            }

            return;
        }

        RefreshAll();
    }

    private void OnClickUnequip()
    {
        if (EquipmentManager.Instance == null)
            return;

        bool unequipped =
            EquipmentManager.Instance
                .Unequip(selectedSlot);

        if (!unequipped)
        {
            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance
                    .ShowNotice(
                        "장비를 해제할 공간이 없습니다."
                    );
            }

            return;
        }

        RefreshAll();
    }

    private void ClearCandidates()
    {
        foreach (GameObject candidateObject
                 in candidateObjects)
        {
            if (candidateObject != null)
                Destroy(candidateObject);
        }

        candidateObjects.Clear();
    }

    private string GetSlotName(
        EquipmentSlotType slot)
    {
        switch (slot)
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