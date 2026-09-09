using System;
using UnityEngine;

public class RoomDirectionVisualController : MonoBehaviour
{
    // =========================================================
    // Direction Visual Set
    // =========================================================

    [Serializable]
    public class DirectionVisualSet
    {
        [Header("Normal / Open")]
        public GameObject normalVisual;

        [Header("Door")]
        public GameObject doorVisual;

        [Header("One Way")]
        public GameObject oneWayVisual;

        [Header("Locked Door")]
        public GameObject lockedDoorVisual;
    }


    // =========================================================
    // References
    // =========================================================

    [Header("References")]

    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private MoveDataLoader moveDataLoader;


    // =========================================================
    // Direction Visuals
    // =========================================================

    [Header("Up")]
    [SerializeField]
    private DirectionVisualSet up;


    [Header("Down")]
    [SerializeField]
    private DirectionVisualSet down;


    [Header("Left")]
    [SerializeField]
    private DirectionVisualSet left;


    [Header("Right")]
    [SerializeField]
    private DirectionVisualSet right;


    // =========================================================
    // Visual Scale
    // =========================================================

    [Header("Visual Scale")]

    [Tooltip(
        "방향 이미지 전체 크기 배율. " +
        "현재 리소스가 작으면 3~5 정도로 테스트하세요."
    )]
    [SerializeField]
    private float visualScale = 4f;


    // =========================================================
    // Debug
    // =========================================================

    [Header("Debug")]

    [SerializeField]
    private bool printLog = true;


    // =========================================================
    // Runtime
    // =========================================================

    private Vector2Int lastRoom;

    private bool initialized;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();

        ApplyScale();

        RefreshVisuals();
    }


    private void Update()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            return;
        }


        Vector2Int currentRoom =
            dungeonManager.CurrentRoom;


        if (
            !initialized ||
            currentRoom != lastRoom
        )
        {
            RefreshVisuals();
        }
    }


    // =========================================================
    // References
    // =========================================================

    private void ResolveReferences()
    {
        if (dungeonManager == null)
        {
            dungeonManager =
                DungeonManager.Instance;
        }


        if (moveDataLoader == null)
        {
            moveDataLoader =
                MoveDataLoader.Instance;
        }
    }


    // =========================================================
    // Scale
    // =========================================================

    private void ApplyScale()
    {
        ApplyScaleToSet(up);
        ApplyScaleToSet(down);
        ApplyScaleToSet(left);
        ApplyScaleToSet(right);
    }


    private void ApplyScaleToSet(
        DirectionVisualSet set)
    {
        if (set == null)
        {
            return;
        }


        ApplyScaleToObject(
            set.normalVisual
        );

        ApplyScaleToObject(
            set.doorVisual
        );

        ApplyScaleToObject(
            set.oneWayVisual
        );

        ApplyScaleToObject(
            set.lockedDoorVisual
        );
    }


    private void ApplyScaleToObject(
        GameObject target)
    {
        if (target == null)
        {
            return;
        }


        target.transform.localScale =
            Vector3.one * visualScale;
    }


    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshVisuals()
    {
        ResolveReferences();


        if (
            dungeonManager == null ||
            moveDataLoader == null
        )
        {
            return;
        }


        Vector2Int currentRoom =
            dungeonManager.CurrentRoom;


        RefreshDirection(
            currentRoom,
            MoveDirection.Up,
            up
        );


        RefreshDirection(
            currentRoom,
            MoveDirection.Down,
            down
        );


        RefreshDirection(
            currentRoom,
            MoveDirection.Left,
            left
        );


        RefreshDirection(
            currentRoom,
            MoveDirection.Right,
            right
        );


        lastRoom =
            currentRoom;


        initialized =
            true;


        if (printLog)
        {
            Debug.Log(
                "[RoomDirectionVisual] 갱신 완료\n" +
                $"현재 위치: {currentRoom}"
            );
        }
    }


    // =========================================================
    // Direction Logic
    // =========================================================

    private void RefreshDirection(
        Vector2Int room,
        MoveDirection direction,
        DirectionVisualSet visualSet)
    {
        if (visualSet == null)
        {
            return;
        }


        DisableAll(
            visualSet
        );


        MoveData moveData =
            moveDataLoader.GetMoveData(
                room,
                direction
            );


        // =====================================================
        // No Data
        // =====================================================

        if (moveData == null)
        {
            PrintResult(
                direction,
                "데이터 없음 → OFF"
            );

            return;
        }


        // =====================================================
        // Wall
        // =====================================================

        if (
            moveData.PathType ==
            MovePathType.Wall
        )
        {
            PrintResult(
                direction,
                "Wall → OFF"
            );

            return;
        }


        // =====================================================
        // Open
        // =====================================================

        if (
            moveData.PathType ==
            MovePathType.Open
        )
        {
            if (moveData.Passable)
            {
                Activate(
                    visualSet.normalVisual
                );

                PrintResult(
                    direction,
                    "Open / Passable → Normal ON"
                );
            }
            else
            {
                PrintResult(
                    direction,
                    "Open / Not Passable → OFF"
                );
            }


            return;
        }


        // =====================================================
        // Door
        // =====================================================

        if (
            moveData.PathType ==
            MovePathType.Door
        )
        {
            if (moveData.Passable)
            {
                if (visualSet.doorVisual != null)
                {
                    Activate(
                        visualSet.doorVisual
                    );
                }
                else
                {
                    Activate(
                        visualSet.normalVisual
                    );
                }


                PrintResult(
                    direction,
                    "Door → ON"
                );
            }
            else
            {
                PrintResult(
                    direction,
                    "Door / Not Passable → OFF"
                );
            }


            return;
        }


        // =====================================================
        // One Way
        // =====================================================

        if (
            moveData.PathType ==
            MovePathType.OneWay
        )
        {
            if (moveData.Passable)
            {
                if (visualSet.oneWayVisual != null)
                {
                    Activate(
                        visualSet.oneWayVisual
                    );
                }
                else
                {
                    Activate(
                        visualSet.normalVisual
                    );
                }


                PrintResult(
                    direction,
                    "OneWay / Passable → ON"
                );
            }
            else
            {
                PrintResult(
                    direction,
                    "OneWay / 역방향 → OFF"
                );
            }


            return;
        }


        // =====================================================
        // Locked Door
        // =====================================================

        if (
            moveData.PathType ==
            MovePathType.LockedDoor
        )
        {
            bool opened =
                LockedDoorManager.Instance != null &&
                LockedDoorManager.Instance.IsOpened(
                    room,
                    direction
                );


            if (opened)
            {
                if (visualSet.doorVisual != null)
                {
                    Activate(
                        visualSet.doorVisual
                    );
                }
                else
                {
                    Activate(
                        visualSet.normalVisual
                    );
                }


                PrintResult(
                    direction,
                    "LockedDoor / Opened → ON"
                );
            }
            else
            {
                /*
                 * 잠긴 문도 실제로 '길이 존재한다'는 것을
                 * 보여주려면 LockedDoor 이미지를 표시한다.
                 */

                if (visualSet.lockedDoorVisual != null)
                {
                    Activate(
                        visualSet.lockedDoorVisual
                    );

                    PrintResult(
                        direction,
                        "LockedDoor / Locked → Locked ON"
                    );
                }
                else
                {
                    PrintResult(
                        direction,
                        "LockedDoor / Locked / 이미지 없음 → OFF"
                    );
                }
            }


            return;
        }


        // =====================================================
        // Gimmick Door
        // =====================================================

        if (
            moveData.PathType ==
            MovePathType.GimmickDoor
        )
        {
            /*
             * 아직 실제 Gimmick 구현 전.
             *
             * 길 존재 여부는 보여주되
             * LockedDoor 이미지가 있으면 임시 사용.
             */

            if (visualSet.lockedDoorVisual != null)
            {
                Activate(
                    visualSet.lockedDoorVisual
                );
            }


            PrintResult(
                direction,
                "GimmickDoor → 임시 표시"
            );


            return;
        }
    }


    // =========================================================
    // Object Control
    // =========================================================

    private void DisableAll(
        DirectionVisualSet set)
    {
        SetActive(
            set.normalVisual,
            false
        );

        SetActive(
            set.doorVisual,
            false
        );

        SetActive(
            set.oneWayVisual,
            false
        );

        SetActive(
            set.lockedDoorVisual,
            false
        );
    }


    private void Activate(
        GameObject target)
    {
        if (target == null)
        {
            return;
        }


        target.SetActive(
            true
        );
    }


    private void SetActive(
        GameObject target,
        bool active)
    {
        if (target == null)
        {
            return;
        }


        target.SetActive(
            active
        );
    }


    // =========================================================
    // Debug
    // =========================================================

    private void PrintResult(
        MoveDirection direction,
        string message)
    {
        if (!printLog)
        {
            return;
        }


        Debug.Log(
            "[RoomDirectionVisual] " +
            $"{direction} / {message}"
        );
    }


    [ContextMenu(
        "DEBUG - Visual 강제 갱신"
    )]
    private void DebugRefresh()
    {
        initialized =
            false;

        RefreshVisuals();
    }


    [ContextMenu(
        "DEBUG - Scale 다시 적용"
    )]
    private void DebugApplyScale()
    {
        ApplyScale();
    }
}