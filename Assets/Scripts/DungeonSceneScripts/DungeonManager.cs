using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;

    [Header("Map Size")]
    [SerializeField] private int mapWidth = 5;
    [SerializeField] private int mapHeight = 5;

    [Header("Start")]
    [SerializeField] private Vector2Int startRoom = new Vector2Int(2, 2);

    [Header("Refs")]
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private MinimapUIManager minimapUI;

    [Header("Info Text")]
    [SerializeField] private TextMeshProUGUI currentTurnText;
    [SerializeField] private TextMeshProUGUI currentEnvironmentText;

    [Header("Environment")]
    [SerializeField] private string currentEnvironment = "지하";

    private Vector2Int currentRoom;
    private HashSet<string> visited = new HashSet<string>();

    private int currentTurn = 0;

    private const string XKEY = "ROOM_X";
    private const string YKEY = "ROOM_Y";
    private const string VISITED = "VISITED";
    private const string TURN_KEY = "DUNGEON_TURN";
    private const string ENV_KEY = "DUNGEON_ENVIRONMENT";

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public Vector2Int CurrentRoom => currentRoom;
    public int CurrentTurn => currentTurn;
    public string CurrentEnvironment => currentEnvironment;

    public event Action<int> OnTurnChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Load();

        MarkVisited(currentRoom);
        Save();
    }

    private void Start()
    {
        RefreshAll();
        RefreshKoreanInfoText();

        Debug.Log($"[던전] 현재 턴: {currentTurn}");
        Debug.Log($"[던전] 현재 장소: {ConvertEnvironmentToKorean(currentEnvironment)}");

        OnTurnChanged?.Invoke(currentTurn);
    }

    private void LateUpdate()
    {
        RefreshKoreanInfoText();
    }

    public void MoveToNextRoom(MoveDirection dir)
    {
        Vector2Int next = currentRoom;

        if (dir == MoveDirection.Up) next += Vector2Int.up;
        if (dir == MoveDirection.Down) next += Vector2Int.down;
        if (dir == MoveDirection.Left) next += Vector2Int.left;
        if (dir == MoveDirection.Right) next += Vector2Int.right;

        if (!IsInside(next)) return;

        currentRoom = next;
        MarkVisited(currentRoom);

        AddTurn("장소 이동");

        Save();
        RefreshAll();
    }

    public void AddTurn(string reason)
    {
        currentTurn++;

        Debug.Log($"[던전 턴] 현재 턴: {currentTurn} / 행동: {reason}");

        Save();
        RefreshKoreanInfoText();
        OnTurnChanged?.Invoke(currentTurn);
    }

    public void SetEnvironment(string environment)
    {
        currentEnvironment = ConvertEnvironmentToKorean(environment);
        Save();
        RefreshKoreanInfoText();
    }

    public bool IsVisited(int x, int y)
    {
        return visited.Contains(x + "," + y);
    }

    public Dictionary<MoveDirection, bool> GetDirections()
    {
        return new Dictionary<MoveDirection, bool>()
        {
            { MoveDirection.Up, currentRoom.y < mapHeight - 1 },
            { MoveDirection.Down, currentRoom.y > 0 },
            { MoveDirection.Left, currentRoom.x > 0 },
            { MoveDirection.Right, currentRoom.x < mapWidth - 1 }
        };
    }

    public void RefreshAll()
    {
        if (uiManager != null)
            uiManager.RefreshDirectionButtons(GetDirections());

        if (minimapUI != null)
            minimapUI.RefreshMinimap();

        RefreshKoreanInfoText();
    }

    private void FindInfoTextsIfNull()
    {
        if (currentTurnText == null)
        {
            GameObject obj = GameObject.Find("CurrentTurnText");
            if (obj != null)
                currentTurnText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (currentEnvironmentText == null)
        {
            GameObject obj = GameObject.Find("CurrentEnvironmentText");
            if (obj != null)
                currentEnvironmentText = obj.GetComponent<TextMeshProUGUI>();
        }
    }

    private void RefreshKoreanInfoText()
    {
        FindInfoTextsIfNull();

        if (currentTurnText != null)
            currentTurnText.text = "현재 턴 : " + currentTurn;

        if (currentEnvironmentText != null)
            currentEnvironmentText.text = "현재 장소 : " + ConvertEnvironmentToKorean(currentEnvironment);
    }

    private string ConvertEnvironmentToKorean(string env)
    {
        switch (env)
        {
            case "Earth":
                return "지하";

            case "Ground":
                return "지상";

            case "Surface":
                return "지상";

            case "Ruins":
                return "폐허";

            case "Forest":
                return "숲";

            case "Lab":
                return "연구소";

            case "City":
                return "도시";

            case "Underground":
                return "지하";

            case "지하":
            case "지상":
            case "폐허":
            case "숲":
            case "연구소":
            case "도시":
                return env;

            default:
                return env;
        }
    }

    private void MarkVisited(Vector2Int r)
    {
        visited.Add(r.x + "," + r.y);
    }

    private bool IsInside(Vector2Int r)
    {
        return r.x >= 0 && r.x < mapWidth && r.y >= 0 && r.y < mapHeight;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(XKEY, currentRoom.x);
        PlayerPrefs.SetInt(YKEY, currentRoom.y);
        PlayerPrefs.SetInt(TURN_KEY, currentTurn);
        PlayerPrefs.SetString(ENV_KEY, ConvertEnvironmentToKorean(currentEnvironment));

        string merged = string.Join("|", visited);
        PlayerPrefs.SetString(VISITED, merged);

        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(XKEY))
        {
            currentRoom = new Vector2Int(
                PlayerPrefs.GetInt(XKEY),
                PlayerPrefs.GetInt(YKEY)
            );
        }
        else
        {
            currentRoom = startRoom;
        }

        currentTurn = PlayerPrefs.GetInt(TURN_KEY, currentTurn);
        currentEnvironment = ConvertEnvironmentToKorean(PlayerPrefs.GetString(ENV_KEY, currentEnvironment));

        visited.Clear();

        if (PlayerPrefs.HasKey(VISITED))
        {
            string data = PlayerPrefs.GetString(VISITED);
            string[] arr = data.Split('|');

            foreach (string s in arr)
            {
                if (!string.IsNullOrEmpty(s))
                    visited.Add(s);
            }
        }
    }
}