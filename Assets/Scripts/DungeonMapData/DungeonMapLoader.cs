using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMapLoader : MonoBehaviour
{
    [Header("Tile Data CSV")]
    [SerializeField] private TextAsset tileDataCsv;

    [Header("Target Database")]
    [SerializeField] private DungeonMapDatabase dungeonMapDatabase;

    private void Start()
    {
        LoadTileData();
    }

    public void LoadTileData()
    {
        if (tileDataCsv == null)
        {
            Debug.LogError("[DungeonMapLoader] Tile_Data CSV가 연결되지 않았습니다.");
            return;
        }

        if (dungeonMapDatabase == null)
        {
            dungeonMapDatabase = DungeonMapDatabase.Instance;
        }

        if (dungeonMapDatabase == null)
        {
            Debug.LogError("[DungeonMapLoader] DungeonMapDatabase를 찾을 수 없습니다.");
            return;
        }

        DungeonMapData mapData = ParseTileData(tileDataCsv.text);

        dungeonMapDatabase.SetMapData(mapData);
    }

    private DungeonMapData ParseTileData(string csvText)
    {
        DungeonMapData mapData = new DungeonMapData();

        if (string.IsNullOrWhiteSpace(csvText))
        {
            Debug.LogError("[DungeonMapLoader] CSV 내용이 비어 있습니다.");
            return mapData;
        }

        string[] lines = csvText.Split(
            new[] { "\r\n", "\n", "\r" },
            StringSplitOptions.RemoveEmptyEntries
        );

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            // 첫 줄 헤더 건너뛰기
            if (i == 0 && line.ToLower().Contains("tile"))
                continue;

            string[] columns = line.Split(',');

            if (columns.Length < 3)
            {
                Debug.LogWarning(
                    $"[DungeonMapLoader] 잘못된 행 형식. Line {i + 1}: {line}"
                );
                continue;
            }

            string xText = CleanValue(columns[0]);
            string yText = CleanValue(columns[1]);
            string typeText = CleanValue(columns[2]);

            if (!int.TryParse(xText, out int x))
            {
                Debug.LogWarning(
                    $"[DungeonMapLoader] X 좌표 변환 실패. Line {i + 1}: {xText}"
                );
                continue;
            }

            if (!int.TryParse(yText, out int y))
            {
                Debug.LogWarning(
                    $"[DungeonMapLoader] Y 좌표 변환 실패. Line {i + 1}: {yText}"
                );
                continue;
            }

            DungeonTileType tileType = ParseTileType(typeText);

            DungeonTileData tileData = new DungeonTileData(
                x,
                y,
                tileType
            );

            mapData.AddTile(tileData);
        }

        Debug.Log(
            $"[DungeonMapLoader] Tile_Data 로드 완료: {mapData.Tiles.Count}개"
        );

        return mapData;
    }

    private DungeonTileType ParseTileType(string rawType)
    {
        string value = CleanValue(rawType);

        switch (value.ToLowerInvariant())
        {
            case "none":
                return DungeonTileType.None;

            case "general":
                return DungeonTileType.General;

            case "farming":
                return DungeonTileType.Farming;

            case "trap":
                return DungeonTileType.Trap;

            case "key":
                return DungeonTileType.Key;

            case "teleport":
                return DungeonTileType.Teleport;

            case "chest":
                return DungeonTileType.Chest;

            case "puzzle/letter":
            case "puzzleletter":
            case "puzzle_letter":
                return DungeonTileType.PuzzleLetter;

            case "event/hint":
            case "eventhint":
            case "event_hint":
                return DungeonTileType.EventHint;

            case "rest":
                return DungeonTileType.Rest;

            case "boss":
                return DungeonTileType.Boss;

            case "locked_door":
            case "lockeddoor":
            case "locked door":
                return DungeonTileType.LockedDoor;

            default:
                Debug.LogWarning(
                    $"[DungeonMapLoader] 알 수 없는 Tile Type: '{rawType}' → None 처리"
                );

                return DungeonTileType.None;
        }
    }

    private string CleanValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        value = value.Trim();

        // CSV에서 "General"처럼 따옴표가 붙은 경우 제거
        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
            value = value.Substring(1, value.Length - 2);
        }

        value = value.Trim();

        // 기존 표 복사 과정에서 들어온 HTML 빈 값 대응
        if (value.Equals("<br>", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return value;
    }
}