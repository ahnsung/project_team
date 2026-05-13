using TMPro;
using UnityEngine;

public class DungeonTurnManager : MonoBehaviour
{
    public static DungeonTurnManager Instance;

    [Header("Turn")]
    public int currentTurn = 1;

    [Header("UI")]
    public TextMeshProUGUI currentTurnText;

    private void Awake()
    {
        Instance = this;
        RefreshUI();
    }

    public void AddTurn(int amount = 1)
    {
        currentTurn += amount;
        RefreshUI();
        Debug.Log("[Dungeon Turn] 현재 턴 : " + currentTurn);
    }

    public void RefreshUI()
    {
        if (currentTurnText != null)
            currentTurnText.text = "Current Turn : " + currentTurn;
    }
}