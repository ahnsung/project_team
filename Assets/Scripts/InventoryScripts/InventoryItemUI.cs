using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Image iconImage;
    public Image highlightImage;

    private InventoryItem item;
    private RectTransform rectTransform;
    private Canvas canvas;

    private bool isPointerDown;
    private bool isDragging;
    private float pointerDownTime;

    private const float holdTime = 0.3f;

    public void Init(InventoryItem item, Canvas canvas)
    {
        this.item = item;
        this.canvas = canvas;

        rectTransform = GetComponent<RectTransform>();

        if (iconImage != null)
            iconImage.sprite = item.data.icon;

        SetHighlight(false);
    }

    private void Update()
    {
        if (!isPointerDown) return;
        if (isDragging) return;

        if (Time.time - pointerDownTime >= holdTime)
        {
            StartDragging();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown) return;

        isPointerDown = false;

        if (isDragging)
        {
            EndDragging();
        }
        else
        {
            InventoryUIManager.Instance.SelectItem(item);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    private void StartDragging()
    {
        isDragging = true;
        InventoryUIManager.Instance.StartHoldItem(item, this);
        transform.SetAsLastSibling();
    }

    private void EndDragging()
    {
        isDragging = false;

        Vector2Int targetCell = InventoryUIManager.Instance.CurrentHoverCell;
        InventoryManager.Instance.TryMoveItem(item, targetCell);

        InventoryUIManager.Instance.EndHoldItem();
    }

    public void SetHighlight(bool value)
    {
        if (highlightImage != null)
            highlightImage.gameObject.SetActive(value);
    }
}