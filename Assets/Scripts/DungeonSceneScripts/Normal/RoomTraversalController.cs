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
    private DungeonTileEventManager tileEventManager;

    [SerializeField]
    private CameraRoomTransition cameraRoomTransition;


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
        /*
         * 방향 선택 중에는
         * 일반 입력을 받지 않는다.
         *
         * 단, 패널 X 버튼은
         * CloseDirectionPanel()을 호출해서
         * WaitingForInput으로 되돌려야 한다.
         */
        if (state !=
            RoomState.WaitingForInput)
        {
            return;
        }


        // =====================================================
        // E = 현재 타일 상호작용
        // =====================================================

        if (Input.GetKeyDown(
            KeyCode.E))
        {
            TryInteract();

            return;
        }


        // =====================================================
        // Space = 방향 선택 패널
        //
        // 현재는 임시 이동 방식.
        //
        // 최종:
        // WASD / 방향키 = Open
        // Space = Door / OneWay
        // =====================================================

        if (Input.GetKeyDown(
            KeyCode.Space))
        {
            OpenDirectionPanel();

            return;
        }
    }


    // =========================================================
    // Reference
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
    }


    // =========================================================
    // Room Start
    // =========================================================

    private IEnumerator RoomStartRoutine()
    {
        yield return StartCoroutine(
            RunRoomEnterEvent()
        );
    }


    // =========================================================
    // Room Enter Event
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
        else
        {
            Debug.LogWarning(
                "[RoomTraversalController] " +
                "DungeonTileEventManager가 없습니다."
            );
        }


        state =
            RoomState.WaitingForInput;
    }


    // =========================================================
    // E Interaction
    // =========================================================

    private void TryInteract()
    {
        if (isInteracting)
            return;


        ResolveReferences();


        if (tileEventManager == null)
        {
            Debug.LogWarning(
                "[RoomTraversalController] " +
                "DungeonTileEventManager가 없습니다."
            );

            return;
        }


        if (!tileEventManager
            .CanInteractCurrentTile())
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "현재 타일에는 " +
                "상호작용할 것이 없습니다."
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
    // Direction Panel
    // =========================================================

    private void OpenDirectionPanel()
    {
        Debug.Log(
            "[RoomTraversalController] Space 입력 / " +
            $"state = {state} / " +
            $"isTransitioning = {isTransitioning} / " +
            $"isInteracting = {isInteracting} / " +
            $"uiManager = {(uiManager != null ? "OK" : "NULL")}"
        );

        if (isTransitioning)
            return;

        if (isInteracting)
            return;

        ResolveReferences();

        if (dungeonManager != null)
        {
            dungeonManager.RefreshAll();
        }

        if (uiManager != null)
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "DirectionSelectPanel 열기 호출"
            );

            uiManager.ShowDirectionPanel();
        }
        else
        {
            Debug.LogError(
                "[RoomTraversalController] " +
                "DungeonUIManager가 없습니다."
            );
        }

        state =
            RoomState.DirectionChoosing;
    }


    public void CloseDirectionPanel()
    {
        /*
         * 전환 중에는 닫기 금지.
         */
        if (isTransitioning)
            return;


        if (uiManager != null)
        {
            uiManager
                .HideDirectionPanel();
        }


        /*
         * 중요:
         *
         * X 버튼으로 패널을 닫을 때
         * 반드시 WaitingForInput으로 복구.
         *
         * 이 값이 복구되지 않으면
         * Space 입력을 다시 받을 수 없다.
         */
        state =
            RoomState.WaitingForInput;


        Debug.Log(
            "[RoomTraversalController] " +
            "방향 선택 취소"
        );
    }


    // =========================================================
    // Direction Select
    // =========================================================

    public void SelectNextRoom(
        MoveDirection dir)
    {
        if (state !=
            RoomState.DirectionChoosing)
        {
            return;
        }


        if (isTransitioning)
            return;


        StartCoroutine(
            ChangeRoom(
                dir
            )
        );
    }


    // =========================================================
    // Change Room
    // =========================================================

    private IEnumerator ChangeRoom(
        MoveDirection dir)
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


        // =====================================================
        // Camera 연출
        // =====================================================

        if (cameraRoomTransition != null)
        {
            yield return
                cameraRoomTransition
                    .PlayRoomMove(
                        dir
                    );
        }


        // =====================================================
        // Fade Out
        // =====================================================

        if (fadeController != null)
        {
            yield return
                fadeController
                    .FadeOut();
        }


        // =====================================================
        // 좌표 이동
        // =====================================================

        if (dungeonManager != null)
        {
            dungeonManager
                .MoveToNextRoom(
                    dir
                );
        }


        // =====================================================
        // 플레이어 중앙 복귀
        // =====================================================

        if (playerCenterPoint != null)
        {
            transform.position =
                playerCenterPoint.position;
        }


        // =====================================================
        // 카메라 위치 복귀
        // =====================================================

        if (cameraRoomTransition != null)
        {
            cameraRoomTransition
                .ResetCameraPosition();
        }


        // =====================================================
        // Fade In
        // =====================================================

        if (fadeController != null)
        {
            yield return
                fadeController
                    .FadeIn();
        }


        isTransitioning =
            false;


        // =====================================================
        // 새 방 이벤트
        // =====================================================

        yield return StartCoroutine(
            RunRoomEnterEvent()
        );
    }
}