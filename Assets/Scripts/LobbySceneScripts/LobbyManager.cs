using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public GameObject confirmDungeonPanel;

    void Start()
    {
        if (confirmDungeonPanel != null)
            confirmDungeonPanel.SetActive(false);
    }

    // Status
    public void OpenStatus()
    {
        Debug.Log("Open Status Window");
    }

    // Inventory
    public void OpenInventory()
    {
        Debug.Log("Open Inventory");
    }

    // Quest
    public void OpenQuest()
    {
        Debug.Log("Open Quest Window");
    }

    // Rest
    public void Rest()
    {
        Debug.Log("Player Rested");

        // 예시
        // PlayerHP = MaxHP
    }

    // Enter Dungeon
    public void EnterDungeon()
    {
        confirmDungeonPanel.SetActive(true);
    }

    public void ConfirmDungeon()
    {
        SceneManager.LoadScene("DungeonScene");
    }

    public void CancelDungeon()
    {
        confirmDungeonPanel.SetActive(false);
    }

    // Exit (저장 후 메뉴)
    public void ExitGame()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("MenuScene");
    }
}