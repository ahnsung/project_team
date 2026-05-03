using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 던전 씬의 UI 중에서
// 1) 방향 선택 패널
// 2) 방향 버튼 활성/비활성
// 3) 미니맵 표시 여부
// 를 관리하는 스크립트
public class DungeonUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject directionPanel; // 상하좌우 버튼 전체 패널
    [SerializeField] private GameObject minimapRoot;    // 미니맵 전체 루트

    [Header("Buttons")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Reference")]
    [SerializeField] private RoomTraversalController roomTraversalController; // 버튼 클릭 시 방 이동 요청할 대상

    private void Start()
    {
        // 시작할 때는 방향 선택 패널 숨김
        HideDirectionPanel();

        // 미니맵은 켜둠
        if (minimapRoot != null)
            minimapRoot.SetActive(true);
    }

    // 현재 방 좌표 기준으로 이동 가능한 방향만 버튼 활성화
    public void RefreshDirectionButtons(Dictionary<MoveDirection, bool> availableDirections)
    {
        if (upButton != null)
            upButton.interactable = availableDirections.ContainsKey(MoveDirection.Up) && availableDirections[MoveDirection.Up];

        if (downButton != null)
            downButton.interactable = availableDirections.ContainsKey(MoveDirection.Down) && availableDirections[MoveDirection.Down];

        if (leftButton != null)
            leftButton.interactable = availableDirections.ContainsKey(MoveDirection.Left) && availableDirections[MoveDirection.Left];

        if (rightButton != null)
            rightButton.interactable = availableDirections.ContainsKey(MoveDirection.Right) && availableDirections[MoveDirection.Right];
    }

    // 방향 선택 패널 표시
    public void ShowDirectionPanel()
    {
        if (directionPanel != null)
            directionPanel.SetActive(true);
    }

    // 방향 선택 패널 숨김
    public void HideDirectionPanel()
    {
        if (directionPanel != null)
            directionPanel.SetActive(false);
    }

    // 위 버튼 클릭 시 호출
    public void OnClickMoveUp()
    {
        if (roomTraversalController != null)
            roomTraversalController.SelectNextRoom(MoveDirection.Up);
    }

    // 아래 버튼 클릭 시 호출
    public void OnClickMoveDown()
    {
        if (roomTraversalController != null)
            roomTraversalController.SelectNextRoom(MoveDirection.Down);
    }

    // 왼쪽 버튼 클릭 시 호출
    public void OnClickMoveLeft()
    {
        if (roomTraversalController != null)
            roomTraversalController.SelectNextRoom(MoveDirection.Left);
    }

    // 오른쪽 버튼 클릭 시 호출
    public void OnClickMoveRight()
    {
        if (roomTraversalController != null)
            roomTraversalController.SelectNextRoom(MoveDirection.Right);
    }
}