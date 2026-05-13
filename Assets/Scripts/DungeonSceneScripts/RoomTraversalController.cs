using System.Collections;
using UnityEngine;

public class RoomTraversalController : MonoBehaviour
{
    private enum RoomState
    {
        EventRunning,
        WaitingForSpace,
        DirectionChoosing,
        Transition
    }

    [Header("Points")]
    [SerializeField] private Transform playerCenterPoint;

    [Header("Managers")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private FadeController fadeController;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private CameraRoomTransition cameraRoomTransition;

    [Header("Event")]
    [SerializeField] private float monsterBattleChance = 100f;
    [SerializeField] private float emptyEventDuration = 1f;

    private RoomState state;
    private bool isTransitioning;

    private void Start()
    {
        if (playerCenterPoint != null)
            transform.position = playerCenterPoint.position;

        if (uiManager != null)
            uiManager.HideDirectionPanel();

        StartCoroutine(RoomStartRoutine());
    }

    private void Update()
    {
        if (state == RoomState.WaitingForSpace)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                OpenDirectionPanel();
        }
    }

    private IEnumerator RoomStartRoutine()
    {
        // 게임 시작 첫 방에서는 페이드 없이 바로 이벤트 시작
        yield return StartCoroutine(RunRoomEvent());
    }
    private IEnumerator RunRoomEvent()
    {
        state = RoomState.EventRunning;

        if (playerCenterPoint != null)
            transform.position = playerCenterPoint.position;

        bool isBattleEvent = Random.Range(0f, 100f) < monsterBattleChance;

        if (isBattleEvent && battleManager != null)
        {
            yield return StartCoroutine(battleManager.StartBattleEncounter());

            while (battleManager.IsBattleRunning())
                yield return null;
        }
        else
        {
            yield return new WaitForSeconds(emptyEventDuration);
        }

        state = RoomState.WaitingForSpace;
    }

    private void OpenDirectionPanel()
    {
        if (dungeonManager != null)
            dungeonManager.RefreshAll();

        if (uiManager != null)
            uiManager.ShowDirectionPanel();

        state = RoomState.DirectionChoosing;
    }

    public void SelectNextRoom(MoveDirection dir)
    {
        if (state != RoomState.DirectionChoosing)
            return;

        if (isTransitioning)
            return;

        StartCoroutine(ChangeRoom(dir));
    }

    public void CloseDirectionPanel()
    {
        if (uiManager != null)
            uiManager.HideDirectionPanel();

        state = RoomState.WaitingForSpace;
    }

    private IEnumerator ChangeRoom(MoveDirection dir)
    {
        isTransitioning = true;
        state = RoomState.Transition;

        if (uiManager != null)
            uiManager.HideDirectionPanel();

        // 좌/우만 카메라 이동 연출
        // 위/아래는 CameraRoomTransition 안에서 이동 없이 넘어감
        if (cameraRoomTransition != null)
            yield return cameraRoomTransition.PlayRoomMove(dir);

        if (fadeController != null)
            yield return fadeController.FadeOut();

        if (dungeonManager != null)
            dungeonManager.MoveToNextRoom(dir);

        if (playerCenterPoint != null)
            transform.position = playerCenterPoint.position;

        if (cameraRoomTransition != null)
            cameraRoomTransition.ResetCameraPosition();

        if (fadeController != null)
            yield return fadeController.FadeIn();

        isTransitioning = false;

        yield return StartCoroutine(RunRoomEvent());
    }
}