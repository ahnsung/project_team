using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject battleRoot;

    public GameObject battleMenuPanel;
    public GameObject actionMenuPanel;

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
}