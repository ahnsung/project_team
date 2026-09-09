using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 방에서 이동 가능한 방향에 따라
/// 상 / 하 / 좌 / 우 방향 오브젝트를 표시하거나 숨긴다.
///
/// 예:
/// UpVisual    = 위쪽 사다리
/// DownVisual  = 아래쪽 사다리
/// LeftVisual  = 왼쪽 통로 이미지
/// RightVisual = 오른쪽 통로 이미지
/// </summary>
public class DungeonDirectionVisuals : MonoBehaviour
{
    [Header("Direction Visual Objects")]
    [SerializeField] private GameObject upVisual;
    [SerializeField] private GameObject downVisual;
    [SerializeField] private GameObject leftVisual;
    [SerializeField] private GameObject rightVisual;

    /// <summary>
    /// DungeonManager에서 계산한 이동 가능 방향을 받아
    /// 해당 방향 오브젝트를 켜거나 끈다.
    /// </summary>
    public void RefreshVisuals(
        Dictionary<MoveDirection, bool> availableDirections)
    {
        if (availableDirections == null)
            return;

        SetVisual(
            upVisual,
            IsAvailable(availableDirections, MoveDirection.Up)
        );

        SetVisual(
            downVisual,
            IsAvailable(availableDirections, MoveDirection.Down)
        );

        SetVisual(
            leftVisual,
            IsAvailable(availableDirections, MoveDirection.Left)
        );

        SetVisual(
            rightVisual,
            IsAvailable(availableDirections, MoveDirection.Right)
        );
    }

    private bool IsAvailable(
        Dictionary<MoveDirection, bool> directions,
        MoveDirection direction)
    {
        return directions.ContainsKey(direction)
               && directions[direction];
    }

    private void SetVisual(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}