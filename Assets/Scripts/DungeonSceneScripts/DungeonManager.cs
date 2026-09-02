using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Map Size")]
    [SerializeField] private int mapWidth = 44;
    [SerializeField] private int mapHeight = 43;

    [Header("Start")]
    [SerializeField]
    private Vector2Int startRoom =
        new Vector2Int(0, 26);

    [Header("Dungeon State")]
    [SerializeField] private int currentTurn = 0;
    [SerializeField] private string currentEnvironment = "지하";
    public event Action<int> OnTurnChanged;

    [Header("Refs")]
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private MinimapUIManager minimapUI;
    [SerializeField] private DungeonMapDatabase mapDatabase;

    private Vector2Int currentRoom;

    private readonly HashSet<string> visited =
        new HashSet<string>();

    private const string XKEY = "ROOM_X";
    private const string YKEY = "ROOM_Y";
    private const string VISITED = "VISITED";

    private const string TURN_KEY = "DUNGEON_TURN";
    private const string ENVIRONMENT_KEY = "DUNGEON_ENVIRONMENT";

    // ============================
    // 기존 스크립트 호환용 Property
    // ============================

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;

    public Vector2Int CurrentRoom => currentRoom;

    public int CurrentTurn => currentTurn;

    public string CurrentEnvironment =>
        currentEnvironment;

    // ============================
    // Unity
    // ============================

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
    }

    private void Start()
    {
        if (mapDatabase == null)
        {
            mapDatabase =
                DungeonMapDatabase.Instance;
        }

        ValidateCurrentRoom();

        Save();

        RefreshAll();

        LogCurrentTile();

        Debug.Log(
            "[던전] 현재 턴: " +
            currentTurn
        );

        Debug.Log(
            "[던전] 현재 장소: " +
            currentEnvironment
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

    // ============================
    // 이동
    // ============================

    public void MoveToNextRoom(
        MoveDirection direction)
    {
        if (mapDatabase == null)
        {
            mapDatabase =
                DungeonMapDatabase.Instance;
        }

        if (mapDatabase == null)
        {
            Debug.LogError(
                "[DungeonManager] DungeonMapDatabase를 찾을 수 없습니다."
            );

            return;
        }

        Vector2Int next =
            GetNextPosition(
                currentRoom,
                direction
            );

        if (!CanMoveTo(next))
        {
            Debug.LogWarning(
                "[DungeonManager] 이동 불가: " +
                currentRoom +
                " -> " +
                next
            );

            return;
        }

        currentRoom = next;

        MarkVisited(currentRoom);

        // 방 하나 이동 = 던전 턴 +1
        AddTurn("방 이동");

        Save();

        RefreshAll();

        LogCurrentTile();
    }

    public bool CanMove(
        MoveDirection direction)
    {
        Vector2Int next =
            GetNextPosition(
                currentRoom,
                direction
            );

        return CanMoveTo(next);
    }

    public bool CanMoveTo(
        Vector2Int position)
    {
        if (mapDatabase == null)
        {
            mapDatabase =
                DungeonMapDatabase.Instance;
        }

        if (mapDatabase == null)
        {
            return false;
        }

        return mapDatabase.IsValidTile(
            position
        );
    }

    public Dictionary<MoveDirection, bool>
        GetDirections()
    {
        return new Dictionary<
            MoveDirection,
            bool>
        {
            {
                MoveDirection.Up,
                CanMove(
                    MoveDirection.Up
                )
            },

            {
                MoveDirection.Down,
                CanMove(
                    MoveDirection.Down
                )
            },

            {
                MoveDirection.Left,
                CanMove(
                    MoveDirection.Left
                )
            },

            {
                MoveDirection.Right,
                CanMove(
                    MoveDirection.Right
                )
            }
        };
    }

    private Vector2Int GetNextPosition(
        Vector2Int origin,
        MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up:
                return origin +
                       Vector2Int.up;

            case MoveDirection.Down:
                return origin +
                       Vector2Int.down;

            case MoveDirection.Left:
                return origin +
                       Vector2Int.left;

            case MoveDirection.Right:
                return origin +
                       Vector2Int.right;
        }

        return origin;
    }

    // ============================
    // Tile
    // ============================

    public DungeonTileData GetCurrentTile()
    {
        if (mapDatabase == null)
        {
            mapDatabase =
                DungeonMapDatabase.Instance;
        }

        if (mapDatabase == null)
        {
            return null;
        }

        return mapDatabase.GetTile(
            currentRoom
        );
    }

    public DungeonTileType
        GetCurrentTileType()
    {
        DungeonTileData tile =
            GetCurrentTile();

        if (tile == null)
        {
            return DungeonTileType.None;
        }

        return tile.TileType;
    }

    private void ValidateCurrentRoom()
    {
        if (mapDatabase == null)
        {
            Debug.LogError(
                "[DungeonManager] DungeonMapDatabase가 없습니다."
            );

            return;
        }

        if (mapDatabase.IsValidTile(
                currentRoom))
        {
            return;
        }

        Debug.LogWarning(
            "[DungeonManager] 저장된 현재 좌표 " +
            currentRoom +
            "가 새 맵에서 유효하지 않습니다."
        );

        if (mapDatabase.IsValidTile(
                startRoom))
        {
            currentRoom = startRoom;

            MarkVisited(currentRoom);

            Debug.Log(
                "[DungeonManager] 시작 좌표 " +
                startRoom +
                "로 이동합니다."
            );

            return;
        }

        Debug.LogError(
            "[DungeonManager] Start Room " +
            startRoom +
            "도 유효하지 않은 타일입니다."
        );
    }

    private void LogCurrentTile()
    {
        DungeonTileData tile =
            GetCurrentTile();

        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonManager] 현재 좌표 " +
                currentRoom +
                "의 Tile_Data가 없습니다."
            );

            return;
        }

        Debug.Log(
            "[DungeonManager] 현재 타일: (" +
            tile.X +
            ", " +
            tile.Y +
            ") / " +
            tile.TileType
        );
    }

    // ============================
    // 방문 여부
    // ============================

    public bool IsVisited(
        int x,
        int y)
    {
        return visited.Contains(
            GetVisitedKey(
                x,
                y
            )
        );
    }

    public bool IsVisited(
        Vector2Int position)
    {
        return IsVisited(
            position.x,
            position.y
        );
    }

    private void MarkVisited(
        Vector2Int room)
    {
        visited.Add(
            GetVisitedKey(
                room.x,
                room.y
            )
        );
    }

    private string GetVisitedKey(
        int x,
        int y)
    {
        return x + "," + y;
    }

    // ============================
    // 턴
    // ============================

    public void AddTurn(string reason = "")
    {
        currentTurn++;

        PlayerPrefs.SetInt(
            TURN_KEY,
            currentTurn
        );

        PlayerPrefs.Save();

        if (string.IsNullOrEmpty(reason))
        {
            Debug.Log(
                "[던전 턴] 현재 턴: " +
                currentTurn
            );
        }
        else
        {
            Debug.Log(
                "[던전 턴] 현재 턴: " +
                currentTurn +
                " / 행동: " +
                reason
            );
        }

        OnTurnChanged?.Invoke(
            currentTurn
        );
    }

    // ============================
    // UI
    // ============================

    public void RefreshAll()
    {
        if (uiManager != null)
        {
            uiManager
                .RefreshDirectionButtons(
                    GetDirections()
                );
        }

        if (minimapUI != null)
        {
            minimapUI
                .RefreshMinimap();
        }
    }
    public void ResetForNewGame(bool saveData = true)
    {
        currentRoom = startRoom;

        currentTurn = 0;

        currentEnvironment = "지하";

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
            "/ 시작 방: " +
            currentRoom +
            " / 턴: " +
            currentTurn +
            " / 환경: " +
            currentEnvironment
        );
    }


    // ============================
    // Save
    // ============================

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

        string merged =
            string.Join(
                "|",
                visited
            );

        PlayerPrefs.SetString(
            VISITED,
            merged
        );

        PlayerPrefs.SetInt(
            TURN_KEY,
            currentTurn
        );

        PlayerPrefs.SetString(
            ENVIRONMENT_KEY,
            currentEnvironment
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
        }
        else
        {
            currentRoom =
                startRoom;
        }

        currentTurn =
            PlayerPrefs.GetInt(
                TURN_KEY,
                0
            );

        currentEnvironment =
            PlayerPrefs.GetString(
                ENVIRONMENT_KEY,
                "지하"
            );

        visited.Clear();

        if (!PlayerPrefs.HasKey(
                VISITED))
        {
            return;
        }

        string data =
            PlayerPrefs.GetString(
                VISITED
            );

        if (string.IsNullOrWhiteSpace(
                data))
        {
            return;
        }

        string[] arr =
            data.Split('|');

        foreach (string value in arr)
        {
            if (!string.IsNullOrWhiteSpace(
                    value))
            {
                visited.Add(value);
            }
        }
    }

    // SaveManager 연동
    // ============================

    public List<string> GetVisitedRoomsForSave()
    {
        return new List<string>(visited);
    }

    public void RestoreDungeonState(
        Vector2Int room,
        int turn,
        string environment,
        List<string> visitedRooms)
    {
        currentRoom = room;
        currentTurn = Mathf.Max(0, turn);

        currentEnvironment =
            string.IsNullOrEmpty(environment)
                ? "지하"
                : environment;

        visited.Clear();

        if (visitedRooms != null)
        {
            foreach (string roomKey in visitedRooms)
            {
                if (!string.IsNullOrWhiteSpace(roomKey))
                {
                    visited.Add(roomKey);
                }
            }
        }

        MarkVisited(currentRoom);

        Save();

        RefreshAll();

        OnTurnChanged?.Invoke(currentTurn);

        Debug.Log(
            "[DungeonManager] SaveManager 던전 상태 복구 완료\n" +
            $"현재 위치: {currentRoom}\n" +
            $"현재 턴: {currentTurn}\n" +
            $"환경: {currentEnvironment}\n" +
            $"방문 타일: {visited.Count}"
        );
    }
}