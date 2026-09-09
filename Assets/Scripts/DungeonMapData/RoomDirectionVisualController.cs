using UnityEngine;

public class RoomDirectionVisualController : MonoBehaviour
{
    // =========================================================
    // Direction Visual Set
    // =========================================================

    [System.Serializable]
    public class DirectionVisualSet
    {
        [Header("Normal / Open")]
        [SerializeField]
        private GameObject normalVisual;

        [Header("Door")]
        [SerializeField]
        private GameObject doorVisual;

        [Header("One Way")]
        [SerializeField]
        private GameObject oneWayVisual;

        [Header("Locked Door")]
        [SerializeField]
        private GameObject lockedDoorVisual;

        [Header("Gimmick Door")]
        [SerializeField]
        private GameObject gimmickDoorVisual;


        public GameObject NormalVisual
            => normalVisual;

        public GameObject DoorVisual
            => doorVisual;

        public GameObject OneWayVisual
            => oneWayVisual;

        public GameObject LockedDoorVisual
            => lockedDoorVisual;

        public GameObject GimmickDoorVisual
            => gimmickDoorVisual;
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
    // Directions
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

        RefreshVisuals();
    }


    private void Update()
    {
        ResolveReferences();


        if (dungeonManager == null)
            return;


        Vector2Int currentRoom =
            dungeonManager.CurrentRoom;


        /*
         * 방이 바뀌었을 때만 다시 갱신.
         *
         * Transform의
         * Position / Rotation / Scale은
         * 절대 수정하지 않는다.
         */
        if (
            !initialized ||
            currentRoom != lastRoom
        )
        {
            lastRoom =
                currentRoom;

            initialized =
                true;

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
    // Refresh All
    // =========================================================

    public void RefreshVisuals()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            Debug.LogWarning(
                "[RoomDirectionVisual] " +
                "DungeonManager가 없습니다."
            );

            HideAll();

            return;
        }


        RefreshDirection(
            MoveDirection.Up,
            up
        );


        RefreshDirection(
            MoveDirection.Down,
            down
        );


        RefreshDirection(
            MoveDirection.Left,
            left
        );


        RefreshDirection(
            MoveDirection.Right,
            right
        );


        lastRoom =
            dungeonManager.CurrentRoom;

        initialized =
            true;


        Print(
            "갱신 완료 / 현재 위치: " +
            dungeonManager.CurrentRoom
        );
    }


    // =========================================================
    // Direction
    // =========================================================

    private void RefreshDirection(
        MoveDirection direction,
        DirectionVisualSet visualSet)
    {
        if (visualSet == null)
            return;


        /*
         * 이전 방에서 켜져 있던
         * 방향 이미지만 전부 끈다.
         *
         * Transform은 건드리지 않는다.
         */
        HideDirection(
            visualSet
        );


        // =====================================================
        // MoveData
        // =====================================================

        MoveData data =
            dungeonManager.GetMoveData(
                direction
            );


        if (data == null)
        {
            Print(
                direction +
                " / MoveData 없음 -> OFF"
            );

            return;
        }


        // =====================================================
        // Open
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Open
        )
        {
            bool canMove =
                dungeonManager.CanMove(
                    direction
                );


            if (!canMove)
            {
                Print(
                    direction +
                    " / Open / 이동 불가 -> OFF"
                );

                return;
            }


            /*
             * 일반 이동 가능.
             *
             * Normal Visual에 연결된
             * 이미지 오브젝트 표시.
             */
            SetVisual(
                visualSet.NormalVisual,
                true
            );


            Print(
                direction +
                " / Open / Passable -> Normal ON"
            );

            return;
        }


        // =====================================================
        // Door
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Door
        )
        {
            bool canMove =
                dungeonManager.CanMove(
                    direction
                );


            if (!canMove)
            {
                Print(
                    direction +
                    " / Door / 이동 불가 -> OFF"
                );

                return;
            }


            if (
                visualSet.DoorVisual !=
                null
            )
            {
                SetVisual(
                    visualSet.DoorVisual,
                    true
                );
            }
            else
            {
                /*
                 * Door 전용 이미지가 없으면
                 * Normal 이미지 사용.
                 */
                SetVisual(
                    visualSet.NormalVisual,
                    true
                );
            }


            Print(
                direction +
                " / Door -> Door ON"
            );

            return;
        }


        // =====================================================
        // One Way
        // =====================================================

        if (
            data.PathType ==
            MovePathType.OneWay
        )
        {
            bool canMove =
                dungeonManager.CanMove(
                    direction
                );


            /*
             * OneWay
             *
             * 현재 방향에서 실제 통과 가능:
             *      OneWay 이미지 ON
             *
             * 역방향이라 통과 불가능:
             *      아무것도 표시하지 않음
             */
            if (!canMove)
            {
                Print(
                    direction +
                    " / OneWay / 역방향 -> OFF"
                );

                return;
            }


            if (
                visualSet.OneWayVisual !=
                null
            )
            {
                SetVisual(
                    visualSet.OneWayVisual,
                    true
                );
            }
            else
            {
                /*
                 * 아직 해당 방향 OneWay 이미지가 없으면
                 * Normal 이미지로 임시 표시.
                 */
                SetVisual(
                    visualSet.NormalVisual,
                    true
                );
            }


            Print(
                direction +
                " / OneWay / 정방향 -> OneWay ON"
            );

            return;
        }


        // =====================================================
        // Locked Door
        // =====================================================

        if (
            data.PathType ==
            MovePathType.LockedDoor
        )
        {
            /*
             * LockedDoor는 현재 잠겨서
             * 통과 불가능하더라도 문 자체는 보여준다.
             */


            if (
                visualSet.LockedDoorVisual !=
                null
            )
            {
                SetVisual(
                    visualSet.LockedDoorVisual,
                    true
                );
            }
            else if (
                visualSet.DoorVisual !=
                null
            )
            {
                /*
                 * LockedDoor 전용 이미지가 아직 없으면
                 * Door 이미지 사용.
                 */
                SetVisual(
                    visualSet.DoorVisual,
                    true
                );
            }
            else
            {
                SetVisual(
                    visualSet.NormalVisual,
                    true
                );
            }


            Print(
                direction +
                " / LockedDoor -> ON"
            );

            return;
        }


        // =====================================================
        // Gimmick Door
        // =====================================================

        if (
            data.PathType ==
            MovePathType.GimmickDoor
        )
        {
            if (
                visualSet.GimmickDoorVisual !=
                null
            )
            {
                SetVisual(
                    visualSet.GimmickDoorVisual,
                    true
                );
            }
            else if (
                visualSet.DoorVisual !=
                null
            )
            {
                SetVisual(
                    visualSet.DoorVisual,
                    true
                );
            }
            else
            {
                SetVisual(
                    visualSet.NormalVisual,
                    true
                );
            }


            Print(
                direction +
                " / GimmickDoor -> ON"
            );

            return;
        }


        // =====================================================
        // Wall / Unknown
        // =====================================================

        Print(
            direction +
            " / " +
            data.PathType +
            " -> OFF"
        );
    }


    // =========================================================
    // Hide Direction
    // =========================================================

    private void HideDirection(
        DirectionVisualSet visualSet)
    {
        if (visualSet == null)
            return;


        SetVisual(
            visualSet.NormalVisual,
            false
        );


        SetVisual(
            visualSet.DoorVisual,
            false
        );


        SetVisual(
            visualSet.OneWayVisual,
            false
        );


        SetVisual(
            visualSet.LockedDoorVisual,
            false
        );


        SetVisual(
            visualSet.GimmickDoorVisual,
            false
        );
    }


    // =========================================================
    // Hide All
    // =========================================================

    private void HideAll()
    {
        HideDirection(
            up
        );


        HideDirection(
            down
        );


        HideDirection(
            left
        );


        HideDirection(
            right
        );
    }


    // =========================================================
    // Set Active
    // =========================================================

    private void SetVisual(
        GameObject target,
        bool active)
    {
        if (target == null)
            return;


        /*
         * =====================================================
         * 매우 중요
         * =====================================================
         *
         * 이 스크립트에서는
         * 오직 SetActive만 사용한다.
         *
         * 아래 값들은 절대 변경하지 않는다.
         *
         * transform.position
         * transform.localPosition
         * transform.rotation
         * transform.localRotation
         * transform.localScale
         *
         * 따라서 Unity에서 직접 설정한
         * 위치 / 회전 / 크기가 그대로 유지된다.
         */

        if (
            target.activeSelf !=
            active
        )
        {
            target.SetActive(
                active
            );
        }
    }


    // =========================================================
    // Debug
    // =========================================================

    private void Print(
        string message)
    {
        if (!printLog)
            return;


        Debug.Log(
            "[RoomDirectionVisual] " +
            message
        );
    }
}