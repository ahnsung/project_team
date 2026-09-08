using System;
using System.Collections.Generic;
using UnityEngine;

public class TeleportDataLoader : MonoBehaviour
{
    public static TeleportDataLoader Instance
    {
        get;
        private set;
    }


    [Header("Teleport Data CSV")]
    [SerializeField]
    private TextAsset teleportDataCsv;


    private readonly Dictionary<
        Vector2Int,
        TeleportTileData
    > dataByPosition =
        new Dictionary<
            Vector2Int,
            TeleportTileData
        >();


    private readonly Dictionary<
        string,
        List<TeleportTileData>
    > dataByTeleportID =
        new Dictionary<
            string,
            List<TeleportTileData>
        >();


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
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


        LoadTeleportData();
    }


    // =========================================================
    // Load
    // =========================================================

    public void LoadTeleportData()
    {
        if (IsLoaded)
            return;


        dataByPosition.Clear();
        dataByTeleportID.Clear();


        if (teleportDataCsv == null)
        {
            Debug.LogError(
                "[TeleportDataLoader] " +
                "Teleport_Data CSV가 연결되지 않았습니다."
            );

            return;
        }


        string csvText =
            teleportDataCsv.text;


        if (string.IsNullOrWhiteSpace(
            csvText))
        {
            Debug.LogError(
                "[TeleportDataLoader] " +
                "Teleport_Data CSV가 비어 있습니다."
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
                StringSplitOptions
                    .RemoveEmptyEntries
            );


        for (
            int i = 0;
            i < lines.Length;
            i++)
        {
            string line =
                lines[i].Trim();


            if (string.IsNullOrWhiteSpace(
                line))
            {
                continue;
            }


            // 헤더 건너뛰기
            if (i == 0 &&
                line.ToLowerInvariant()
                    .Contains(
                        "connectedteleportid"
                    ))
            {
                continue;
            }


            string[] columns =
                line.Split(',');


            if (columns.Length < 3)
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "잘못된 행 형식\n" +
                    $"Line: {i + 1}\n" +
                    $"내용: {line}"
                );

                continue;
            }


            string xText =
                CleanValue(
                    columns[0]
                );


            string yText =
                CleanValue(
                    columns[1]
                );


            string teleportID =
                CleanValue(
                    columns[2]
                );


            if (!int.TryParse(
                xText,
                out int x))
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "X 좌표 변환 실패\n" +
                    $"Line: {i + 1}\n" +
                    $"값: {xText}"
                );

                continue;
            }


            if (!int.TryParse(
                yText,
                out int y))
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "Y 좌표 변환 실패\n" +
                    $"Line: {i + 1}\n" +
                    $"값: {yText}"
                );

                continue;
            }


            if (string.IsNullOrWhiteSpace(
                teleportID))
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "ConnectedTeleportID가 비어 있습니다.\n" +
                    $"좌표: ({x}, {y})"
                );

                continue;
            }


            Vector2Int position =
                new Vector2Int(
                    x,
                    y
                );


            if (dataByPosition.ContainsKey(
                position))
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "중복 좌표입니다: " +
                    position
                );

                continue;
            }


            TeleportTileData data =
                new TeleportTileData
                {
                    x = x,
                    y = y,
                    connectedTeleportID =
                        teleportID
                };


            dataByPosition.Add(
                position,
                data
            );


            if (!dataByTeleportID.ContainsKey(
                teleportID))
            {
                dataByTeleportID.Add(
                    teleportID,
                    new List<TeleportTileData>()
                );
            }


            dataByTeleportID[
                teleportID
            ].Add(
                data
            );
        }


        IsLoaded =
            true;


        Debug.Log(
            "[TeleportDataLoader] " +
            "Teleport_Data 로드 완료: " +
            dataByPosition.Count +
            "개"
        );


        ValidatePairs();
    }


    // =========================================================
    // Pair Validation
    // =========================================================

    private void ValidatePairs()
    {
        foreach (
            KeyValuePair<
                string,
                List<TeleportTileData>
            > pair
            in dataByTeleportID)
        {
            if (pair.Value.Count != 2)
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "Teleport ID는 정확히 2개 좌표가 " +
                    "필요합니다.\n" +
                    $"ID: {pair.Key}\n" +
                    $"현재 개수: {pair.Value.Count}"
                );
            }
        }
    }


    // =========================================================
    // Get Data
    // =========================================================

    public TeleportTileData GetData(
        int x,
        int y)
    {
        return GetData(
            new Vector2Int(
                x,
                y
            )
        );
    }


    public TeleportTileData GetData(
        Vector2Int position)
    {
        if (dataByPosition.TryGetValue(
            position,
            out TeleportTileData data))
        {
            return data;
        }


        return null;
    }


    // =========================================================
    // Get Destination
    // =========================================================

    public bool TryGetDestination(
        Vector2Int currentPosition,
        out Vector2Int destination)
    {
        destination =
            currentPosition;


        TeleportTileData currentData =
            GetData(
                currentPosition
            );


        if (currentData == null)
        {
            return false;
        }


        string teleportID =
            currentData.connectedTeleportID;


        if (string.IsNullOrWhiteSpace(
            teleportID))
        {
            return false;
        }


        if (!dataByTeleportID.TryGetValue(
            teleportID,
            out List<TeleportTileData> pair))
        {
            return false;
        }


        if (pair == null ||
            pair.Count != 2)
        {
            return false;
        }


        foreach (
            TeleportTileData data
            in pair)
        {
            Vector2Int position =
                new Vector2Int(
                    data.x,
                    data.y
                );


            if (position != currentPosition)
            {
                destination =
                    position;

                return true;
            }
        }


        return false;
    }


    public bool HasData(
        int x,
        int y)
    {
        return dataByPosition.ContainsKey(
            new Vector2Int(
                x,
                y
            )
        );
    }


    // =========================================================
    // Utility
    // =========================================================

    private string CleanValue(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
            value))
        {
            return string.Empty;
        }


        value =
            value.Trim();


        if (value.StartsWith("\"") &&
            value.EndsWith("\"") &&
            value.Length >= 2)
        {
            value =
                value.Substring(
                    1,
                    value.Length - 2
                );
        }


        return value.Trim();
    }
}