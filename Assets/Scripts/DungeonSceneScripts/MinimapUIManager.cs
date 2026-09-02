using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private DungeonMapDatabase mapDatabase;
    [SerializeField] private RectTransform gridRoot;
    [SerializeField] private GameObject cellPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private Sprite visitedSprite;
    [SerializeField] private Sprite currentSprite;

    [Header("Minimap View")]
    [SerializeField]
    [Range(3, 15)]
    private int viewSize = 9;

    private readonly List<Image> cells =
        new List<Image>();

    private GridLayoutGroup gridLayout;

    private IEnumerator Start()
    {
        if (dungeonManager == null)
            dungeonManager = DungeonManager.Instance;

        if (mapDatabase == null)
            mapDatabase = DungeonMapDatabase.Instance;

        if (dungeonManager == null)
        {
            Debug.LogError(
                "[MinimapUIManager] DungeonManager를 찾을 수 없습니다."
            );

            yield break;
        }

        if (mapDatabase == null)
        {
            Debug.LogError(
                "[MinimapUIManager] DungeonMapDatabase를 찾을 수 없습니다."
            );

            yield break;
        }

        if (gridRoot == null)
        {
            Debug.LogError(
                "[MinimapUIManager] Grid Root가 연결되지 않았습니다."
            );

            yield break;
        }

        if (cellPrefab == null)
        {
            Debug.LogError(
                "[MinimapUIManager] Cell Prefab이 연결되지 않았습니다."
            );

            yield break;
        }

        gridLayout =
            gridRoot.GetComponent<GridLayoutGroup>();

        if (gridLayout == null)
        {
            Debug.LogError(
                "[MinimapUIManager] GridRoot에 GridLayoutGroup이 없습니다."
            );

            yield break;
        }

        /*
         * DungeonMapLoader.Start()보다
         * 미니맵 Start()가 먼저 실행될 수 있으므로
         * 실제 Tile_Data 등록이 끝날 때까지 기다린다.
         */
        float timeout = 5f;
        float elapsed = 0f;

        while (
            mapDatabase.MapData == null ||
            mapDatabase.MapData.Tiles.Count == 0
        )
        {
            elapsed += Time.unscaledDeltaTime;

            if (elapsed >= timeout)
            {
                Debug.LogError(
                    "[MinimapUIManager] 맵 데이터 로드를 기다렸지만 " +
                    "5초 안에 완료되지 않았습니다."
                );

                yield break;
            }

            yield return null;
        }

        /*
         * 한 프레임 더 기다려서
         * Canvas RectTransform 계산도 끝나도록 한다.
         */
        yield return null;

        if (viewSize % 2 == 0)
            viewSize++;

        ConfigureGrid();
        BuildGrid();
        RefreshMinimap();

        Debug.Log(
            "[MinimapUIManager] 초기 미니맵 표시 완료"
        );
    }

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
            availableWidth / viewSize;

        float cellHeight =
            availableHeight / viewSize;

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
            GridLayoutGroup.Constraint.FixedColumnCount;

        gridLayout.constraintCount =
            viewSize;

        gridLayout.startCorner =
            GridLayoutGroup.Corner.UpperLeft;

        gridLayout.startAxis =
            GridLayoutGroup.Axis.Horizontal;

        gridLayout.childAlignment =
            TextAnchor.MiddleCenter;

        Debug.Log(
            "[MinimapUIManager] " +
            $"미니맵 {viewSize}x{viewSize} 설정 완료 / " +
            $"Cell Size: {cellSize}"
        );
    }

    private void BuildGrid()
    {
        /*
         * 기존 44x43용 Cell들을 모두 제거한다.
         */
        for (
            int i = gridRoot.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                gridRoot.GetChild(i).gameObject
            );
        }

        cells.Clear();

        int cellCount =
            viewSize * viewSize;

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

            Image image =
                obj.GetComponent<Image>();

            if (image == null)
            {
                Debug.LogError(
                    "[MinimapUIManager] CellPrefab에 Image가 없습니다."
                );

                continue;
            }

            image.sprite =
                hiddenSprite;

            cells.Add(image);
        }

        Debug.Log(
            "[MinimapUIManager] 미니맵 Cell 생성 완료: " +
            cells.Count
        );
    }

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
         * 위 → 아래 순서로 생성한다.
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

                Image image =
                    cells[index];

                index++;

                DungeonTileData tile =
                    mapDatabase.GetTile(
                        worldPosition
                    );

                /*
                 * Tile_Data에 없거나 None이면
                 * 미니맵에서 투명 처리.
                 */
                if (
                    tile == null ||
                    tile.TileType ==
                    DungeonTileType.None
                )
                {
                    image.enabled = false;

                    continue;
                }

                image.enabled = true;

                bool isCurrent =
                    worldPosition == center;

                bool isVisited =
                    dungeonManager.IsVisited(
                        worldPosition
                    );

                if (isCurrent)
                {
                    image.sprite =
                        currentSprite;
                }
                else if (isVisited)
                {
                    image.sprite =
                        visitedSprite;
                }
                else
                {
                    image.sprite =
                        hiddenSprite;
                }
            }
        }
    }
}