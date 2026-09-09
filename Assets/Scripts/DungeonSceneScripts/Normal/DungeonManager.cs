using System.Collections.Generic;
using UnityEngine;

// 던전 전체 맵의 논리 좌표를 관리하는 스크립트
// 역할:
// 1) 현재 방 좌표 저장
// 2) 방문한 방 목록 저장
// 3) 이동 가능한 방향 계산
// 4) 미니맵 / 방향 버튼 UI 갱신
// 5) 이동 가능한 방향의 시각 오브젝트 갱신
public class DungeonManager : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private int mapWidth = 5;
    [SerializeField] private int mapHeight = 5;

    [Header("Start")]
    [SerializeField]
    private Vector2Int startRoom =
        new Vector2Int(2, 2);

    [Header("Refs")]
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private MinimapUIManager minimapUI;

    [SerializeField]
    private DungeonDirectionVisuals directionVisuals;

    // 현재 플레이어가 있는 논리 방 좌표
    private Vector2Int currentRoom;

    // 방문한 방들 저장
    private HashSet<string> visited = new HashSet<string>();

    // PlayerPrefs 키
    private const string XKEY = "ROOM_X";
    private const string YKEY = "ROOM_Y";
    private const string VISITED = "VISITED";

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public Vector2Int CurrentRoom => currentRoom;

    private void Awake()
    {
        Load();

        MarkVisited(currentRoom);

        Save();
    }

    private void Start()
    {
        RefreshAll();
    }

    // 방향 버튼을 눌렀을 때 실제 다음 방으로 이동
    public void MoveToNextRoom(MoveDirection dir)
    {
        Vector2Int next = currentRoom;

        if (dir == MoveDirection.Up)
            next += Vector2Int.up;

        if (dir == MoveDirection.Down)
            next += Vector2Int.down;

        if (dir == MoveDirection.Left)
            next += Vector2Int.left;

        if (dir == MoveDirection.Right)
            next += Vector2Int.right;

        // 맵 바깥이면 이동 불가
        if (!IsInside(next))
            return;

        currentRoom = next;

        MarkVisited(currentRoom);

        Save();

        RefreshAll();
    }

    // 방문 여부
    public bool IsVisited(int x, int y)
    {
        return visited.Contains(x + "," + y);
    }

    // 현재 위치 기준 이동 가능 방향
    public Dictionary<MoveDirection, bool> GetDirections()
    {
        return new Dictionary<MoveDirection, bool>()
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

    // UI / 미니맵 / 방향 오브젝트 전체 갱신
    public void RefreshAll()
    {
        Dictionary<MoveDirection, bool> directions =
            GetDirections();

        // 방향 버튼
        if (uiManager != null)
            uiManager.RefreshDirectionButtons(directions);

        // 미니맵
        if (minimapUI != null)
            minimapUI.RefreshMinimap();

        // 실제 방에 보이는 방향 이미지
        if (directionVisuals != null)
            directionVisuals.RefreshVisuals(directions);
    }

    private void MarkVisited(Vector2Int r)
    {
        visited.Add(r.x + "," + r.y);
    }

    private bool IsInside(Vector2Int r)
    {
        return
            r.x >= 0 &&
            r.x < mapWidth &&
            r.y >= 0 &&
            r.y < mapHeight;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(XKEY, currentRoom.x);
        PlayerPrefs.SetInt(YKEY, currentRoom.y);

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

        visited.Clear();

        if (PlayerPrefs.HasKey(VISITED))
        {
            string data =
                PlayerPrefs.GetString(VISITED);

            string[] arr =
                data.Split('|');

            foreach (string s in arr)
            {
                if (!string.IsNullOrEmpty(s))
                    visited.Add(s);
            }
        }
    }
}