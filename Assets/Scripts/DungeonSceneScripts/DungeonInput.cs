using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonInput : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayerPrefs.Save();
            SceneManager.LoadScene("LobbyScene");
        }
    }
}