using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    // 기본 저장 키
    private const string HAS_SAVE_KEY = "HasSave";
    private const string CUTSCENE_PLAYED_KEY = "CutscenePlayed";
    private const string SELECTED_CHARACTER_KEY = "SelectedCharacter";
    private const string PLAYER_NAME_KEY = "PlayerName";

    // 던전 저장 키
    private const string ROOM_X_KEY = "ROOM_X";
    private const string ROOM_Y_KEY = "ROOM_Y";
    private const string VISITED_KEY = "VISITED";
    private const string TURN_KEY = "DUNGEON_TURN";
    private const string ENVIRONMENT_KEY = "DUNGEON_ENVIRONMENT";

    // 자원 저장 키
    private const string HEALTH_KEY = "PLAYER_HEALTH";
    private const string MENTAL_KEY = "PLAYER_MENTAL";
    private const string HUNGER_KEY = "PLAYER_HUNGER";
    private const string LAST_PROCESSED_TURN_KEY = "PLAYER_LAST_PROCESSED_TURN";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    // 새 저장 데이터 생성
    public void CreateNewSave(int characterID, string playerName)
    {
        PlayerPrefs.SetInt(HAS_SAVE_KEY, 1);
        PlayerPrefs.SetInt(CUTSCENE_PLAYED_KEY, 1);
        PlayerPrefs.SetInt(SELECTED_CHARACTER_KEY, characterID);
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);

        PlayerPrefs.Save();
    }

    // 저장 데이터 존재 여부
    public bool HasSave()
    {
        return PlayerPrefs.GetInt(HAS_SAVE_KEY, 0) == 1;
    }

    // 컷씬을 봤는지
    public bool HasPlayedCutscene()
    {
        return PlayerPrefs.GetInt(CUTSCENE_PLAYED_KEY, 0) == 1;
    }

    // 선택 캐릭터 가져오기
    public int GetSelectedCharacter()
    {
        return PlayerPrefs.GetInt(SELECTED_CHARACTER_KEY, -1);
    }

    // 이름 가져오기
    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
    }

    // 저장 데이터 삭제
    public void DeleteSave()
    {
        // 기본 저장 삭제
        PlayerPrefs.DeleteKey(HAS_SAVE_KEY);
        PlayerPrefs.DeleteKey(CUTSCENE_PLAYED_KEY);
        PlayerPrefs.DeleteKey(SELECTED_CHARACTER_KEY);
        PlayerPrefs.DeleteKey(PLAYER_NAME_KEY);

        // 던전 저장 삭제
        PlayerPrefs.DeleteKey(ROOM_X_KEY);
        PlayerPrefs.DeleteKey(ROOM_Y_KEY);
        PlayerPrefs.DeleteKey(VISITED_KEY);
        PlayerPrefs.DeleteKey(TURN_KEY);
        PlayerPrefs.DeleteKey(ENVIRONMENT_KEY);

        // 자원 저장 삭제
        PlayerPrefs.DeleteKey(HEALTH_KEY);
        PlayerPrefs.DeleteKey(MENTAL_KEY);
        PlayerPrefs.DeleteKey(HUNGER_KEY);
        PlayerPrefs.DeleteKey(LAST_PROCESSED_TURN_KEY);

        // 새 게임 컷씬 플래그도 정리
        PlayerPrefs.DeleteKey("AfterCutsceneGoToCharacterSelect");

        PlayerPrefs.Save();

        Debug.Log("세이브 / 던전 / 미니맵 / 턴 / 자원 데이터 초기화 완료");
    }
}