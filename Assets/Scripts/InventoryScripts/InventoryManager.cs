using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Size")]
    public int width = 8;
    public int height = 5;

    [Header("Unlocked Area")]
    public int unlockedWidth = 8;
    public int unlockedHeight = 4;

    public List<InventoryItem> items = new List<InventoryItem>();

    private InventoryItem[,] grid;

    private void Awake()
    {
        Instance = this;
        grid = new InventoryItem[width, height];
    }

    private void Start()
    {
        RefreshGrid();
    }

    public void AddTestBandage()
    {
        AddItem(1001);
    }

    public bool AddItem(int itemId)
    {
        ItemData data = ItemDatabase.Instance.GetItem(itemId);
        if (data == null) return false;

        InventoryItem newItem = new InventoryItem(data);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (CanPlaceItem(newItem, pos, null))
                {
                    PlaceItem(newItem, pos);
                    items.Add(newItem);

                    InventoryUIManager.Instance.RefreshUI();
                    return true;
                }
            }
        }

        Debug.Log("인벤토리 초과 상태. 나중에 초과 칸 처리 필요.");
        return false;
    }

    public bool CanPlaceItem(InventoryItem item, Vector2Int targetPos, InventoryItem ignoreItem)
    {
        List<Vector2Int> cells = item.GetOccupiedCells(targetPos);

        foreach (Vector2Int cell in cells)
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
                return false;

            if (cell.x >= unlockedWidth || cell.y >= unlockedHeight)
                return false;

            InventoryItem occupying = grid[cell.x, cell.y];

            if (occupying != null && occupying != ignoreItem)
                return false;
        }

        return true;
    }

    public InventoryItem GetItemAt(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return null;

        return grid[cell.x, cell.y];
    }

    public List<InventoryItem> GetOverlappedItems(InventoryItem movingItem, Vector2Int targetPos)
    {
        List<InventoryItem> result = new List<InventoryItem>();

        foreach (Vector2Int cell in movingItem.GetOccupiedCells(targetPos))
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
                continue;

            InventoryItem other = grid[cell.x, cell.y];

            if (other != null && other != movingItem && !result.Contains(other))
                result.Add(other);
        }

        return result;
    }

    public bool TryMoveItem(InventoryItem movingItem, Vector2Int targetPos)
    {
        Vector2Int originalPos = movingItem.position;

        RemoveFromGrid(movingItem);

        List<InventoryItem> overlapped = GetOverlappedItems(movingItem, targetPos);

        if (overlapped.Count == 0)
        {
            if (CanPlaceItem(movingItem, targetPos, movingItem))
            {
                PlaceItem(movingItem, targetPos);
                InventoryUIManager.Instance.RefreshUI();
                return true;
            }
        }
        else if (overlapped.Count == 1)
        {
            InventoryItem swapItem = overlapped[0];
            Vector2Int swapOriginalPos = swapItem.position;

            RemoveFromGrid(swapItem);

            bool canMovingGoTarget = CanPlaceItem(movingItem, targetPos, movingItem);
            bool canSwapGoOriginal = CanPlaceItem(swapItem, originalPos, swapItem);

            if (canMovingGoTarget && canSwapGoOriginal)
            {
                PlaceItem(movingItem, targetPos);
                PlaceItem(swapItem, originalPos);

                InventoryUIManager.Instance.RefreshUI();
                return true;
            }

            PlaceItem(swapItem, swapOriginalPos);
        }

        PlaceItem(movingItem, originalPos);
        InventoryUIManager.Instance.RefreshUI();
        return false;
    }

    public void UseItem(InventoryItem item)
    {
        if (item == null) return;

        if (item.data.id == 1001)
        {
            Debug.Log("붕대 사용: 체력 2 회복");
        }

        item.remainUseCount--;

        if (item.data.consumeTurnOnUse)
        {
            Debug.Log("아이템 사용으로 턴 +1");
            // 나중에 CurrentTurnManager 있으면 여기 연결
            // CurrentTurnManager.Instance.AddTurn(1);
        }

        if (item.remainUseCount <= 0)
        {
            RemoveItem(item);
        }

        InventoryUIManager.Instance.RefreshUI();
    }

    public void RemoveItem(InventoryItem item)
    {
        if (item == null) return;

        RemoveFromGrid(item);
        items.Remove(item);

        InventoryUIManager.Instance.RefreshUI();
    }

    private void PlaceItem(InventoryItem item, Vector2Int pos)
    {
        item.position = pos;

        foreach (Vector2Int cell in item.GetOccupiedCells(pos))
        {
            grid[cell.x, cell.y] = item;
        }
    }

    private void RemoveFromGrid(InventoryItem item)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == item)
                    grid[x, y] = null;
            }
        }
    }

    private void RefreshGrid()
    {
        grid = new InventoryItem[width, height];

        foreach (InventoryItem item in items)
        {
            PlaceItem(item, item.position);
        }
    }

    public bool IsOverCapacity()
    {
        foreach (InventoryItem item in items)
        {
            foreach (Vector2Int cell in item.GetOccupiedCells(item.position))
            {
                if (cell.x >= unlockedWidth || cell.y >= unlockedHeight)
                    return true;
            }
        }

        return false;
    }
}