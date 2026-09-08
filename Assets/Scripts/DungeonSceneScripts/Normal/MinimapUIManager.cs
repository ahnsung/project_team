using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUIManager : MonoBehaviour
{
    // =========================================================
    // References
    // =========================================================

    [Header("References")]
    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private DungeonMapDatabase mapDatabase;

    [SerializeField]
    private RectTransform gridRoot;

    [SerializeField]
    private GameObject cellPrefab;


    // =========================================================
    // MapTile Sprites
    // =========================================================

    [Header("MapTile Sprites")]

    [SerializeField]
    private Sprite normalSprite;

    [SerializeField]
    private Sprite farmingSprite;

    [SerializeField]
    private Sprite trapSprite;

    [SerializeField]
    private Sprite keySprite;

    [SerializeField]
    private Sprite teleportSprite;

    [SerializeField]
    private Sprite chestSprite;

    [SerializeField]
    private Sprite eventSprite;

    [SerializeField]
    private Sprite puzzleLetterSprite;

    [SerializeField]
    private Sprite restSprite;

    [SerializeField]
    private Sprite bossSprite;


    // =========================================================
    // Current Position
    // =========================================================

    [Header("Current Position")]

    [SerializeField]
    private Color currentBorderColor =
        new Color(
            0.15f,
            0.45f,
            1f,
            1f
        );

    [SerializeField]
    [Range(1f, 8f)]
    private float currentBorderThickness = 2f;


    // =========================================================
    // Minimap View
    // =========================================================

    [Header("Minimap View")]

    [SerializeField]
    [Range(3, 15)]
    private int viewSize = 9;


    // =========================================================
    // Cell
    // =========================================================

    private class MinimapCell
    {
        public Image tileImage;

        public GameObject currentBorder;
    }


    private readonly List<MinimapCell> cells =
        new List<MinimapCell>();


    private GridLayoutGroup gridLayout;


    // =========================================================
    // Unity
    // =========================================================

    private IEnumerator Start()
    {
        if (dungeonManager == null)
        {
            dungeonManager =
                DungeonManager.Instance;
        }


        if (mapDatabase == null)
        {
            mapDatabase =
                DungeonMapDatabase.Instance;
        }


        if (dungeonManager == null)
        {
            Debug.LogError(
                "[MinimapUIManager] " +
                "DungeonManager를 찾을 수 없습니다."
            );

            yield break;
        }


        if (mapDatabase == null)
        {
            Debug.LogError(
                "[MinimapUIManager] " +
                "DungeonMapDatabase를 찾을 수 없습니다."
            );

            yield break;
        }


        if (gridRoot == null)
        {
            Debug.LogError(
                "[MinimapUIManager] " +
                "Grid Root가 연결되지 않았습니다."
            );

            yield break;
        }


        if (cellPrefab == null)
        {
            Debug.LogError(
                "[MinimapUIManager] " +
                "Cell Prefab이 연결되지 않았습니다."
            );

            yield break;
        }


        gridLayout =
            gridRoot.GetComponent<GridLayoutGroup>();


        if (gridLayout == null)
        {
            Debug.LogError(
                "[MinimapUIManager] " +
                "GridRoot에 GridLayoutGroup이 없습니다."
            );

            yield break;
        }


        // -----------------------------------------------------
        // 맵 데이터 로드 대기
        // -----------------------------------------------------

        float timeout = 5f;
        float elapsed = 0f;


        while (
            mapDatabase.MapData == null ||
            mapDatabase.MapData.Tiles.Count == 0
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            if (elapsed >= timeout)
            {
                Debug.LogError(
                    "[MinimapUIManager] " +
                    "맵 데이터 로드를 기다렸지만 " +
                    "5초 안에 완료되지 않았습니다."
                );

                yield break;
            }


            yield return null;
        }


        // Canvas 계산까지 한 프레임 대기
        yield return null;


        if (viewSize % 2 == 0)
        {
            viewSize++;
        }


        ConfigureGrid();

        BuildGrid();

        RefreshMinimap();


        Debug.Log(
            "[MinimapUIManager] " +
            "초기 미니맵 표시 완료"
        );
    }


    // =========================================================
    // Grid 설정
    // =========================================================

    private void ConfigureGrid()
    {
        Canvas.ForceUpdateCanvases();


        Rect rect =
            gridRoot.rect;


        float availableWidth =
            rect.width
            - gridLayout.padding.left
            - gridLayout.padding.right;


        float availableHeight =
            rect.height
            - gridLayout.padding.top
            - gridLayout.padding.bottom;


        float cellWidth =
            availableWidth /
            viewSize;


        float cellHeight =
            availableHeight /
            viewSize;


        float cellSize =
            Mathf.Min(
                cellWidth,
                cellHeight
            );


        gridLayout.spacing =
            Vector2.zero;


        gridLayout.cellSize =
            new Vector2(
                cellSize,
                cellSize
            );


        gridLayout.constraint =
            GridLayoutGroup.Constraint
                .FixedColumnCount;


        gridLayout.constraintCount =
            viewSize;


        gridLayout.startCorner =
            GridLayoutGroup.Corner
                .UpperLeft;


        gridLayout.startAxis =
            GridLayoutGroup.Axis
                .Horizontal;


        gridLayout.childAlignment =
            TextAnchor.MiddleCenter;


        Debug.Log(
            "[MinimapUIManager] " +
            $"미니맵 {viewSize}x{viewSize} 설정 완료 / " +
            $"Cell Size: {cellSize}"
        );
    }


    // =========================================================
    // Grid 생성
    // =========================================================

    private void BuildGrid()
    {
        // 기존 Cell 제거
        for (
            int i = gridRoot.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                gridRoot
                    .GetChild(i)
                    .gameObject
            );
        }


        cells.Clear();


        int cellCount =
            viewSize *
            viewSize;


        for (
            int i = 0;
            i < cellCount;
            i++
        )
        {
            GameObject obj =
                Instantiate(
                    cellPrefab,
                    gridRoot
                );


            Image tileImage =
                obj.GetComponent<Image>();


            if (tileImage == null)
            {
                Debug.LogError(
                    "[MinimapUIManager] " +
                    "CellPrefab에 Image가 없습니다."
                );

                Destroy(obj);

                continue;
            }


            tileImage.preserveAspect =
                true;


            tileImage.enabled =
                false;


            GameObject border =
                CreateCurrentBorder(
                    obj.transform
                );


            border.SetActive(
                false
            );


            MinimapCell cell =
                new MinimapCell
                {
                    tileImage =
                        tileImage,

                    currentBorder =
                        border
                };


            cells.Add(
                cell
            );
        }


        Debug.Log(
            "[MinimapUIManager] " +
            "미니맵 Cell 생성 완료: " +
            cells.Count
        );
    }


    // =========================================================
    // 현재 위치 Border
    // =========================================================

    private GameObject CreateCurrentBorder(
        Transform parent)
    {
        GameObject borderRoot =
            new GameObject(
                "CurrentBorder",
                typeof(RectTransform)
            );


        borderRoot.transform.SetParent(
            parent,
            false
        );


        RectTransform rootRect =
            borderRoot.GetComponent<
                RectTransform>();


        rootRect.anchorMin =
            Vector2.zero;

        rootRect.anchorMax =
            Vector2.one;

        rootRect.offsetMin =
            Vector2.zero;

        rootRect.offsetMax =
            Vector2.zero;


        // 위
        CreateBorderLine(
            borderRoot.transform,
            "Top",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(
                0f,
                -currentBorderThickness
            ),
            Vector2.zero
        );


        // 아래
        CreateBorderLine(
            borderRoot.transform,
            "Bottom",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            Vector2.zero,
            new Vector2(
                0f,
                currentBorderThickness
            )
        );


        // 왼쪽
        CreateBorderLine(
            borderRoot.transform,
            "Left",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            Vector2.zero,
            new Vector2(
                currentBorderThickness,
                0f
            )
        );


        // 오른쪽
        CreateBorderLine(
            borderRoot.transform,
            "Right",
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(
                -currentBorderThickness,
                0f
            ),
            Vector2.zero
        );


        return borderRoot;
    }


    private void CreateBorderLine(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject lineObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image)
            );


        lineObject.transform.SetParent(
            parent,
            false
        );


        RectTransform rect =
            lineObject.GetComponent<
                RectTransform>();


        rect.anchorMin =
            anchorMin;

        rect.anchorMax =
            anchorMax;

        rect.offsetMin =
            offsetMin;

        rect.offsetMax =
            offsetMax;


        Image image =
            lineObject.GetComponent<Image>();


        image.color =
            currentBorderColor;


        image.raycastTarget =
            false;
    }


    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshMinimap()
    {
        if (dungeonManager == null)
        {
            dungeonManager =
                DungeonManager.Instance;
        }


        if (mapDatabase == null)
        {
            mapDatabase =
                DungeonMapDatabase.Instance;
        }


        if (dungeonManager == null ||
            mapDatabase == null)
        {
            return;
        }


        if (cells.Count == 0)
        {
            return;
        }


        Vector2Int center =
            dungeonManager.CurrentRoom;


        int radius =
            viewSize / 2;


        int index = 0;


        /*
         * 화면 위쪽이 높은 Y좌표가 되도록
         * 위 → 아래 순서
         */
        for (
            int row = 0;
            row < viewSize;
            row++
        )
        {
            int offsetY =
                radius - row;


            for (
                int column = 0;
                column < viewSize;
                column++
            )
            {
                if (index >= cells.Count)
                {
                    return;
                }


                int offsetX =
                    column - radius;


                Vector2Int worldPosition =
                    new Vector2Int(
                        center.x + offsetX,
                        center.y + offsetY
                    );


                MinimapCell cell =
                    cells[index];


                index++;


                DungeonTileData tile =
                    mapDatabase.GetTile(
                        worldPosition
                    );


                // ---------------------------------------------
                // 타일 없음 / None
                // ---------------------------------------------

                if (
                    tile == null ||
                    tile.TileType ==
                    DungeonTileType.None
                )
                {
                    HideCell(
                        cell
                    );

                    continue;
                }


                bool isCurrent =
                    worldPosition ==
                    center;


                bool isVisited =
                    dungeonManager.IsVisited(
                        worldPosition
                    );


                // ---------------------------------------------
                // 현재 위치
                // ---------------------------------------------

                if (isCurrent)
                {
                    ShowVisitedTile(
                        cell,
                        tile.TileType
                    );


                    cell.currentBorder
                        .SetActive(true);


                    continue;
                }


                // ---------------------------------------------
                // 방문한 타일
                // ---------------------------------------------

                if (isVisited)
                {
                    ShowVisitedTile(
                        cell,
                        tile.TileType
                    );


                    cell.currentBorder
                        .SetActive(false);


                    continue;
                }


                // ---------------------------------------------
                // 미방문 타일
                //
                // 기획상 아예 보이지 않는다.
                // ---------------------------------------------

                HideCell(
                    cell
                );
            }
        }
    }


    // =========================================================
    // Cell 표시
    // =========================================================

    private void ShowVisitedTile(
        MinimapCell cell,
        DungeonTileType tileType)
    {
        if (cell == null ||
            cell.tileImage == null)
        {
            return;
        }


        Sprite sprite =
            GetTileSprite(
                tileType
            );


        if (sprite == null)
        {
            cell.tileImage.enabled =
                false;

            return;
        }


        cell.tileImage.sprite =
            sprite;


        cell.tileImage.enabled =
            true;
    }


    private void HideCell(
        MinimapCell cell)
    {
        if (cell == null)
        {
            return;
        }


        if (cell.tileImage != null)
        {
            cell.tileImage.enabled =
                false;
        }


        if (cell.currentBorder != null)
        {
            cell.currentBorder
                .SetActive(false);
        }
    }


    // =========================================================
    // TileType -> Sprite
    // =========================================================

    private Sprite GetTileSprite(
        DungeonTileType tileType)
    {
        switch (tileType)
        {
            case DungeonTileType.General:

                return normalSprite;


            case DungeonTileType.Farming:

                return farmingSprite;


            case DungeonTileType.Trap:

                return trapSprite;


            case DungeonTileType.Key:

                return keySprite;


            case DungeonTileType.Teleport:

                return teleportSprite;


            case DungeonTileType.Chest:

                return chestSprite;


            case DungeonTileType.EventHint:

                return eventSprite;


            case DungeonTileType.PuzzleLetter:

                return puzzleLetterSprite;


            case DungeonTileType.Rest:

                if (restSprite != null)
                {
                    return restSprite;
                }

                return normalSprite;


            case DungeonTileType.Boss:

                if (bossSprite != null)
                {
                    return bossSprite;
                }

                return normalSprite;


            default:

                return normalSprite;
        }
    }
}