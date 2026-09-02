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
    [SerializeField] private Transform playerCenterPoint;

    [Header("Managers")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private FadeController fadeController;
    [SerializeField] private DungeonTileEventManager tileEventManager;
    [SerializeField] private CameraRoomTransition cameraRoomTransition;

    private RoomState state;
    private bool isTransitioning;
    private bool isInteracting;

    private void Start()
    {
        ResolveReferences();

        if (playerCenterPoint != null)
            transform.position =
                playerCenterPoint.position;

        if (uiManager != null)
            uiManager.HideDirectionPanel();

        StartCoroutine(
            RoomStartRoutine()
        );
    }

    private void ResolveReferences()
    {
        if (dungeonManager == null)
            dungeonManager =
                DungeonManager.Instance;

        if (tileEventManager == null)
            tileEventManager =
                DungeonTileEventManager.Instance;
    }

    private void Update()
    {
        if (state != RoomState.WaitingForInput)
            return;

        /*
         * E = 현재 타일 상호작용
         *
         * Farming / Chest / Key / Rest /
         * Puzzle 등.
         */
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();

            return;
        }

        /*
         * 아직 이동 정보 데이터가 없기 때문에
         * Space 방향 선택 이동은 기존 방식 그대로 유지한다.
         *
         * 이동 데이터가 들어오면:
         *
         * WASD = Open 일반 이동
         * Space = Door / OneWay 특수 이동
         *
         * 으로 변경한다.
         */
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OpenDirectionPanel();
        }
    }

    // =========================================================
    // 방 시작
    // =========================================================

    private IEnumerator RoomStartRoutine()
    {
        yield return StartCoroutine(
            RunRoomEnterEvent()
        );
    }

    // =========================================================
    // 방 진입 자동 이벤트
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
                tileEventManager.ExecuteEnterEvent()
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
    // E 상호작용
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

        if (!tileEventManager.CanInteractCurrentTile())
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
        isInteracting = true;

        state =
            RoomState.EventRunning;

        yield return StartCoroutine(
            tileEventManager.ExecuteInteraction()
        );

        state =
            RoomState.WaitingForInput;

        isInteracting = false;
    }

    // =========================================================
    // 기존 방향 선택창
    // =========================================================

    private void OpenDirectionPanel()
    {
        if (dungeonManager != null)
            dungeonManager.RefreshAll();

        if (uiManager != null)
            uiManager.ShowDirectionPanel();

        state =
            RoomState.DirectionChoosing;
    }

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
            ChangeRoom(dir)
        );
    }

    public void CloseDirectionPanel()
    {
        if (uiManager != null)
            uiManager.HideDirectionPanel();

        state =
            RoomState.WaitingForInput;
    }

    // =========================================================
    // 방 이동
    // =========================================================

    private IEnumerator ChangeRoom(
        MoveDirection dir)
    {
        isTransitioning = true;

        state =
            RoomState.Transition;

        if (uiManager != null)
            uiManager.HideDirectionPanel();

        /*
         * 현재 기존 연출 유지.
         *
         * 이동 데이터가 들어오면
         * 일반 이동 / 특수 이동 모두
         * 이 전환 코루틴을 공유하게 만들 예정이다.
         */
        if (cameraRoomTransition != null)
        {
            yield return
                cameraRoomTransition
                    .PlayRoomMove(dir);
        }

        if (fadeController != null)
        {
            yield return
                fadeController.FadeOut();
        }

        if (dungeonManager != null)
        {
            dungeonManager
                .MoveToNextRoom(dir);
        }

        if (playerCenterPoint != null)
        {
            transform.position =
                playerCenterPoint.position;
        }

        if (cameraRoomTransition != null)
        {
            cameraRoomTransition
                .ResetCameraPosition();
        }

        if (fadeController != null)
        {
            yield return
                fadeController.FadeIn();
        }

        isTransitioning = false;

        /*
         * 다음 방에 도착했으므로
         * 새 타일의 자동 이벤트를 실행.
         */
        yield return StartCoroutine(
            RunRoomEnterEvent()
        );
    }
}