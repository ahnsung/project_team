using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string uniqueId;
    public ItemData data;
    public int remainUseCount;
    public Vector2Int position;
    public int rotation;

    public InventoryItem(ItemData data)
    {
        uniqueId = System.Guid.NewGuid().ToString();
        this.data = data;
        remainUseCount = data.maxUseCount;
        position = Vector2Int.zero;
        rotation = 0;
    }

    public List<Vector2Int> GetOccupiedCells(Vector2Int basePosition)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int offset in GetRotatedShape())
            result.Add(basePosition + offset);

        return result;
    }

    public List<Vector2Int> GetRotatedShape()
    {
        List<Vector2Int> rotated = new List<Vector2Int>();

        foreach (Vector2Int cell in data.shape)
        {
            Vector2Int p = cell;

            for (int i = 0; i < rotation; i++)
                p = new Vector2Int(p.y, -p.x);

            rotated.Add(p);
        }

        return NormalizeShape(rotated);
    }

    public void RotateClockwise()
    {
        rotation = (rotation + 1) % 4;
    }

    public void SetRotation(int value)
    {
        rotation = value;
    }

    private List<Vector2Int> NormalizeShape(List<Vector2Int> shape)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;

        foreach (Vector2Int cell in shape)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.y < minY) minY = cell.y;
        }

        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int cell in shape)
            result.Add(new Vector2Int(cell.x - minX, cell.y - minY));

        return result;
    }

    public Vector2Int GetShapeSize()
    {
        int maxX = 0;
        int maxY = 0;

        foreach (Vector2Int cell in GetRotatedShape())
        {
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y > maxY) maxY = cell.y;
        }

        return new Vector2Int(maxX + 1, maxY + 1);
    }
}