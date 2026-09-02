using System;
using System.Collections.Generic;
using UnityEngine;

public class FarmingDataLoader : MonoBehaviour
{
    public static FarmingDataLoader Instance { get; private set; }

    [Header("Farming Data CSV")]
    [SerializeField] private TextAsset farmingDataCsv;

    private readonly Dictionary<Vector2Int, FarmingTileData>
        farmingDataLookup =
            new Dictionary<Vector2Int, FarmingTileData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LoadFarmingData();
    }

    public void LoadFarmingData()
    {
        farmingDataLookup.Clear();

        if (farmingDataCsv == null)
        {
            Debug.LogError(
                "[FarmingDataLoader] Farming_Data CSV가 연결되지 않았습니다."
            );

            return;
        }

        string[] lines =
            farmingDataCsv.text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.RemoveEmptyEntries
            );

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] columns = line.Split(',');

            if (columns.Length < 5)
            {
                Debug.LogWarning(
                    $"[FarmingDataLoader] 잘못된 데이터입니다. Line {i + 1}: {line}"
                );

                continue;
            }

            if (!int.TryParse(columns[0].Trim(), out int x) ||
                !int.TryParse(columns[1].Trim(), out int y) ||
                !int.TryParse(columns[3].Trim(), out int minQuantity) ||
                !int.TryParse(columns[4].Trim(), out int maxQuantity))
            {
                Debug.LogWarning(
                    $"[FarmingDataLoader] 숫자 변환 실패. Line {i + 1}: {line}"
                );

                continue;
            }

            string itemGroup =
                columns[2].Trim();

            if (string.IsNullOrWhiteSpace(itemGroup))
            {
                Debug.LogWarning(
                    $"[FarmingDataLoader] ItemGroup이 비어 있습니다. ({x}, {y})"
                );

                continue;
            }

            FarmingTileData data =
                new FarmingTileData
                {
                    x = x,
                    y = y,
                    itemGroup = itemGroup,
                    minItemQuantity = minQuantity,
                    maxItemQuantity = maxQuantity
                };

            Vector2Int position =
                new Vector2Int(x, y);

            if (farmingDataLookup.ContainsKey(position))
            {
                Debug.LogWarning(
                    $"[FarmingDataLoader] 중복 좌표입니다: ({x}, {y})"
                );

                continue;
            }

            farmingDataLookup.Add(
                position,
                data
            );
        }

        Debug.Log(
            "[FarmingDataLoader] Farming_Data 로드 완료: " +
            farmingDataLookup.Count +
            "개"
        );
    }

    public FarmingTileData GetData(Vector2Int position)
    {
        if (farmingDataLookup.TryGetValue(
            position,
            out FarmingTileData data))
        {
            return data;
        }

        return null;
    }

    public FarmingTileData GetData(int x, int y)
    {
        return GetData(
            new Vector2Int(x, y)
        );
    }

    public bool HasData(Vector2Int position)
    {
        return farmingDataLookup.ContainsKey(position);
    }
}