using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler
{
    public Vector2Int cellPosition;
    public Image background;

    private Color normalColor = new Color(0.06f, 0.06f, 0.06f, 1f);
    private Color lockedColor = new Color(0.08f, 0.12f, 0.18f, 1f);

    public void Init(Vector2Int pos)
    {
        cellPosition = pos;
    }

    public void SetLocked(bool locked)
    {
        if (background == null)
            background = GetComponent<Image>();

        if (background == null) return;

        background.sprite = null;
        background.color = locked ? lockedColor : normalColor;

        Outline outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = new Color(0.45f, 0.65f, 0.65f, 0.9f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.SetHoverCell(cellPosition);
    }
}