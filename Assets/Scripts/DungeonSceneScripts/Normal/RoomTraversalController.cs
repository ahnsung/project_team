using System.Collections;
using UnityEngine;

public class RoomTraversalController : MonoBehaviour
{
    private enum RoomState
    {
        EventRunning,
        WaitingForInput,
        DirectionChoosing,
        Transition
    }


    [Header("Points")]
    [SerializeField]
    private Transform playerCenterPoint;


    [Header("Managers")]
    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private DungeonUIManager uiManager;

    [SerializeField]
    private FadeController fadeController;

    [SerializeField]
    private DungeonTileEventManager
        tileEventManager;

    [SerializeField]
    private CameraRoomTransition
        cameraRoomTransition;


    private RoomState state;

    private bool isTransitioning;

    private bool isInteracting;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();


        if (playerCenterPoint != null)
        {
            transform.position =
                playerCenterPoint.position;
        }


        if (uiManager != null)
        {
            uiManager.HideDirectionPanel();
        }


        StartCoroutine(
            RoomStartRoutine()
        );
    }


    private void Update()
    {
        if (
            state !=
            RoomState.WaitingForInput
        )
        {
            return;
        }


        if (
            isTransitioning ||
            isInteracting
        )
        {
            return;
        }


        // =====================================================
        // E
        // =====================================================

        if (
            Input.GetKeyDown(
                KeyCode.E
            )
        )
        {
            TryInteract();

            return;
        }


        // =====================================================
        // Space
        // =====================================================

        if (
            Input.GetKeyDown(
                KeyCode.Space
            )
        )
        {
            OpenDirectionPanel();

            return;
        }


        // =====================================================
        // WASD
        // =====================================================

        if (
            Input.GetKeyDown(
                KeyCode.W
            ) ||
            Input.GetKeyDown(
                KeyCode.UpArrow
            )
        )
        {
            TryOpenMove(
                MoveDirection.Up
            );

            return;
        }


        if (
            Input.GetKeyDown(
                KeyCode.S
            ) ||
            Input.GetKeyDown(
                KeyCode.DownArrow
            )
        )
        {
            TryOpenMove(
                MoveDirection.Down
            );

            return;
        }


        if (
            Input.GetKeyDown(
                KeyCode.A
            ) ||
            Input.GetKeyDown(
                KeyCode.LeftArrow
            )
        )
        {
            TryOpenMove(
                MoveDirection.Left
            );

            return;
        }


        if (
            Input.GetKeyDown(
                KeyCode.D
            ) ||
            Input.GetKeyDown(
                KeyCode.RightArrow
            )
        )
        {
            TryOpenMove(
                MoveDirection.Right
            );

            return;
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


        if (tileEventManager == null)
        {
            tileEventManager =
                DungeonTileEventManager.Instance;
        }


        if (uiManager == null)
        {
            uiManager =
                FindFirstObjectByType<
                    DungeonUIManager
                >();
        }


        if (fadeController == null)
        {
            fadeController =
                FindFirstObjectByType<
                    FadeController
                >();
        }


        if (
            cameraRoomTransition == null
        )
        {
            cameraRoomTransition =
                FindFirstObjectByType<
                    CameraRoomTransition
                >();
        }
    }


    // =========================================================
    // Start
    // =========================================================

    private IEnumerator RoomStartRoutine()
    {
        yield return StartCoroutine(
            RunRoomEnterEvent()
        );
    }


    // =========================================================
    // Enter Event
    // =========================================================

    private IEnumerator RunRoomEnterEvent()
    {
        state =
            RoomState.EventRunning;


        if (playerCenterPoint != null)
        {
            transform.position =
                playerCenterPoint.position;
        }


        ResolveReferences();


        if (tileEventManager != null)
        {
            yield return StartCoroutine(
                tileEventManager
                    .ExecuteEnterEvent()
            );
        }


        state =
            RoomState.WaitingForInput;
    }


    // =========================================================
    // Interaction
    // =========================================================

    private void TryInteract()
    {
        if (
            isInteracting ||
            isTransitioning
        )
        {
            return;
        }


        ResolveReferences();


        // =========================================================
        // Base Camp Exit
        // =========================================================

        if (
            BaseCampExitManager.Instance != null &&
            BaseCampExitManager.Instance
                .IsAtBaseCamp()
        )
        {
            bool opened =
                BaseCampExitManager.Instance
                    .TryOpenExitConfirm();


            if (opened)
            {
                return;
            }
        }


        // =========================================================
        // Normal Tile Interaction
        // =========================================================

        if (tileEventManager == null)
        {
            return;
        }


        if (
            !tileEventManager
                .CanInteractCurrentTile()
        )
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "현재 타일에는 상호작용할 것이 없습니다."
            );

            return;
        }


        StartCoroutine(
            InteractionRoutine()
        );
    }


    private IEnumerator InteractionRoutine()
    {
        isInteracting =
            true;


        state =
            RoomState.EventRunning;


        yield return StartCoroutine(
            tileEventManager
                .ExecuteInteraction()
        );


        state =
            RoomState.WaitingForInput;


        isInteracting =
            false;
    }


    // =========================================================
    // WASD Open
    // =========================================================

    private void TryOpenMove(
        MoveDirection direction)
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            return;
        }


        MoveData moveData =
            dungeonManager.GetMoveData(
                direction
            );


        if (moveData == null)
        {
            Debug.Log(
                "[이동] Move Data가 없습니다."
            );

            return;
        }


        Debug.Log(
            "[이동 입력]\n" +
            $"현재 위치: {dungeonManager.CurrentRoom}\n" +
            $"방향: {direction}\n" +
            $"Type: {moveData.PathType}\n" +
            $"Passable: {moveData.Passable}"
        );


        if (!moveData.Passable)
        {
            Debug.Log(
                "[이동] 이동할 수 없는 방향입니다."
            );

            return;
        }


        if (
            moveData.PathType !=
            MovePathType.Open
        )
        {
            Debug.Log(
                "[이동] 이 방향은 Open이 아닙니다.\n" +
                $"Type: {moveData.PathType}"
            );

            return;
        }


        if (
            !dungeonManager.CanMoveOpen(
                direction
            )
        )
        {
            return;
        }


        StartCoroutine(
            ChangeRoom(
                direction,
                false
            )
        );
    }


    // =========================================================
    // Space
    // =========================================================

    private void OpenDirectionPanel()
    {
        if (
            isTransitioning ||
            isInteracting
        )
        {
            return;
        }


        ResolveReferences();


        if (dungeonManager == null)
        {
            return;
        }


        if (
            !dungeonManager.HasAnySpecialPath()
        )
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "현재 위치에는 Space로 사용할 " +
                "Door / OneWay / LockedDoor 계열 통로가 없습니다."
            );

            return;
        }


        if (uiManager != null)
        {
            uiManager
                .ShowSpecialDirectionPanel(
                    dungeonManager
                        .GetSpecialDirections()
                );
        }


        state =
            RoomState.DirectionChoosing;


        Debug.Log(
            "[RoomTraversalController] " +
            "특수 통로 방향 선택 패널 열기"
        );
    }


    // =========================================================
    // Close
    // =========================================================

    public void CloseDirectionPanel()
    {
        if (isTransitioning)
        {
            return;
        }


        if (uiManager != null)
        {
            uiManager
                .HideDirectionPanel();
        }


        state =
            RoomState.WaitingForInput;


        Debug.Log(
            "[RoomTraversalController] " +
            "방향 선택 취소"
        );
    }


    // =========================================================
    // Select Special Path
    // =========================================================

    public void SelectNextRoom(
        MoveDirection direction)
    {
        if (
            state !=
            RoomState.DirectionChoosing
        )
        {
            return;
        }


        if (isTransitioning)
        {
            return;
        }


        ResolveReferences();


        if (dungeonManager == null)
        {
            return;
        }


        MoveData moveData =
            dungeonManager.GetMoveData(
                direction
            );


        if (moveData == null)
        {
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
            LockedDoorManager
                lockedDoorManager =
                    LockedDoorManager.Instance;


            if (lockedDoorManager == null)
            {
                Debug.LogError(
                    "[RoomTraversalController] " +
                    "LockedDoorManager가 없습니다."
                );

                return;
            }


            bool opened =
                lockedDoorManager.IsOpened(
                    dungeonManager.CurrentRoom,
                    direction
                );


            if (!opened)
            {
                opened =
                    lockedDoorManager.TryOpenDoor(
                        dungeonManager.CurrentRoom,
                        direction
                    );
            }


            if (!opened)
            {
                Debug.Log(
                    "[RoomTraversalController] " +
                    "잠긴 문을 열 수 없습니다."
                );

                return;
            }
        }


        // =====================================================
        // Gimmick
        // =====================================================

        if (
            moveData.PathType ==
            MovePathType.GimmickDoor
        )
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "GimmickDoor는 아직 구현되지 않았습니다."
            );

            return;
        }


        // =====================================================
        // Door / OneWay Validation
        // =====================================================

        if (
            moveData.PathType !=
            MovePathType.LockedDoor &&
            !dungeonManager
                .CanUseSpecialPath(
                    direction
                )
        )
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "사용할 수 없는 특수 통로입니다."
            );

            return;
        }


        Debug.Log(
            "[특수 이동 선택]\n" +
            $"현재 위치: {dungeonManager.CurrentRoom}\n" +
            $"방향: {direction}\n" +
            $"Type: {moveData.PathType}"
        );


        StartCoroutine(
            ChangeRoom(
                direction,
                true
            )
        );
    }


    // =========================================================
    // Change Room
    // =========================================================

    private IEnumerator ChangeRoom(
        MoveDirection direction,
        bool specialMove)
    {
        isTransitioning =
            true;


        state =
            RoomState.Transition;


        if (uiManager != null)
        {
            uiManager
                .HideDirectionPanel();
        }


        // Camera movement
        if (
            cameraRoomTransition != null
        )
        {
            yield return
                cameraRoomTransition
                    .PlayRoomMove(
                        direction
                    );
        }


        // Fade Out
        if (fadeController != null)
        {
            yield return
                fadeController
                    .FadeOut();
        }


        bool moved =
            false;


        if (dungeonManager != null)
        {
            moved =
                dungeonManager
                    .MoveToNextRoom(
                        direction
                    );
        }


        if (playerCenterPoint != null)
        {
            transform.position =
                playerCenterPoint.position;
        }


        if (
            cameraRoomTransition != null
        )
        {
            cameraRoomTransition
                .ResetCameraPosition();
        }


        // Fade In
        if (fadeController != null)
        {
            yield return
                fadeController
                    .FadeIn();
        }


        isTransitioning =
            false;


        if (!moved)
        {
            state =
                RoomState.WaitingForInput;

            yield break;
        }


        Debug.Log(
            "[RoomTraversalController] 이동 완료\n" +
            "이동 방식: " +
            (
                specialMove
                ? "특수 통로"
                : "Open"
            ) +
            "\n현재 위치: " +
            dungeonManager.CurrentRoom
        );


        yield return StartCoroutine(
            RunRoomEnterEvent()
        );
    }
}