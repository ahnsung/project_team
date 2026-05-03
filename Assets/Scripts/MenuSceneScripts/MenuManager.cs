using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject continueButton;
    public GameObject warningPanel;

    void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
            continueButton.SetActive(true);
        else
            continueButton.SetActive(false);

        if (warningPanel != null)
            warningPanel.SetActive(false);
    }

    public void ContinueGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }

    public void NewGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            warningPanel.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("AfterCutsceneGoToCharacterSelect", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene("StartScene");
        }
    }

    public void ConfirmNewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }
        else
        {
            // 혹시 SaveManager가 없으면 직접 삭제
            PlayerPrefs.DeleteKey("HasSave");
            PlayerPrefs.DeleteKey("CutscenePlayed");
            PlayerPrefs.DeleteKey("SelectedCharacter");
            PlayerPrefs.DeleteKey("PlayerName");
            PlayerPrefs.DeleteKey("ROOM_X");
            PlayerPrefs.DeleteKey("ROOM_Y");
            PlayerPrefs.DeleteKey("VISITED");
            PlayerPrefs.DeleteKey("AfterCutsceneGoToCharacterSelect");
            PlayerPrefs.Save();
        }

        PlayerPrefs.SetInt("AfterCutsceneGoToCharacterSelect", 1);
        PlayerPrefs.Save();

        warningPanel.SetActive(false);
        SceneManager.LoadScene("StartScene");
    }

    public void CancelNewGame()
    {
        warningPanel.SetActive(false);
    }
}