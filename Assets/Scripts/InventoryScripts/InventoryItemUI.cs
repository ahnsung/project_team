using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler
{
    public Image iconImage;
    public Image highlightImage;

    private InventoryItem item;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 originalAnchoredPosition;
    private bool didDrag;

    public void Init(InventoryItem item, Canvas canvas)
    {
        this.item = item;
        this.canvas = canvas;

        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (iconImage != null)
        {
            iconImage.sprite = item.data.icon;
            iconImage.raycastTarget = false;
        }

        if (highlightImage != null)
        {
            highlightImage.raycastTarget = false;
            highlightImage.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        didDrag = false;
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        didDrag = true;

        InventoryUIManager.Instance.StartHoldItem(item, this);

        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.75f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        didDrag = true;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Vector2Int targetCell = InventoryUIManager.Instance.ScreenToCell(eventData.position);

        bool success = InventoryManager.Instance.TryMoveItem(item, targetCell);

        if (!success)
            rectTransform.anchoredPosition = originalAnchoredPosition;

        InventoryUIManager.Instance.EndHoldItem();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (didDrag) return;

        InventoryUIManager.Instance.SelectItem(item);
    }

    public void SetHighlight(bool value)
    {
        if (highlightImage != null)
            highlightImage.gameObject.SetActive(value);
    }
}