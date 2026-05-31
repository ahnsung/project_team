using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public ItemCategory category;

    public int maxUseCount;
    public bool consumeTurnOnUse;
    public bool canDrop;

    [TextArea]
    public string effectDescription;

    public Sprite icon;

    // 아이템이 차지하는 칸 모양
    // 예: 1칸 = (0,0)
    // 예: 세로 2칸 = (0,0), (0,1)
    public List<Vector2Int> shape = new List<Vector2Int>();
}