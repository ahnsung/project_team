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
        inventoryRoot.SetActive(false);
        infoPanel.SetActive(false);
        dropPopup.SetActive(false);
        noticePopup.SetActive(false);

        useButton.onClick.AddListener(OnClickUse);
        dropButton.onClick.AddListener(OnClickDrop);
        closeInfoButton.onClick.AddListener(CloseInfoPanel);
        confirmDropButton.onClick.AddListener(ConfirmDrop);
        cancelDropButton.onClick.AddListener(CancelDrop);
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
            InventoryManager.Instance.AddTestBandage();

        if (Input.GetKeyDown(KeyCode.N))
            InventoryManager.Instance.AddTestLongBandage();

        if (Input.GetKeyDown(KeyCode.M))
            InventoryManager.Instance.AddTestMedKit();

        if (Input.GetKeyDown(KeyCode.G))
            InventoryManager.Instance.AddTestScrap();
    }

    public void OpenInventory()
    {
        inventoryRoot.SetActive(true);
        state = InventoryState.Default;
        RefreshUI();
    }

    public void CloseInventory()
    {
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
                slot.Init(new Vector2Int(x, y));

                bool locked =
                    x >= InventoryManager.Instance.unlockedWidth ||
                    y >= InventoryManager.Instance.unlockedHeight;

                slot.SetLocked(locked);
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

        foreach (InventoryItem item in InventoryManager.Instance.items)
            CreateItemUI(item);

        if (selectedItem != null && InventoryManager.Instance.items.Contains(selectedItem))
            UpdateInfoPanel();
        else
            selectedItem = null;
    }

    private void CreateItemUI(InventoryItem item)
    {
        GameObject obj = Instantiate(itemPrefab, gridRoot);
        obj.name = "Item_" + item.data.itemName;

        RectTransform rt = obj.GetComponent<RectTransform>();

        int width = InventoryManager.Instance.width;
        int height = InventoryManager.Instance.height;

        float startX = -(width * cellSize) / 2f + cellSize / 2f;
        float startY = (height * cellSize) / 2f - cellSize / 2f;

        Vector2Int shapeSize = item.GetShapeSize();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(shapeSize.x * cellSize, shapeSize.y * cellSize);

        rt.anchoredPosition = new Vector2(
            startX + item.position.x * cellSize + ((shapeSize.x - 1) * cellSize / 2f),
            startY - item.position.y * cellSize - ((shapeSize.y - 1) * cellSize / 2f)
        );

        InventoryItemUI itemUI = obj.GetComponent<InventoryItemUI>();
        itemUI.Init(item, canvas);

        itemUIs[item] = itemUI;
    }

    public Vector2Int ScreenToCell(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRoot,
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        int width = InventoryManager.Instance.width;
        int height = InventoryManager.Instance.height;

        float startX = -(width * cellSize) / 2f;
        float startY = (height * cellSize) / 2f;

        int x = Mathf.FloorToInt((localPoint.x - startX) / cellSize);
        int y = Mathf.FloorToInt((startY - localPoint.y) / cellSize);

        return new Vector2Int(x, y);
    }

    public void SelectItem(InventoryItem item)
    {
        if (state == InventoryState.ItemHolding) return;

        selectedItem = item;
        state = InventoryState.ItemSelected;

        foreach (var pair in itemUIs)
            pair.Value.SetHighlight(pair.Key == item);

        UpdateInfoPanel();
        infoPanel.SetActive(true);
    }

    private void UpdateInfoPanel()
    {
        if (selectedItem == null) return;

        itemNameText.text = selectedItem.data.itemName;

        itemInfoText.text =
            "남은 사용횟수: " + selectedItem.remainUseCount + "\n" +
            "[" + selectedItem.data.category + "] 아이템\n" +
            "버리기 가능: " + (selectedItem.data.canDrop ? "O" : "X") + "\n" +
            "사용 시 턴 소모: " + (selectedItem.data.consumeTurnOnUse ? "O" : "X") + "\n" +
            "회전: R 키\n" +
            "효과: " + selectedItem.data.effectDescription;
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
        dropPopup.SetActive(true);
        dropPopupText.text = "정말로 아이템을\n버리시겠습니까?";
    }

    private void ConfirmDrop()
    {
        if (selectedItem != null)
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
        infoPanel.SetActive(false);

        foreach (var pair in itemUIs)
            pair.Value.SetHighlight(false);

        if (state == InventoryState.ItemSelected)
            state = InventoryState.Default;
    }

    private void CloseDropPopup()
    {
        dropPopup.SetActive(false);
    }

    private void ShowNotice(string message)
    {
        noticePopup.SetActive(true);
        noticeText.text = message;
    }

    private void CloseNotice()
    {
        noticePopup.SetActive(false);

        if (state != InventoryState.Closed)
            state = InventoryState.Default;
    }
}