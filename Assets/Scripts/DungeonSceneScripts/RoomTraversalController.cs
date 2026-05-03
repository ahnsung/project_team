using System.Collections;
using UnityEngine;

public class RoomTraversalController : MonoBehaviour
{
    private enum State
    {
        ChoosingPath,   // 시작 후 좌/우 선택 이동
        EventRunning,   // 이벤트 진행 중
        MovingToExit,   // 이벤트 후 같은 방향 출구로 이동
        Waiting,        // 출구 도착 후 방향 선택 대기
        Transition      // 다음 방 이동 중
    }

    private enum PathSide
    {
        None,
        Left,
        Right
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Move Clamp")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;

    [Header("Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform eventPointLeft;
    [SerializeField] private Transform eventPointRight;
    [SerializeField] private Transform exitPointLeft;
    [SerializeField] private Transform exitPointRight;

    [Header("Managers")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private FadeController fadeController;

    [Header("Event")]
    [SerializeField] private float eventDuration = 1f;

    private State state;
    private PathSide currentPath = PathSide.None;

    private bool eventDone;
    private bool exitDone;
    private bool isTransitioning;

    private void Start()
    {
        StartFlow();
    }

    private void Update()
    {
        if (state == State.ChoosingPath || state == State.MovingToExit)
            Move();

        if (state == State.ChoosingPath)
            CheckEvent();

        if (state == State.MovingToExit)
            CheckExit();
    }

    private void StartFlow()
    {
        if (startPoint != null)
            transform.position = startPoint.position;

        eventDone = false;
        exitDone = false;
        isTransitioning = false;
        currentPath = PathSide.None;

        if (uiManager != null)
            uiManager.HideDirectionPanel();

        state = State.ChoosingPath;
    }

    private void Move()
    {
        float h = 0f;

        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;

        Vector3 pos = transform.position;
        pos.x += h * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;
    }

    private void CheckEvent()
    {
        if (eventDone) return;

        if (eventPointLeft != null && Vector3.Distance(transform.position, eventPointLeft.position) < 0.2f)
        {
            eventDone = true;
            currentPath = PathSide.Left;
            StartCoroutine(EventRoutine());
            return;
        }

        if (eventPointRight != null && Vector3.Distance(transform.position, eventPointRight.position) < 0.2f)
        {
            eventDone = true;
            currentPath = PathSide.Right;
            StartCoroutine(EventRoutine());
            return;
        }
    }

    private void CheckExit()
    {
        if (exitDone) return;

        if (currentPath == PathSide.Left)
        {
            if (exitPointLeft != null && Vector3.Distance(transform.position, exitPointLeft.position) < 0.2f)
            {
                ReachExit();
            }
        }
        else if (currentPath == PathSide.Right)
        {
            if (exitPointRight != null && Vector3.Distance(transform.position, exitPointRight.position) < 0.2f)
            {
                ReachExit();
            }
        }
    }

    private void ReachExit()
    {
        exitDone = true;
        state = State.Waiting;

        if (dungeonManager != null)
            dungeonManager.RefreshAll();

        if (uiManager != null)
            uiManager.ShowDirectionPanel();
    }

    private IEnumerator EventRoutine()
    {
        state = State.EventRunning;

        Debug.Log("이벤트 발생: " + currentPath);

        yield return new WaitForSeconds(eventDuration);

        state = State.MovingToExit;
    }

    public void SelectNextRoom(MoveDirection dir)
    {
        if (state != State.Waiting) return;
        if (isTransitioning) return;

        StartCoroutine(ChangeRoom(dir));
    }

    private IEnumerator ChangeRoom(MoveDirection dir)
    {
        isTransitioning = true;
        state = State.Transition;

        if (uiManager != null)
            uiManager.HideDirectionPanel();

        if (fadeController != null)
            yield return fadeController.FadeOut();

        if (dungeonManager != null)
            dungeonManager.MoveToNextRoom(dir);

        if (startPoint != null)
            transform.position = startPoint.position;

        eventDone = false;
        exitDone = false;
        currentPath = PathSide.None;

        if (fadeController != null)
            yield return fadeController.FadeIn();

        state = State.ChoosingPath;
        isTransitioning = false;
    }
}