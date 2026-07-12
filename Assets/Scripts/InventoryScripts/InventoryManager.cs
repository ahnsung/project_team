using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Size")]
    public int width = 8;
    public int height = 8;

    [Header("Unlocked Area")]
    public int unlockedWidth = 8;
    public int unlockedHeight = 1;

    public List<InventoryItem> items =
        new List<InventoryItem>();

    private InventoryItem[,] grid;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        grid =
            new InventoryItem[
                width,
                height
            ];
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

        int capacity =
            PlayerStats.Instance
                .InventoryCapacity;

        int calculatedHeight =
            Mathf.CeilToInt(
                (float)capacity /
                Mathf.Max(1, width)
            );

        unlockedWidth =
            Mathf.Clamp(
                unlockedWidth,
                1,
                width
            );

        unlockedHeight =
            Mathf.Clamp(
                calculatedHeight,
                1,
                height
            );

        RefreshGrid();

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance
                .RebuildAndRefresh();
        }
    }

    public void AddTestBandage()
    {
        AddItem(1001);
    }

    public void AddTestLongBandage()
    {
        AddItem(1004);
    }

    public void AddTestMedKit()
    {
        AddItem(1007);
    }

    public void AddTestScrap()
    {
        AddItem(1005);
    }

    public void AddTestWeapon()
    {
        AddItem(2001);
    }

    public void AddTestArmor()
    {
        AddItem(2002);
    }

    public bool AddItem(int itemId)
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError(
                "ItemDatabase.Instance가 없습니다."
            );

            return false;
        }

        ItemData data =
            ItemDatabase.Instance
                .GetItem(itemId);

        if (data == null)
            return false;

        InventoryItem newItem =
            new InventoryItem(data);

        bool found =
            TryFindEmptyPosition(
                newItem,
                true,
                out Vector2Int position,
                out int rotation
            );

        if (!found)
        {
            Debug.Log(
                "인벤토리 공간이 부족합니다. " +
                "초과 칸은 다음 단계에서 연결합니다."
            );

            return false;
        }

        newItem.SetRotation(rotation);

        items.Add(newItem);

        PlaceItem(
            newItem,
            position
        );

        RefreshUI();

        return true;
    }

    public bool TryMoveItem(
        InventoryItem movingItem,
        Vector2Int targetPosition)
    {
        if (movingItem == null ||
            !items.Contains(movingItem))
        {
            return false;
        }

        Vector2Int originalPosition =
            movingItem.position;

        int originalRotation =
            movingItem.rotation;

        RemoveFromGrid(movingItem);

        List<InventoryItem> overlappedItems =
            GetOverlappedItems(
                movingItem,
                targetPosition
            );

        if (overlappedItems.Count == 0)
        {
            if (CanPlaceItem(
                movingItem,
                targetPosition,
                null))
            {
                PlaceItem(
                    movingItem,
                    targetPosition
                );

                RefreshUI();

                return true;
            }
        }
        else if (overlappedItems.Count == 1)
        {
            InventoryItem swapItem =
                overlappedItems[0];

            Vector2Int swapOriginalPosition =
                swapItem.position;

            int swapOriginalRotation =
                swapItem.rotation;

            RemoveFromGrid(swapItem);

            bool canMove =
                CanPlaceItem(
                    movingItem,
                    targetPosition,
                    null
                );

            bool canSwap =
                CanPlaceItem(
                    swapItem,
                    originalPosition,
                    null
                );

            if (canMove && canSwap)
            {
                PlaceItem(
                    movingItem,
                    targetPosition
                );

                PlaceItem(
                    swapItem,
                    originalPosition
                );

                RefreshUI();

                return true;
            }

            swapItem.SetRotation(
                swapOriginalRotation
            );

            PlaceItem(
                swapItem,
                swapOriginalPosition
            );
        }

        movingItem.SetRotation(
            originalRotation
        );

        PlaceItem(
            movingItem,
            originalPosition
        );

        RefreshUI();

        return false;
    }

    public bool CanPlaceItem(
        InventoryItem item,
        Vector2Int targetPosition,
        InventoryItem ignoreItem)
    {
        if (item == null)
            return false;

        foreach (
            Vector2Int cell
            in item.GetOccupiedCells(
                targetPosition
            ))
        {
            if (!IsInsideUnlocked(cell))
                return false;

            InventoryItem occupying =
                grid[cell.x, cell.y];

            if (occupying != null &&
                occupying != ignoreItem)
            {
                return false;
            }
        }

        return true;
    }

    public List<InventoryItem>
        GetOverlappedItems(
            InventoryItem movingItem,
            Vector2Int targetPosition)
    {
        List<InventoryItem> result =
            new List<InventoryItem>();

        foreach (
            Vector2Int cell
            in movingItem.GetOccupiedCells(
                targetPosition
            ))
        {
            if (!IsInside(cell))
                continue;

            InventoryItem other =
                grid[cell.x, cell.y];

            if (other != null &&
                other != movingItem &&
                !result.Contains(other))
            {
                result.Add(other);
            }
        }

        return result;
    }

    public void UseItem(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null ||
            item.data.IsEquipment)
        {
            return;
        }

        if (BattleManager.Instance != null &&
            BattleManager.Instance
                .IsBattleRunning() &&
            !BattleManager.Instance
                .CanPlayerUseItem())
        {
            Debug.Log(
                "지금은 플레이어 턴이 아니라 " +
                "아이템을 사용할 수 없습니다."
            );

            return;
        }

        ApplyItemEffect(item);

        item.remainUseCount--;

        if (item.remainUseCount <= 0)
        {
            RemoveItem(item);
        }
        else
        {
            RefreshUI();
        }

        if (BattleManager.Instance != null &&
            BattleManager.Instance
                .IsBattleRunning())
        {
            BattleManager.Instance
                .OnPlayerUsedItem();
        }
        else if (
            DungeonManager.Instance != null &&
            item.data.consumeTurnOnUse)
        {
            DungeonManager.Instance
                .AddTurn("아이템 사용");
        }
    }

    private void ApplyItemEffect(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null ||
            PlayerResourceManager.Instance == null)
        {
            return;
        }

        switch (item.data.id)
        {
            case 1001:
            case 1002:
            case 1003:
                PlayerResourceManager.Instance
                    .HealHealthItem(10);
                break;

            case 1004:
                PlayerResourceManager.Instance
                    .HealHungerItem(10);
                break;

            case 1007:
                PlayerResourceManager.Instance
                    .HealMentalItem(10);
                break;
        }
    }

    public void RemoveItem(
        InventoryItem item)
    {
        if (item == null)
            return;

        RemoveFromGrid(item);

        items.Remove(item);

        RefreshUI();
    }

    public bool ContainsItem(
        InventoryItem item)
    {
        return item != null &&
               items.Contains(item);
    }

    public bool TryTakeItemForEquipment(
        InventoryItem item)
    {
        if (!ContainsItem(item))
            return false;

        RemoveFromGrid(item);

        items.Remove(item);

        RefreshUI();

        return true;
    }

    public bool TryReturnEquipmentToInventory(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null)
        {
            return false;
        }

        if (items.Contains(item))
            return true;

        bool found =
            TryFindEmptyPosition(
                item,
                true,
                out Vector2Int position,
                out int rotation
            );

        if (!found)
            return false;

        item.SetRotation(rotation);

        items.Add(item);

        PlaceItem(
            item,
            position
        );

        RefreshUI();

        return true;
    }

    public bool TryRestoreItem(
        InventoryItem item,
        Vector2Int originalPosition,
        int originalRotation)
    {
        if (item == null)
            return false;

        item.SetRotation(
            originalRotation
        );

        if (!CanPlaceItem(
            item,
            originalPosition,
            null))
        {
            return false;
        }

        if (!items.Contains(item))
        {
            items.Add(item);
        }

        PlaceItem(
            item,
            originalPosition
        );

        RefreshUI();

        return true;
    }

    public bool TryFindEmptyPosition(
        InventoryItem item,
        bool allowRotation,
        out Vector2Int foundPosition,
        out int foundRotation)
    {
        foundPosition =
            Vector2Int.zero;

        foundRotation =
            item != null
                ? item.rotation
                : 0;

        if (item == null)
            return false;

        int originalRotation =
            item.rotation;

        int rotationCount =
            allowRotation
                ? 4
                : 1;

        for (
            int rotationIndex = 0;
            rotationIndex < rotationCount;
            rotationIndex++)
        {
            item.SetRotation(
                allowRotation
                    ? rotationIndex
                    : originalRotation
            );

            for (
                int y = 0;
                y < unlockedHeight;
                y++)
            {
                for (
                    int x = 0;
                    x < unlockedWidth;
                    x++)
                {
                    Vector2Int target =
                        new Vector2Int(
                            x,
                            y
                        );

                    if (!CanPlaceItem(
                        item,
                        target,
                        null))
                    {
                        continue;
                    }

                    foundPosition =
                        target;

                    foundRotation =
                        item.rotation;

                    item.SetRotation(
                        originalRotation
                    );

                    return true;
                }
            }
        }

        item.SetRotation(
            originalRotation
        );

        return false;
    }

    public InventoryItem FindFirstEquipment(
        EquipmentType equipmentType)
    {
        foreach (
            InventoryItem item
            in items)
        {
            if (item == null ||
                item.data == null ||
                !item.data.IsEquipment)
            {
                continue;
            }

            if (item.data.equipmentType ==
                equipmentType)
            {
                return item;
            }
        }

        return null;
    }

    public List<InventoryItem>
        FindEquipmentByType(
            EquipmentType equipmentType)
    {
        List<InventoryItem> result =
            new List<InventoryItem>();

        foreach (
            InventoryItem item
            in items)
        {
            if (item != null &&
                item.data != null &&
                item.data.IsEquipment &&
                item.data.equipmentType ==
                equipmentType)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private void PlaceItem(
        InventoryItem item,
        Vector2Int position)
    {
        item.position = position;

        foreach (
            Vector2Int cell
            in item.GetOccupiedCells(
                position
            ))
        {
            if (IsInside(cell))
            {
                grid[cell.x, cell.y] =
                    item;
            }
        }
    }

    private void RemoveFromGrid(
        InventoryItem item)
    {
        if (grid == null)
            return;

        for (
            int y = 0;
            y < height;
            y++)
        {
            for (
                int x = 0;
                x < width;
                x++)
            {
                if (grid[x, y] == item)
                {
                    grid[x, y] =
                        null;
                }
            }
        }
    }

    private void RefreshGrid()
    {
        grid =
            new InventoryItem[
                width,
                height
            ];

        foreach (
            InventoryItem item
            in items)
        {
            if (item == null)
                continue;

            if (CanPlaceItem(
                item,
                item.position,
                null))
            {
                PlaceItem(
                    item,
                    item.position
                );
            }
            else
            {
                Debug.LogWarning(
                    $"{item.data?.itemName}의 " +
                    "저장 위치가 현재 인벤토리 범위와 맞지 않습니다."
                );
            }
        }
    }

    private bool IsInside(
        Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < width &&
               cell.y >= 0 &&
               cell.y < height;
    }

    private bool IsInsideUnlocked(
        Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < unlockedWidth &&
               cell.y >= 0 &&
               cell.y < unlockedHeight;
    }

    public bool IsOverCapacity()
    {
        foreach (
            InventoryItem item
            in items)
        {
            foreach (
                Vector2Int cell
                in item.GetOccupiedCells(
                    item.position
                ))
            {
                if (!IsInsideUnlocked(cell))
                    return true;
            }
        }

        return false;
    }

    private void RefreshUI()
    {
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance
                .RefreshUI();
        }
    }
}