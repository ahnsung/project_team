using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler
{
    public Vector2Int cellPosition;
    public Image background;

    public void Init(Vector2Int pos)
    {
        cellPosition = pos;
    }

    public void SetLocked(bool locked)
    {
        if (background == null) return;

        if (locked)
            background.color = new Color(0.1f, 0.35f, 0.8f, 1f);
        else
            background.color = new Color(0.08f, 0.08f, 0.08f, 1f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryUIManager.Instance.SetHoverCell(cellPosition);
    }
}