using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Root")]
    public GameObject inventoryRoot;
    public Canvas canvas;

    [Header("Grid")]
    public RectTransform gridRoot;
    public GameObject slotPrefab;
    public GameObject itemPrefab;
    public float cellSize = 64f;

    [Header("Info Panel")]
    public GameObject infoPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemInfoText;
    public Button useButton;
    public Button dropButton;
    public Button closeInfoButton;

    [Header("Drop Popup")]
    public GameObject dropPopup;
    public TextMeshProUGUI dropPopupText;
    public Button confirmDropButton;
    public Button cancelDropButton;

    [Header("Notice Popup")]
    public GameObject noticePopup;
    public TextMeshProUGUI noticeText;
    public Button noticeConfirmButton;

    private InventoryState state = InventoryState.Closed;

    private InventoryItem selectedItem;
    private InventoryItem holdingItem;

    private readonly Dictionary<InventoryItem, InventoryItemUI> itemUIs = new Dictionary<InventoryItem, InventoryItemUI>();

    public Vector2Int CurrentHoverCell { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (dropPopup != null)
            dropPopup.SetActive(false);

        if (noticePopup != null)
            noticePopup.SetActive(false);

        if (useButton != null)
            useButton.onClick.AddListener(OnClickUse);

        if (dropButton != null)
            dropButton.onClick.AddListener(OnClickDrop);

        if (closeInfoButton != null)
            closeInfoButton.onClick.AddListener(CloseInfoPanel);

        if (confirmDropButton != null)
            confirmDropButton.onClick.AddListener(ConfirmDrop);

        if (cancelDropButton != null)
            cancelDropButton.onClick.AddListener(CancelDrop);

        if (noticeConfirmButton != null)
            noticeConfirmButton.onClick.AddListener(CloseNotice);

        BuildSlots();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (state == InventoryState.Closed)
                OpenInventory();
            else
                CloseInventory();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddTestBandage();
        }
    }

    public void OpenInventory()
    {
        if (inventoryRoot == null) return;

        inventoryRoot.SetActive(true);
        state = InventoryState.Default;
        RefreshUI();
    }

    public void CloseInventory()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);

        CloseInfoPanel();
        CloseDropPopup();
        CloseNotice();

        selectedItem = null;
        holdingItem = null;

        state = InventoryState.Closed;
    }

    private void BuildSlots()
    {
        if (gridRoot == null || slotPrefab == null) return;
        if (InventoryManager.Instance == null) return;

        for (int i = gridRoot.childCount - 1; i >= 0; i--)
            Destroy(gridRoot.GetChild(i).gameObject);

        int width = InventoryManager.Instance.width;
        int height = InventoryManager.Instance.height;

        float startX = -(width * cellSize) / 2f + cellSize / 2f;
        float startY = (height * cellSize) / 2f - cellSize / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridRoot);
                slotObj.name = "Slot_" + x + "_" + y;

                RectTransform rt = slotObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(startX + x * cellSize, startY - y * cellSize);

                InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();
                if (slot != null)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    slot.Init(cell);

                    bool locked =
                        x >= InventoryManager.Instance.unlockedWidth ||
                        y >= InventoryManager.Instance.unlockedHeight;

                    slot.SetLocked(locked);
                }
            }
        }
    }

    public void RefreshUI()
    {
        foreach (var pair in itemUIs)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        itemUIs.Clear();

        if (InventoryManager.Instance == null) return;

        foreach (InventoryItem item in InventoryManager.Instance.items)
            CreateItemUI(item);

        if (selectedItem != null && InventoryManager.Instance.items.Contains(selectedItem))
            UpdateInfoPanel();
        else
            selectedItem = null;
    }

    private void CreateItemUI(InventoryItem item)
    {
        if (gridRoot == null || itemPrefab == null || item == null) return;

        GameObject obj = Instantiate(itemPrefab, gridRoot);
        obj.name = "Item_" + item.data.itemName;

        RectTransform rt = obj.GetComponent<RectTransform>();

        int width = InventoryManager.Instance.width;
        int height = InventoryManager.Instance.height;

        float startX = -(width * cellSize) / 2f + cellSize / 2f;
        float startY = (height * cellSize) / 2f - cellSize / 2f;

        int maxX = 0;
        int maxY = 0;

        foreach (Vector2Int offset in item.data.shape)
        {
            if (offset.x > maxX) maxX = offset.x;
            if (offset.y > maxY) maxY = offset.y;
        }

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.sizeDelta = new Vector2((maxX + 1) * cellSize, (maxY + 1) * cellSize);

        rt.anchoredPosition = new Vector2(
            startX + item.position.x * cellSize,
            startY - item.position.y * cellSize
        );

        InventoryItemUI itemUI = obj.GetComponent<InventoryItemUI>();
        if (itemUI != null)
        {
            itemUI.Init(item, canvas);
            itemUIs[item] = itemUI;
        }
    }

    public void SelectItem(InventoryItem item)
    {
        if (item == null) return;
        if (state == InventoryState.ItemHolding) return;

        selectedItem = item;
        state = InventoryState.ItemSelected;

        foreach (var pair in itemUIs)
        {
            if (pair.Value != null)
                pair.Value.SetHighlight(pair.Key == item);
        }

        UpdateInfoPanel();

        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    private void UpdateInfoPanel()
    {
        if (selectedItem == null) return;

        if (itemNameText != null)
            itemNameText.text = selectedItem.data.itemName;

        if (itemInfoText != null)
        {
            string categoryText = selectedItem.data.category.ToString();

            itemInfoText.text =
                "남은 사용횟수: " + selectedItem.remainUseCount + "\n" +
                "[" + categoryText + "] 아이템\n" +
                "버리기 가능: " + (selectedItem.data.canDrop ? "O" : "X") + "\n" +
                "사용 시 턴 소모: " + (selectedItem.data.consumeTurnOnUse ? "O" : "X") + "\n" +
                "효과: " + selectedItem.data.effectDescription;
        }
    }

    public void StartHoldItem(InventoryItem item, InventoryItemUI ui)
    {
        holdingItem = item;
        state = InventoryState.ItemHolding;

        CloseInfoPanel();
    }

    public void EndHoldItem()
    {
        holdingItem = null;
        state = InventoryState.Default;
    }

    public void SetHoverCell(Vector2Int cell)
    {
        CurrentHoverCell = cell;
    }

    private void OnClickUse()
    {
        if (selectedItem == null) return;
        if (InventoryManager.Instance == null) return;

        InventoryItem usedItem = selectedItem;

        InventoryManager.Instance.UseItem(usedItem);

        if (!InventoryManager.Instance.items.Contains(usedItem))
        {
            selectedItem = null;
            CloseInfoPanel();
        }
        else
        {
            selectedItem = usedItem;
            UpdateInfoPanel();
        }
    }

    private void OnClickDrop()
    {
        if (selectedItem == null) return;

        if (!selectedItem.data.canDrop)
        {
            ShowNotice("이 아이템은\n버릴 수 없습니다.");
            CloseInfoPanel();
            return;
        }

        state = InventoryState.DropPopup;

        if (dropPopup != null)
            dropPopup.SetActive(true);

        if (dropPopupText != null)
            dropPopupText.text = "정말로 아이템을\n버리시겠습니까?";
    }

    private void ConfirmDrop()
    {
        if (selectedItem != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(selectedItem);
            selectedItem = null;
        }

        CloseDropPopup();
        CloseInfoPanel();

        state = InventoryState.Default;
    }

    private void CancelDrop()
    {
        CloseDropPopup();
        CloseInfoPanel();

        selectedItem = null;
        state = InventoryState.Default;
    }

    private void CloseInfoPanel()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        foreach (var pair in itemUIs)
        {
            if (pair.Value != null)
                pair.Value.SetHighlight(false);
        }

        if (state == InventoryState.ItemSelected)
            state = InventoryState.Default;
    }

    private void CloseDropPopup()
    {
        if (dropPopup != null)
            dropPopup.SetActive(false);
    }

    private void ShowNotice(string message)
    {
        if (noticePopup != null)
            noticePopup.SetActive(true);

        if (noticeText != null)
            noticeText.text = message;
    }

    private void CloseNotice()
    {
        if (noticePopup != null)
            noticePopup.SetActive(false);

        if (state != InventoryState.Closed)
            state = InventoryState.Default;
    }
}