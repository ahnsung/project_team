using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject battleRoot;
    public GameObject battleMenuPanel;
    public GameObject actionMenuPanel;

    private void Awake()
    {
        HideBattleUI();
    }

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

        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(false);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(false);
    }

    public void ShowMainBattleMenu()
    {
        if (battleRoot != null)
            battleRoot.SetActive(true);

        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(true);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(false);
    }

    public void ShowActionMenu()
    {
        if (battleRoot != null)
            battleRoot.SetActive(true);

        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(false);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(true);
    }
}