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
        ApplyCapacityFromStats();
        RefreshGrid();
    }

    public void ApplyCapacityFromStats()
    {
        if (PlayerStats.Instance == null)
            return;

        int capacity = PlayerStats.Instance.InventoryCapacity;
        int calculatedHeight = Mathf.CeilToInt((float)capacity / width);

        unlockedHeight = Mathf.Clamp(calculatedHeight, 1, height);
    }

    public void AddTestBandage() => AddItem(1001);
    public void AddTestLongBandage() => AddItem(1004);
    public void AddTestMedKit() => AddItem(1007);
    public void AddTestScrap() => AddItem(1005);

    public bool AddItem(int itemId)
    {
        ItemData data = ItemDatabase.Instance.GetItem(itemId);
        if (data == null) return false;

        InventoryItem newItem = new InventoryItem(data);

        for (int y = 0; y < unlockedHeight; y++)
        {
            for (int x = 0; x < unlockedWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (CanPlaceItem(newItem, pos, null))
                {
                    items.Add(newItem);
                    PlaceItem(newItem, pos);

                    InventoryUIManager.Instance.RefreshUI();
                    return true;
                }
            }
        }

        Debug.Log("인벤토리 초과 상태. 추후 초과 칸 처리 예정.");
        return false;
    }

    public bool TryMoveItem(InventoryItem movingItem, Vector2Int targetPos)
    {
        if (movingItem == null) return false;

        Vector2Int originalPos = movingItem.position;

        RemoveFromGrid(movingItem);

        List<InventoryItem> overlappedItems = GetOverlappedItems(movingItem, targetPos);

        if (overlappedItems.Count == 0)
        {
            if (CanPlaceItem(movingItem, targetPos, null))
            {
                PlaceItem(movingItem, targetPos);
                InventoryUIManager.Instance.RefreshUI();
                return true;
            }
        }
        else if (overlappedItems.Count == 1)
        {
            InventoryItem swapItem = overlappedItems[0];
            Vector2Int swapOriginalPos = swapItem.position;

            RemoveFromGrid(swapItem);

            bool canMove = CanPlaceItem(movingItem, targetPos, null);
            bool canSwap = CanPlaceItem(swapItem, originalPos, null);

            if (canMove && canSwap)
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

    public bool CanPlaceItem(InventoryItem item, Vector2Int targetPos, InventoryItem ignoreItem)
    {
        foreach (Vector2Int cell in item.GetOccupiedCells(targetPos))
        {
            if (!IsInsideUnlocked(cell))
                return false;

            InventoryItem occupying = grid[cell.x, cell.y];

            if (occupying != null && occupying != ignoreItem)
                return false;
        }

        return true;
    }

    public List<InventoryItem> GetOverlappedItems(InventoryItem movingItem, Vector2Int targetPos)
    {
        List<InventoryItem> result = new List<InventoryItem>();

        foreach (Vector2Int cell in movingItem.GetOccupiedCells(targetPos))
        {
            if (!IsInside(cell))
                continue;

            InventoryItem other = grid[cell.x, cell.y];

            if (other != null && other != movingItem && !result.Contains(other))
                result.Add(other);
        }

        return result;
    }

    public void UseItem(InventoryItem item)
    {
        if (item == null) return;

        if (PlayerResourceManager.Instance != null)
        {
            if (item.data.id == 1001)
                PlayerResourceManager.Instance.HealHealthItem(10);

            else if (item.data.id == 1004)
                PlayerResourceManager.Instance.HealHungerItem(10);

            else if (item.data.id == 1007)
                PlayerResourceManager.Instance.HealMentalItem(10);

            else if (item.data.id == 1002)
                PlayerResourceManager.Instance.HealHealthItem(10);

            else if (item.data.id == 1003)
                PlayerResourceManager.Instance.HealHealthItem(10);
        }

        item.remainUseCount--;

        if (item.data.consumeTurnOnUse && DungeonManager.Instance != null)
            DungeonManager.Instance.AddTurn("아이템 사용");

        if (item.remainUseCount <= 0)
            RemoveItem(item);

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
            if (IsInside(cell))
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
            PlaceItem(item, item.position);
    }

    private bool IsInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    private bool IsInsideUnlocked(Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < unlockedWidth &&
               cell.y >= 0 &&
               cell.y < unlockedHeight;
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