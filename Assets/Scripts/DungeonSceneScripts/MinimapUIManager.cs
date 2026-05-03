using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 미니맵 UI를 실제로 그려주는 스크립트
// 역할:
// 1) GridRoot 안에 칸 자동 생성
// 2) 현재 위치 / 방문한 칸 / 미방문 칸 스프라이트 변경
public class MinimapUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager; // 현재 방 좌표/방문 정보 가져오기용
    [SerializeField] private RectTransform gridRoot;        // 칸들이 생성될 부모
    [SerializeField] private GameObject cellPrefab;         // 미니맵 칸 1개 프리팹

    [Header("Sprites")]
    [SerializeField] private Sprite hiddenSprite;   // 미방문 칸 스프라이트 (검정)
    [SerializeField] private Sprite visitedSprite;  // 방문 칸 스프라이트 (회색)
    [SerializeField] private Sprite currentSprite;  // 현재 위치 칸 스프라이트 (빨강)

    // "x,y" -> 해당 칸의 Image 저장
    private Dictionary<string, Image> cells = new Dictionary<string, Image>();

    private void Start()
    {
        // 시작 시 미니맵 칸 생성
        BuildGrid();

        // 현재 상태 기준으로 한 번 갱신
        RefreshMinimap();
    }

    // 현재 좌표 / 방문 상태에 맞춰 미니맵 칸 스프라이트 변경
    public void RefreshMinimap()
    {
        int width = dungeonManager.MapWidth;
        int height = dungeonManager.MapHeight;
        Vector2Int current = dungeonManager.CurrentRoom;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                string key = x + "," + y;

                if (!cells.ContainsKey(key))
                    continue;

                Image img = cells[key];

                bool isCurrent = (current.x == x && current.y == y);
                bool isVisited = dungeonManager.IsVisited(x, y);

                // 현재 위치면 빨강
                if (isCurrent)
                    img.sprite = currentSprite;
                // 현재 위치는 아니지만 방문했으면 회색
                else if (isVisited)
                    img.sprite = visitedSprite;
                // 그 외는 검정
                else
                    img.sprite = hiddenSprite;
            }
        }
    }

    // GridRoot에 맵 크기만큼 칸을 자동 생성
    private void BuildGrid()
    {
        int width = dungeonManager.MapWidth;
        int height = dungeonManager.MapHeight;

        // 혹시 기존 칸이 남아 있으면 삭제
        for (int i = gridRoot.childCount - 1; i >= 0; i--)
            Destroy(gridRoot.GetChild(i).gameObject);

        cells.Clear();

        // 위쪽 줄부터 아래로 보이게 하기 위해 row를 뒤집어서 처리
        for (int row = 0; row < height; row++)
        {
            int y = (height - 1) - row;

            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(cellPrefab, gridRoot);
                Image img = obj.GetComponent<Image>();

                // 좌표별로 Image 저장
                cells[x + "," + y] = img;
            }
        }
    }
}