using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string uniqueId;
    public ItemData data;
    public int remainUseCount;
    public Vector2Int position;

    public InventoryItem(ItemData data)
    {
        this.uniqueId = System.Guid.NewGuid().ToString();
        this.data = data;
        this.remainUseCount = data.maxUseCount;
        this.position = Vector2Int.zero;
    }

    public List<Vector2Int> GetOccupiedCells(Vector2Int basePosition)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int offset in data.shape)
        {
            result.Add(basePosition + offset);
        }

        return result;
    }
}