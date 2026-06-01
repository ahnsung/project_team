using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler
{
    public Vector2Int cellPosition;
    public Image background;

    private Color normalColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    private Color lockedColor = new Color(0.05f, 0.12f, 0.25f, 1f);

    private bool isLocked;

    public void Init(Vector2Int pos)
    {
        cellPosition = pos;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (background == null)
            background = GetComponent<Image>();

        if (background == null) return;

        background.sprite = null;
        background.color = isLocked ? lockedColor : normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.SetHoverCell(cellPosition);
    }
}