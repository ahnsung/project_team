using System;
using System.Collections.Generic;
using UnityEngine;

public class TrapDataLoader : MonoBehaviour
{
    public static TrapDataLoader Instance { get; private set; }

    [Header("CSV Data")]
    [SerializeField] private TextAsset trapDataFile;

    private readonly Dictionary<Vector2Int, TrapTileData> trapData =
        new Dictionary<Vector2Int, TrapTileData>();

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
        LoadData();
    }

    // =========================================================
    // CSV 로드
    // =========================================================

    private void LoadData()
    {
        trapData.Clear();

        if (trapDataFile == null)
        {
            Debug.LogError(
                "[TrapDataLoader] Trap_Data.csv가 연결되지 않았습니다."
            );

            return;
        }

        string[] lines =
            trapDataFile.text.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries
            );

        if (lines.Length <= 1)
        {
            Debug.LogWarning(
                "[TrapDataLoader] Trap_Data.csv에 데이터가 없습니다."
            );

            return;
        }

        /*
         * 0번 줄은 헤더:
         *
         * X,Y,TrapType,TrapPossibility,TrapAmount
         */
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] values = line.Split(',');

            if (values.Length < 5)
            {
                Debug.LogWarning(
                    $"[TrapDataLoader] 잘못된 데이터 형식 - {i + 1}번째 줄\n" +
                    line
                );

                continue;
            }

            if (!int.TryParse(values[0].Trim(), out int x) ||
                !int.TryParse(values[1].Trim(), out int y) ||
                !int.TryParse(values[2].Trim(), out int trapType) ||
                !int.TryParse(values[3].Trim(), out int trapPossibility) ||
                !int.TryParse(values[4].Trim(), out int trapAmount))
            {
                Debug.LogWarning(
                    $"[TrapDataLoader] 숫자 변환 실패 - {i + 1}번째 줄\n" +
                    line
                );

                continue;
            }

            Vector2Int position =
                new Vector2Int(x, y);

            if (trapData.ContainsKey(position))
            {
                Debug.LogWarning(
                    $"[TrapDataLoader] 중복 좌표 발견: {position}"
                );

                continue;
            }

            TrapTileData data =
                new TrapTileData();

            data.x = x;
            data.y = y;
            data.trapType = trapType;
            data.trapPossibility = trapPossibility;
            data.trapAmount = trapAmount;

            trapData.Add(
                position,
                data
            );
        }

        Debug.Log(
            $"[TrapDataLoader] Trap_Data 로드 완료: {trapData.Count}개"
        );
    }

    // =========================================================
    // 데이터 검색
    // =========================================================

    public TrapTileData GetData(
        int x,
        int y)
    {
        Vector2Int position =
            new Vector2Int(x, y);

        if (trapData.TryGetValue(
            position,
            out TrapTileData data))
        {
            return data;
        }

        return null;
    }

    public bool HasData(
        int x,
        int y)
    {
        return trapData.ContainsKey(
            new Vector2Int(x, y)
        );
    }
}