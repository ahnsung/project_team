using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-850)]
public class MoveDataLoader : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static MoveDataLoader Instance
    {
        get;
        private set;
    }


    // =========================================================
    // Inspector
    // =========================================================

    [Header("CSV")]

    [SerializeField]
    private TextAsset moveDataCsv;


    // =========================================================
    // Runtime Data
    // =========================================================

    private readonly Dictionary<
        Vector2Int,
        Dictionary<MoveDirection, MoveData>
    > moveDataByPosition
        =
        new Dictionary<
            Vector2Int,
            Dictionary<MoveDirection, MoveData>
        >();


    public int Count
    {
        get;
        private set;
    }


    public bool IsLoaded
    {
        get;
        private set;
    }


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


        Instance = this;


        LoadMoveData();
    }


    // =========================================================
    // Load
    // =========================================================

    public void LoadMoveData()
    {
        moveDataByPosition.Clear();

        Count = 0;
        IsLoaded = false;


        if (moveDataCsv == null)
        {
            Debug.LogError(
                "[MoveDataLoader] " +
                "Move Data CSV가 연결되지 않았습니다."
            );

            return;
        }


        string csvText =
            moveDataCsv.text;


        if (string.IsNullOrWhiteSpace(csvText))
        {
            Debug.LogError(
                "[MoveDataLoader] " +
                "Move Data CSV 내용이 비어 있습니다."
            );

            return;
        }


        string[] lines =
            csvText.Split(
                new[]
                {
                    "\r\n",
                    "\n",
                    "\r"
                },
                StringSplitOptions.None
            );


        if (lines.Length <= 1)
        {
            Debug.LogError(
                "[MoveDataLoader] " +
                "Move Data CSV에 데이터가 없습니다."
            );

            return;
        }


        int successCount = 0;
        int skippedCount = 0;
        int errorCount = 0;


        // 0번 줄은 Header
        for (
            int i = 1;
            i < lines.Length;
            i++
        )
        {
            string line =
                lines[i].Trim();


            // 빈 줄 무시
            if (string.IsNullOrWhiteSpace(line))
            {
                skippedCount++;
                continue;
            }


            string[] columns =
                line.Split(',');


            if (columns.Length < 5)
            {
                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"CSV {i + 1}번째 줄 형식 오류\n" +
                    $"내용: {line}"
                );

                errorCount++;
                continue;
            }


            string xText =
                CleanValue(columns[0]);

            string yText =
                CleanValue(columns[1]);

            string directionText =
                CleanValue(columns[2]);

            string typeText =
                CleanValue(columns[3]);

            string passableText =
                CleanValue(columns[4]);


            // ---------------------------------------------
            // 완전히 비어 있는 행
            // ---------------------------------------------

            if (
                string.IsNullOrWhiteSpace(xText) &&
                string.IsNullOrWhiteSpace(yText) &&
                string.IsNullOrWhiteSpace(directionText)
            )
            {
                skippedCount++;
                continue;
            }


            // ---------------------------------------------
            // X
            // ---------------------------------------------

            if (
                !int.TryParse(
                    xText,
                    out int x
                )
            )
            {
                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 X 변환 실패: " +
                    xText
                );

                errorCount++;
                continue;
            }


            // ---------------------------------------------
            // Y
            // ---------------------------------------------

            if (
                !int.TryParse(
                    yText,
                    out int y
                )
            )
            {
                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 Y 변환 실패: " +
                    yText
                );

                errorCount++;
                continue;
            }


            // ---------------------------------------------
            // Direction
            // ---------------------------------------------

            if (
                !TryParseDirection(
                    directionText,
                    out MoveDirection direction
                )
            )
            {
                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 Direction 오류: " +
                    directionText
                );

                errorCount++;
                continue;
            }


            // ---------------------------------------------
            // Type
            // ---------------------------------------------

            if (
                !TryParsePathType(
                    typeText,
                    out MovePathType pathType
                )
            )
            {
                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 Type 오류: " +
                    typeText
                );

                errorCount++;
                continue;
            }


            // ---------------------------------------------
            // Passable
            // ---------------------------------------------

            if (
                !TryParsePassable(
                    passableText,
                    out bool passable
                )
            )
            {
                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 Passable 오류: " +
                    passableText
                );

                errorCount++;
                continue;
            }


            Vector2Int position =
                new Vector2Int(
                    x,
                    y
                );


            MoveData moveData =
                new MoveData(
                    x,
                    y,
                    direction,
                    pathType,
                    passable
                );


            // ---------------------------------------------
            // 좌표 Dictionary 생성
            // ---------------------------------------------

            if (
                !moveDataByPosition.TryGetValue(
                    position,
                    out Dictionary<
                        MoveDirection,
                        MoveData
                    > directionMap
                )
            )
            {
                directionMap =
                    new Dictionary<
                        MoveDirection,
                        MoveData
                    >();


                moveDataByPosition.Add(
                    position,
                    directionMap
                );
            }


            // ---------------------------------------------
            // 같은 좌표 + 방향 중복 검사
            // ---------------------------------------------

            if (
                directionMap.ContainsKey(
                    direction
                )
            )
            {
                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    "중복 이동 데이터 발견\n" +
                    $"좌표: {position}\n" +
                    $"방향: {direction}\n" +
                    "뒤쪽 데이터를 사용합니다."
                );


                directionMap[direction] =
                    moveData;
            }
            else
            {
                directionMap.Add(
                    direction,
                    moveData
                );
            }


            successCount++;
        }


        Count = successCount;

        IsLoaded =
            successCount > 0;


        Debug.Log(
            "[MoveDataLoader] " +
            $"Move Data 로드 완료: {successCount}개\n" +
            $"좌표 수: {moveDataByPosition.Count}개\n" +
            $"빈 줄 무시: {skippedCount}개\n" +
            $"오류: {errorCount}개"
        );


        ValidateData();
    }


    // =========================================================
    // Public - Get
    // =========================================================

    public MoveData GetMoveData(
        Vector2Int position,
        MoveDirection direction)
    {
        if (
            !moveDataByPosition.TryGetValue(
                position,
                out Dictionary<
                    MoveDirection,
                    MoveData
                > directionMap
            )
        )
        {
            return null;
        }


        if (
            !directionMap.TryGetValue(
                direction,
                out MoveData moveData
            )
        )
        {
            return null;
        }


        return moveData;
    }


    public MoveData GetMoveData(
        int x,
        int y,
        MoveDirection direction)
    {
        return GetMoveData(
            new Vector2Int(
                x,
                y
            ),
            direction
        );
    }


    // =========================================================
    // Public - Has
    // =========================================================

    public bool HasMoveData(
        Vector2Int position,
        MoveDirection direction)
    {
        return
            GetMoveData(
                position,
                direction
            ) != null;
    }


    // =========================================================
    // Public - Passable
    // =========================================================

    public bool IsPassable(
        Vector2Int position,
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                position,
                direction
            );


        if (data == null)
        {
            return false;
        }


        return data.Passable;
    }


    // =========================================================
    // Public - Type
    // =========================================================

    public MovePathType GetPathType(
        Vector2Int position,
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                position,
                direction
            );


        if (data == null)
        {
            return MovePathType.Wall;
        }


        return data.PathType;
    }


    // =========================================================
    // Public - Destination
    // =========================================================

    public Vector2Int GetDestination(
        Vector2Int position,
        MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up:

                return position +
                       Vector2Int.up;


            case MoveDirection.Down:

                return position +
                       Vector2Int.down;


            case MoveDirection.Left:

                return position +
                       Vector2Int.left;


            case MoveDirection.Right:

                return position +
                       Vector2Int.right;


            default:

                return position;
        }
    }


    // =========================================================
    // Validation
    // =========================================================

    private void ValidateData()
    {
        int incompletePositionCount = 0;


        foreach (
            KeyValuePair<
                Vector2Int,
                Dictionary<MoveDirection, MoveData>
            > pair
            in moveDataByPosition
        )
        {
            Dictionary<
                MoveDirection,
                MoveData
            > directionMap =
                pair.Value;


            if (
                !directionMap.ContainsKey(
                    MoveDirection.Up
                ) ||
                !directionMap.ContainsKey(
                    MoveDirection.Down
                ) ||
                !directionMap.ContainsKey(
                    MoveDirection.Left
                ) ||
                !directionMap.ContainsKey(
                    MoveDirection.Right
                )
            )
            {
                incompletePositionCount++;


                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"4방향 데이터가 모두 없는 좌표: " +
                    pair.Key
                );
            }
        }


        if (incompletePositionCount == 0)
        {
            Debug.Log(
                "[MoveDataLoader] " +
                "모든 좌표의 4방향 이동 데이터 확인 완료."
            );
        }
        else
        {
            Debug.LogWarning(
                "[MoveDataLoader] " +
                "4방향 데이터가 부족한 좌표 수: " +
                incompletePositionCount
            );
        }
    }


    // =========================================================
    // Parse Helpers
    // =========================================================

    private string CleanValue(
        string value)
    {
        if (value == null)
        {
            return string.Empty;
        }


        return value
            .Trim()
            .Trim('"')
            .Trim();
    }


    private bool TryParseDirection(
        string value,
        out MoveDirection direction)
    {
        direction =
            MoveDirection.Up;


        switch (
            value.Trim().ToLowerInvariant()
        )
        {
            case "north":
                direction =
                    MoveDirection.Up;

                return true;


            case "south":
                direction =
                    MoveDirection.Down;

                return true;


            case "west":
                direction =
                    MoveDirection.Left;

                return true;


            case "east":
                direction =
                    MoveDirection.Right;

                return true;
        }


        return false;
    }


    private bool TryParsePathType(
        string value,
        out MovePathType pathType)
    {
        pathType =
            MovePathType.Wall;


        switch (
            value.Trim().ToLowerInvariant()
        )
        {
            case "open":
                pathType =
                    MovePathType.Open;

                return true;


            case "wall":
                pathType =
                    MovePathType.Wall;

                return true;


            case "door":
                pathType =
                    MovePathType.Door;

                return true;


            case "oneway":
                pathType =
                    MovePathType.OneWay;

                return true;


            case "lockeddoor":
                pathType =
                    MovePathType.LockedDoor;

                return true;


            case "gimmickdoor":
                pathType =
                    MovePathType.GimmickDoor;

                return true;
        }


        return false;
    }


    private bool TryParsePassable(
        string value,
        out bool passable)
    {
        passable = false;


        string lower =
            value
                .Trim()
                .ToLowerInvariant();


        switch (lower)
        {
            case "o":
            case "true":
            case "1":
            case "yes":

                passable = true;
                return true;


            case "x":
            case "false":
            case "0":
            case "no":

                passable = false;
                return true;
        }


        return false;
    }


    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("현재 Move Data 다시 로드")]
    private void ReloadFromInspector()
    {
        LoadMoveData();
    }
}