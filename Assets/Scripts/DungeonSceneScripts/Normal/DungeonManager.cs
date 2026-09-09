using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance
    {
        get;
        private set;
    }


    // =========================================================
    // Map
    // =========================================================

    [Header("Map Size")]
    [SerializeField]
    private int mapWidth = 44;

    [SerializeField]
    private int mapHeight = 43;


    // =========================================================
    // Start / Base Camp
    // =========================================================

    [Header("Start / Base Camp")]
    [SerializeField]
    private Vector2Int startRoom =
        new Vector2Int(15, 29);


    // =========================================================
    // State
    // =========================================================

    [Header("Dungeon State")]
    [SerializeField]
    private int currentTurn = 0;

    [SerializeField]
    private string currentEnvironment =
        "지하";


    public event Action<int> OnTurnChanged;


    // =========================================================
    // References
    // =========================================================

    [Header("Refs")]

    [SerializeField]
    private DungeonUIManager uiManager;

    [SerializeField]
    private MinimapUIManager minimapUI;

    [SerializeField]
    private DungeonMapDatabase mapDatabase;

    [SerializeField]
    private MoveDataLoader moveDataLoader;


    // =========================================================
    // Runtime
    // =========================================================

    private Vector2Int currentRoom;


    private readonly HashSet<string>
        visited =
            new HashSet<string>();


    private bool freshDungeonEntry;


    // =========================================================
    // PlayerPrefs
    // =========================================================

    private const string XKEY =
        "ROOM_X";

    private const string YKEY =
        "ROOM_Y";

    private const string VISITED =
        "VISITED";

    private const string TURN_KEY =
        "DUNGEON_TURN";

    private const string ENVIRONMENT_KEY =
        "DUNGEON_ENVIRONMENT";


    private const string FreshDungeonEntryKey =
        "DUNGEON_FRESH_ENTRY";


    // =========================================================
    // Properties
    // =========================================================

    public int MapWidth =>
        mapWidth;


    public int MapHeight =>
        mapHeight;


    public Vector2Int CurrentRoom =>
        currentRoom;


    public int CurrentTurn =>
        currentTurn;


    public string CurrentEnvironment =>
        currentEnvironment;


    public Vector2Int StartRoom =>
        startRoom;


    public bool IsFreshDungeonEntry =>
        freshDungeonEntry;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);

            return;
        }


        Instance =
            this;


        // =====================================================
        // 기존 던전 상태 불러오기
        // =====================================================

        Load();


        // =====================================================
        // Lobby에서 새 Run으로 들어온 것인지 확인
        // =====================================================

        freshDungeonEntry =
            PlayerPrefs.GetInt(
                FreshDungeonEntryKey,
                0
            ) == 1;


        if (freshDungeonEntry)
        {
            /*
             * Lobby -> Dungeon 정상 입장.
             *
             * 저장된 마지막 던전 위치가 어디든
             * Base Camp에서 시작한다.
             */

            currentRoom =
                startRoom;


            /*
             * 플래그는 1회용.
             */
            PlayerPrefs.DeleteKey(
                FreshDungeonEntryKey
            );


            PlayerPrefs.Save();


            Debug.Log(
                "[DungeonManager] " +
                "새 던전 Run 입장\n" +
                $"Base Camp 시작: {currentRoom}"
            );
        }


        MarkVisited(
            currentRoom
        );
    }


    private void Start()
    {
        ResolveReferences();


        ValidateCurrentRoom();


        /*
         * 새 Run인 경우 DungeonScene 안의
         * Run 단위 상태들을 초기화한다.
         */
        if (freshDungeonEntry)
        {
            ResetRunState();
        }


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
            Instance =
                null;
        }
    }


    // =========================================================
    // New Run Reset
    // =========================================================

    private void ResetRunState()
    {
        DungeonTileEventManager
            tileEventManager =
                DungeonTileEventManager.Instance;


        if (tileEventManager != null)
        {
            /*
             * Farming:
             * 한 번 던전을 나가면 다시 사용 가능.
             */
            tileEventManager
                .ClearUsedFarmingTiles();


            /*
             * General:
             * 새로운 Run에서는 다시 10%부터.
             */
            tileEventManager
                .ResetGeneralBattleChance();
        }


        /*
         * RestTileManager는 DungeonScene 오브젝트이므로
         * Scene 재진입 과정에서 새 인스턴스로 생성된다.
         * 따라서 사용한 Rest HashSet도 자연스럽게 초기화된다.
         *
         * Chest / Key:
         * 절대 초기화하지 않는다.
         *
         * LockedDoor:
         * SaveManager가 열린 문 상태를 복구하므로
         * 절대 초기화하지 않는다.
         */


        Debug.Log(
            "[DungeonManager] 새 Run 상태 초기화\n" +
            "Farming = 초기화\n" +
            "General = 10% 초기화\n" +
            "Rest = 새 Scene 인스턴스로 초기화\n" +
            "Chest = 유지\n" +
            "Key = 유지\n" +
            "LockedDoor = 유지"
        );
    }


    // =========================================================
    // References
    // =========================================================

    private void ResolveReferences()
    {
        if (mapDatabase == null)
        {
            mapDatabase =
                DungeonMapDatabase.Instance;
        }


        if (moveDataLoader == null)
        {
            moveDataLoader =
                MoveDataLoader.Instance;
        }


        if (uiManager == null)
        {
            uiManager =
                FindFirstObjectByType<
                    DungeonUIManager
                >();
        }


        if (minimapUI == null)
        {
            minimapUI =
                FindFirstObjectByType<
                    MinimapUIManager
                >();
        }
    }


    // =========================================================
    // Move Data
    // =========================================================

    public MoveData GetMoveData(
        MoveDirection direction)
    {
        ResolveReferences();


        if (moveDataLoader == null)
        {
            return null;
        }


        return
            moveDataLoader.GetMoveData(
                currentRoom,
                direction
            );
    }


    // =========================================================
    // Open Move
    // =========================================================

    public bool CanMoveOpen(
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                direction
            );


        if (data == null)
        {
            return false;
        }


        if (!data.Passable)
        {
            return false;
        }


        if (
            data.PathType !=
            MovePathType.Open
        )
        {
            return false;
        }


        return
            IsDestinationValid(
                direction
            );
    }


    // =========================================================
    // Special Path
    // =========================================================

    public bool CanUseSpecialPath(
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                direction
            );


        if (data == null)
        {
            return false;
        }


        // Door
        if (
            data.PathType ==
            MovePathType.Door
        )
        {
            return
                data.Passable &&
                IsDestinationValid(
                    direction
                );
        }


        // OneWay
        if (
            data.PathType ==
            MovePathType.OneWay
        )
        {
            return
                data.Passable &&
                IsDestinationValid(
                    direction
                );
        }


        // LockedDoor
        if (
            data.PathType ==
            MovePathType.LockedDoor
        )
        {
            /*
             * LockedDoor는 닫혀 있을 때
             * Passable=False여도 Space UI에는 표시.
             */

            return
                IsDestinationValid(
                    direction
                );
        }


        // GimmickDoor
        if (
            data.PathType ==
            MovePathType.GimmickDoor
        )
        {
            return
                IsDestinationValid(
                    direction
                );
        }


        return false;
    }


    public bool HasAnySpecialPath()
    {
        return
            CanUseSpecialPath(
                MoveDirection.Up
            ) ||

            CanUseSpecialPath(
                MoveDirection.Down
            ) ||

            CanUseSpecialPath(
                MoveDirection.Left
            ) ||

            CanUseSpecialPath(
                MoveDirection.Right
            );
    }


    // =========================================================
    // General Move
    // =========================================================

    public bool CanMove(
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                direction
            );


        if (data == null)
        {
            return false;
        }


        if (data.Passable)
        {
            return
                IsDestinationValid(
                    direction
                );
        }


        /*
         * 열린 LockedDoor는
         * 원본 Move Data의 Passable=False여도 통과.
         */

        if (
            data.PathType ==
                MovePathType.LockedDoor &&
            LockedDoorManager.Instance != null &&
            LockedDoorManager.Instance
                .IsOpened(
                    currentRoom,
                    direction
                )
        )
        {
            return
                IsDestinationValid(
                    direction
                );
        }


        return false;
    }


    // =========================================================
    // Move Room
    // =========================================================

    public bool MoveToNextRoom(
        MoveDirection direction)
    {
        ResolveReferences();


        MoveData moveData =
            GetMoveData(
                direction
            );


        if (moveData == null)
        {
            Debug.LogWarning(
                "[DungeonManager] Move Data가 없습니다.\n" +
                $"현재 위치: {currentRoom}\n" +
                $"방향: {direction}"
            );

            return false;
        }


        bool canPass =
            moveData.Passable;


        // 열린 LockedDoor
        if (
            moveData.PathType ==
                MovePathType.LockedDoor &&
            LockedDoorManager.Instance != null &&
            LockedDoorManager.Instance
                .IsOpened(
                    currentRoom,
                    direction
                )
        )
        {
            canPass =
                true;
        }


        if (!canPass)
        {
            Debug.Log(
                "[DungeonManager] 이동 불가\n" +
                $"현재 위치: {currentRoom}\n" +
                $"방향: {direction}\n" +
                $"Type: {moveData.PathType}\n" +
                $"Passable: {moveData.Passable}"
            );

            return false;
        }


        if (moveDataLoader == null)
        {
            return false;
        }


        Vector2Int destination =
            moveDataLoader
                .GetDestination(
                    currentRoom,
                    direction
                );


        if (!CanMoveTo(
            destination))
        {
            Debug.LogWarning(
                "[DungeonManager] 목적지 타일이 " +
                "유효하지 않습니다.\n" +
                $"목적지: {destination}"
            );

            return false;
        }


        Vector2Int previous =
            currentRoom;


        currentRoom =
            destination;


        MarkVisited(
            currentRoom
        );


        AddTurn(
            "방 이동"
        );


        Save();


        RefreshAll();


        LogCurrentTile();


        Debug.Log(
            "[DungeonManager] 이동 완료\n" +
            $"{previous} -> {currentRoom}\n" +
            $"방향: {direction}\n" +
            $"Type: {moveData.PathType}"
        );


        return true;
    }


    // =========================================================
    // Teleport
    // =========================================================

    public bool TeleportToRoom(
        Vector2Int destination)
    {
        ResolveReferences();


        if (mapDatabase == null)
        {
            return false;
        }


        if (
            !mapDatabase.IsValidTile(
                destination
            )
        )
        {
            Debug.LogError(
                "[DungeonManager] 텔레포트 목적지가 " +
                "유효하지 않습니다.\n" +
                $"목적지: {destination}"
            );

            return false;
        }


        Vector2Int previous =
            currentRoom;


        currentRoom =
            destination;


        MarkVisited(
            currentRoom
        );


        /*
         * Teleport 자체는 추가 턴 없음.
         */


        Save();


        RefreshAll();


        LogCurrentTile();


        Debug.Log(
            "[DungeonManager] 텔레포트 이동 완료\n" +
            $"출발: {previous}\n" +
            $"도착: {currentRoom}"
        );


        return true;
    }


    // =========================================================
    // Destination
    // =========================================================

    private bool IsDestinationValid(
        MoveDirection direction)
    {
        ResolveReferences();


        if (moveDataLoader == null)
        {
            return false;
        }


        Vector2Int destination =
            moveDataLoader
                .GetDestination(
                    currentRoom,
                    direction
                );


        return
            CanMoveTo(
                destination
            );
    }


    public bool CanMoveTo(
        Vector2Int position)
    {
        ResolveReferences();


        // =========================================================
        // Base Camp
        // =========================================================

        /*
         * Base Camp는 Tile_Data에 존재하지 않는
         * 특수 좌표다.
         *
         * 따라서 DungeonMapDatabase에 없어도
         * 이동 가능한 유효 좌표로 취급한다.
         */
        if (position == startRoom)
        {
            return true;
        }


        // =========================================================
        // Normal Dungeon Tile
        // =========================================================

        if (mapDatabase == null)
        {
            return false;
        }


        return
            mapDatabase.IsValidTile(
                position
            );
    }


    // =========================================================
    // Directions
    // =========================================================

    public Dictionary<
        MoveDirection,
        bool
    > GetDirections()
    {
        return
            new Dictionary<
                MoveDirection,
                bool
            >
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


    public Dictionary<
        MoveDirection,
        bool
    > GetSpecialDirections()
    {
        return
            new Dictionary<
                MoveDirection,
                bool
            >
            {
                {
                    MoveDirection.Up,
                    CanUseSpecialPath(
                        MoveDirection.Up
                    )
                },

                {
                    MoveDirection.Down,
                    CanUseSpecialPath(
                        MoveDirection.Down
                    )
                },

                {
                    MoveDirection.Left,
                    CanUseSpecialPath(
                        MoveDirection.Left
                    )
                },

                {
                    MoveDirection.Right,
                    CanUseSpecialPath(
                        MoveDirection.Right
                    )
                }
            };
    }


    // =========================================================
    // Tile
    // =========================================================

    public DungeonTileData GetCurrentTile()
    {
        ResolveReferences();


        if (mapDatabase == null)
        {
            return null;
        }


        return
            mapDatabase.GetTile(
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
            return
                DungeonTileType.None;
        }


        return
            tile.TileType;
    }


    // =========================================================
    // Validation
    // =========================================================

    private void ValidateCurrentRoom()
    {
        ResolveReferences();


        // =========================================================
        // Base Camp
        // =========================================================

        /*
         * Base Camp는 Tile_Data에 없는 특수 좌표이므로
         * 여기서는 정상 좌표로 인정하고 종료.
         */
        if (currentRoom == startRoom)
        {
            Debug.Log(
                "[DungeonManager] 현재 위치: Base Camp\n" +
                $"좌표: {currentRoom}"
            );

            return;
        }


        // =========================================================
        // Database
        // =========================================================

        if (mapDatabase == null)
        {
            Debug.LogError(
                "[DungeonManager] " +
                "DungeonMapDatabase가 없습니다."
            );

            return;
        }


        // =========================================================
        // Normal Tile
        // =========================================================

        if (
            mapDatabase.IsValidTile(
                currentRoom
            )
        )
        {
            return;
        }


        // =========================================================
        // Invalid Saved Position
        // =========================================================

        Debug.LogWarning(
            "[DungeonManager] " +
            "유효하지 않은 현재 좌표입니다.\n" +
            $"현재: {currentRoom}\n" +
            "Base Camp으로 복구합니다."
        );


        currentRoom =
            startRoom;


        MarkVisited(
            currentRoom
        );


        Debug.Log(
            "[DungeonManager] " +
            "Base Camp으로 복구 완료\n" +
            $"좌표: {currentRoom}"
        );
    }


    // =========================================================
    // Log
    // =========================================================

    public void LogCurrentTile()
    {
        // =========================================================
        // Base Camp
        // =========================================================

        if (currentRoom == startRoom)
        {
            Debug.Log(
                "[DungeonManager] 현재 위치: Base Camp\n" +
                $"좌표: {currentRoom}"
            );

            return;
        }


        // =========================================================
        // Normal Tile
        // =========================================================

        DungeonTileData tile =
            GetCurrentTile();


        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonManager] 현재 타일 데이터 없음\n" +
                $"좌표: {currentRoom}"
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


    // =========================================================
    // Visited
    // =========================================================

    public bool IsVisited(
        int x,
        int y)
    {
        return
            visited.Contains(
                GetVisitedKey(
                    x,
                    y
                )
            );
    }


    public bool IsVisited(
        Vector2Int position)
    {
        return
            IsVisited(
                position.x,
                position.y
            );
    }


    private void MarkVisited(
        Vector2Int position)
    {
        visited.Add(
            GetVisitedKey(
                position.x,
                position.y
            )
        );
    }


    private string GetVisitedKey(
        int x,
        int y)
    {
        return
            x + "," + y;
    }


    // =========================================================
    // Turn
    // =========================================================

    public void AddTurn(
        string reason = "")
    {
        currentTurn++;


        PlayerPrefs.SetInt(
            TURN_KEY,
            currentTurn
        );


        PlayerPrefs.Save();


        Debug.Log(
            "[던전 턴] 현재 턴: " +
            currentTurn +
            (
                string.IsNullOrEmpty(
                    reason
                )
                ? ""
                : " / 행동: " + reason
            )
        );


        OnTurnChanged?.Invoke(
            currentTurn
        );
    }


    // =========================================================
    // UI
    // =========================================================

    public void RefreshAll()
    {
        ResolveReferences();


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


    // =========================================================
    // New Game
    // =========================================================

    public void ResetForNewGame(
        bool saveData = true)
    {
        currentRoom =
            startRoom;


        currentTurn =
            0;


        currentEnvironment =
            "지하";


        freshDungeonEntry =
            true;


        visited.Clear();


        MarkVisited(
            currentRoom
        );


        if (
            LockedDoorManager.Instance != null
        )
        {
            /*
             * 진짜 New Game에서만 LockedDoor 초기화.
             */
            LockedDoorManager.Instance
                .ClearOpenedDoors();
        }


        ResetRunState();


        if (saveData)
        {
            Save();
        }


        RefreshAll();


        OnTurnChanged?.Invoke(
            currentTurn
        );
    }


    // =========================================================
    // Save
    // =========================================================

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


        PlayerPrefs.SetString(
            VISITED,
            string.Join(
                "|",
                visited
            )
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
        if (
            PlayerPrefs.HasKey(
                XKEY
            ) &&
            PlayerPrefs.HasKey(
                YKEY
            )
        )
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


        string visitedData =
            PlayerPrefs.GetString(
                VISITED,
                ""
            );


        if (
            string.IsNullOrWhiteSpace(
                visitedData
            )
        )
        {
            return;
        }


        string[] values =
            visitedData.Split('|');


        foreach (
            string value
            in values
        )
        {
            if (
                !string.IsNullOrWhiteSpace(
                    value
                )
            )
            {
                visited.Add(
                    value
                );
            }
        }
    }


    // =========================================================
    // SaveManager API
    // =========================================================

    public List<string>
        GetVisitedRoomsForSave()
    {
        return
            new List<string>(
                visited
            );
    }


    public void RestoreDungeonState(
        Vector2Int room,
        int turn,
        string environment,
        List<string> visitedRooms)
    {
        /*
         * Lobby에서 새 Run으로 들어왔다면
         * SaveManager가 과거 좌표를 복원해
         * Base Camp 위치를 덮어쓰면 안 된다.
         */

        if (freshDungeonEntry)
        {
            room =
                startRoom;
        }


        currentRoom =
            room;


        currentTurn =
            Mathf.Max(
                0,
                turn
            );


        currentEnvironment =
            string.IsNullOrEmpty(
                environment
            )
            ? "지하"
            : environment;


        visited.Clear();


        if (visitedRooms != null)
        {
            foreach (
                string roomKey
                in visitedRooms
            )
            {
                if (
                    !string.IsNullOrWhiteSpace(
                        roomKey
                    )
                )
                {
                    visited.Add(
                        roomKey
                    );
                }
            }
        }


        MarkVisited(
            currentRoom
        );


        Save();


        RefreshAll();


        OnTurnChanged?.Invoke(
            currentTurn
        );


        Debug.Log(
            "[DungeonManager] 던전 상태 복구\n" +
            $"현재 위치: {currentRoom}\n" +
            $"새 Run: {freshDungeonEntry}"
        );
    }


    // =========================================================
    // Tests
    // =========================================================

    [ContextMenu(
        "TEST - Move To Base Camp"
    )]
    public void TestMoveToBaseCamp()
    {
        TestMoveTo(
            startRoom,
            "Base Camp"
        );
    }


    [ContextMenu(
        "TEST - Move To Door Test"
    )]
    public void TestMoveToDoor()
    {
        TestMoveTo(
            new Vector2Int(
                6,
                24
            ),
            "Door Test"
        );
    }


    [ContextMenu(
        "TEST - Move To Farming"
    )]
    public void TestMoveToFarming()
    {
        TestMoveTo(
            new Vector2Int(
                8,
                23
            ),
            "Farming"
        );
    }


    [ContextMenu(
        "TEST - Move To Teleport"
    )]
    public void TestMoveToTeleport()
    {
        TestMoveTo(
            new Vector2Int(
                3,
                33
            ),
            "Teleport"
        );
    }


    private void TestMoveTo(
        Vector2Int position,
        string label)
    {
        currentRoom =
            position;


        MarkVisited(
            currentRoom
        );


        Save();


        RefreshAll();


        LogCurrentTile();


        Debug.Log(
            "[DungeonManager] " +
            label +
            " 테스트 위치 이동: " +
            currentRoom
        );
    }
}