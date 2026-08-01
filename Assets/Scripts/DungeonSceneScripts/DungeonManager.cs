using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;

    [Header("Map Size")]
    [SerializeField]
    private int mapWidth = 5;

    [SerializeField]
    private int mapHeight = 5;

    [Header("Start")]
    [SerializeField]
    private Vector2Int startRoom =
        new Vector2Int(2, 2);

    [Header("Refs")]
    [SerializeField]
    private DungeonUIManager uiManager;

    [SerializeField]
    private MinimapUIManager minimapUI;

    [Header("Info Text")]
    [SerializeField]
    private TextMeshProUGUI currentTurnText;

    [SerializeField]
    private TextMeshProUGUI currentEnvironmentText;

    [Header("Environment")]
    [SerializeField]
    private string currentEnvironment = "지하";

    private const string DEFAULT_ENVIRONMENT =
        "지하";

    private Vector2Int currentRoom;

    private readonly HashSet<string> visited =
        new HashSet<string>();

    private int currentTurn;

    private const string XKEY =
        "ROOM_X";

    private const string YKEY =
        "ROOM_Y";

    private const string VISITED =
        "VISITED";

    private const string TURN_KEY =
        "DUNGEON_TURN";

    private const string ENV_KEY =
        "DUNGEON_ENVIRONMENT";

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;

    public Vector2Int CurrentRoom =>
        currentRoom;

    public int CurrentTurn =>
        currentTurn;

    public string CurrentEnvironment =>
        currentEnvironment;

    public event Action<int> OnTurnChanged;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Load();

        MarkVisited(currentRoom);
        Save();
    }

    private void Start()
    {
        RefreshAll();

        Debug.Log(
            $"[던전] 현재 턴: {currentTurn}"
        );

        Debug.Log(
            "[던전] 현재 장소: " +
            ConvertEnvironmentToKorean(
                currentEnvironment
            )
        );

        OnTurnChanged?.Invoke(
            currentTurn
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void MoveToNextRoom(
        MoveDirection direction)
    {
        Vector2Int next =
            currentRoom;

        switch (direction)
        {
            case MoveDirection.Up:
                next += Vector2Int.up;
                break;

            case MoveDirection.Down:
                next += Vector2Int.down;
                break;

            case MoveDirection.Left:
                next += Vector2Int.left;
                break;

            case MoveDirection.Right:
                next += Vector2Int.right;
                break;
        }

        if (!IsInside(next))
            return;

        currentRoom =
            next;

        MarkVisited(
            currentRoom
        );

        AddTurn(
            "장소 이동"
        );

        RefreshAll();
    }

    public void AddTurn(
        string reason)
    {
        currentTurn++;

        Debug.Log(
            $"[던전 턴] 현재 턴: {currentTurn} " +
            $"/ 행동: {reason}"
        );

        Save();
        RefreshKoreanInfoText();

        OnTurnChanged?.Invoke(
            currentTurn
        );
    }

    public void SetEnvironment(
        string environment)
    {
        currentEnvironment =
            ConvertEnvironmentToKorean(
                environment
            );

        Save();
        RefreshKoreanInfoText();
    }

    public bool IsVisited(
        int x,
        int y)
    {
        return visited.Contains(
            GetRoomKey(x, y)
        );
    }

    public Dictionary<MoveDirection, bool>
        GetDirections()
    {
        return new Dictionary<
            MoveDirection,
            bool
        >
        {
            {
                MoveDirection.Up,
                currentRoom.y < mapHeight - 1
            },
            {
                MoveDirection.Down,
                currentRoom.y > 0
            },
            {
                MoveDirection.Left,
                currentRoom.x > 0
            },
            {
                MoveDirection.Right,
                currentRoom.x < mapWidth - 1
            }
        };
    }

    public void RefreshAll()
    {
        if (uiManager != null)
        {
            uiManager.RefreshDirectionButtons(
                GetDirections()
            );
        }

        if (minimapUI != null)
        {
            minimapUI.RefreshMinimap();
        }

        RefreshKoreanInfoText();
    }

    public void ResetForNewGame(
        bool saveData = true)
    {
        currentRoom =
            ClampRoomToMap(
                startRoom
            );

        currentTurn =
            0;

        currentEnvironment =
            DEFAULT_ENVIRONMENT;

        visited.Clear();

        MarkVisited(
            currentRoom
        );

        if (saveData)
        {
            Save();
        }

        RefreshAll();

        OnTurnChanged?.Invoke(
            currentTurn
        );

        Debug.Log(
            "[DungeonManager] 새 게임 초기화 완료 " +
            $"/ 시작 방: {currentRoom} " +
            $"/ 턴: {currentTurn} " +
            $"/ 환경: {currentEnvironment}"
        );
    }

    private void FindInfoTextsIfNull()
    {
        if (currentTurnText == null)
        {
            GameObject turnObject =
                GameObject.Find(
                    "CurrentTurnText"
                );

            if (turnObject != null)
            {
                currentTurnText =
                    turnObject.GetComponent<
                        TextMeshProUGUI
                    >();
            }
        }

        if (currentEnvironmentText == null)
        {
            GameObject environmentObject =
                GameObject.Find(
                    "CurrentEnvironmentText"
                );

            if (environmentObject != null)
            {
                currentEnvironmentText =
                    environmentObject.GetComponent<
                        TextMeshProUGUI
                    >();
            }
        }
    }

    private void RefreshKoreanInfoText()
    {
        FindInfoTextsIfNull();

        if (currentTurnText != null)
        {
            currentTurnText.text =
                "현재 턴 : " +
                currentTurn;
        }

        if (currentEnvironmentText != null)
        {
            currentEnvironmentText.text =
                "현재 장소 : " +
                ConvertEnvironmentToKorean(
                    currentEnvironment
                );
        }
    }

    private string ConvertEnvironmentToKorean(
        string environment)
    {
        switch (environment)
        {
            case "Earth":
            case "Underground":
                return "지하";

            case "Ground":
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

            case "지하":
            case "지상":
            case "폐허":
            case "숲":
            case "연구소":
            case "도시":
                return environment;

            default:
                return string.IsNullOrEmpty(
                    environment
                )
                    ? DEFAULT_ENVIRONMENT
                    : environment;
        }
    }

    private void MarkVisited(
        Vector2Int room)
    {
        visited.Add(
            GetRoomKey(
                room.x,
                room.y
            )
        );
    }

    private string GetRoomKey(
        int x,
        int y)
    {
        return x + "," + y;
    }

    private bool IsInside(
        Vector2Int room)
    {
        return
            room.x >= 0 &&
            room.x < mapWidth &&
            room.y >= 0 &&
            room.y < mapHeight;
    }

    private Vector2Int ClampRoomToMap(
        Vector2Int room)
    {
        int safeWidth =
            Mathf.Max(1, mapWidth);

        int safeHeight =
            Mathf.Max(1, mapHeight);

        return new Vector2Int(
            Mathf.Clamp(
                room.x,
                0,
                safeWidth - 1
            ),
            Mathf.Clamp(
                room.y,
                0,
                safeHeight - 1
            )
        );
    }

    private void Save()
    {
        PlayerPrefs.SetInt(
            XKEY,
            currentRoom.x
        );

        PlayerPrefs.SetInt(
            YKEY,
            currentRoom.y
        );

        PlayerPrefs.SetInt(
            TURN_KEY,
            currentTurn
        );

        PlayerPrefs.SetString(
            ENV_KEY,
            ConvertEnvironmentToKorean(
                currentEnvironment
            )
        );

        string merged =
            string.Join(
                "|",
                visited
            );

        PlayerPrefs.SetString(
            VISITED,
            merged
        );

        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(XKEY) &&
            PlayerPrefs.HasKey(YKEY))
        {
            currentRoom =
                new Vector2Int(
                    PlayerPrefs.GetInt(
                        XKEY
                    ),
                    PlayerPrefs.GetInt(
                        YKEY
                    )
                );

            currentRoom =
                ClampRoomToMap(
                    currentRoom
                );
        }
        else
        {
            currentRoom =
                ClampRoomToMap(
                    startRoom
                );
        }

        currentTurn =
            Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    TURN_KEY,
                    0
                )
            );

        currentEnvironment =
            ConvertEnvironmentToKorean(
                PlayerPrefs.GetString(
                    ENV_KEY,
                    DEFAULT_ENVIRONMENT
                )
            );

        visited.Clear();

        if (PlayerPrefs.HasKey(VISITED))
        {
            string data =
                PlayerPrefs.GetString(
                    VISITED,
                    ""
                );

            if (!string.IsNullOrEmpty(data))
            {
                string[] roomKeys =
                    data.Split('|');

                foreach (
                    string roomKey
                    in roomKeys)
                {
                    if (!string.IsNullOrEmpty(
                        roomKey))
                    {
                        visited.Add(
                            roomKey
                        );
                    }
                }
            }
        }
    }
}