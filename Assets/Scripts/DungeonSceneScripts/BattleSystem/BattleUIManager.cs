using UnityEngine;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject battleRoot;
    public GameObject battleMenuPanel;
    public GameObject actionMenuPanel;

    [Header("Texts")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI messageText;

    public void ShowBattleUI()
    {
        if (battleRoot != null)
            battleRoot.SetActive(true);

        ShowMainBattleMenu();
    }

    public void HideBattleUI()
    {
        if (battleRoot != null)
            battleRoot.SetActive(false);
    }

    public void ShowMainBattleMenu()
    {
        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(true);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(false);
    }

    public void ShowActionMenu()
    {
        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(false);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(true);
    }

    public void SetTurnText(int turn)
    {
        if (turnText != null)
            turnText.text = "Current Turn : " + turn;
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}