using System.Collections;
using UnityEngine;

public class RoomTraversalController : MonoBehaviour
{
    private enum State
    {
        Exploring,      // 방 안에서 자유롭게 A/D 이동 가능
        EventRunning,   // 이벤트 진행 중
        Waiting,        // 출구 도착 후 방향 선택 UI 대기
        Transition      // 방 이동 중
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Move Clamp")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;

    [Header("Points")]
    [SerializeField] private Transform startPoint;

    [Header("Event Points")]
    [SerializeField] private Transform leftEventPoint;
    [SerializeField] private Transform rightEventPoint;

    [Header("Exit Points")]
    [SerializeField] private Transform leftExitPoint;
    [SerializeField] private Transform rightExitPoint;

    [Header("Managers")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private FadeController fadeController;

    [Header("Battle")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private float monsterBattleChance = 50f;

    [Header("Dummy Event")]
    [SerializeField] private float eventDuration = 1f;

    private State state;

    private bool eventDone;
    private bool exitDone;
    private bool isTransitioning;

    private void Start()
    {
        StartRoom();
    }

    private void Update()
    {
        if (state != State.Exploring)
            return;

        Move();
        CheckEvent();
        CheckExit();
    }

    private void StartRoom()
    {
        transform.position = startPoint.position;

        eventDone = false;
        exitDone = false;
        isTransitioning = false;

        if (uiManager != null)
            uiManager.HideDirectionPanel();

        state = State.Exploring;
    }

    private void Move()
    {
        float h = 0f;

        if (Input.GetKey(KeyCode.A))
            h = -1f;

        if (Input.GetKey(KeyCode.D))
            h = 1f;

        Vector3 pos = transform.position;
        pos.x += h * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;
    }

    private void CheckEvent()
    {
        if (eventDone) return;

        bool touchedLeftEvent =
            leftEventPoint != null &&
            Mathf.Abs(transform.position.x - leftEventPoint.position.x) < 0.4f;

        bool touchedRightEvent =
            rightEventPoint != null &&
            Mathf.Abs(transform.position.x - rightEventPoint.position.x) < 0.4f;

        if (touchedLeftEvent || touchedRightEvent)
        {
            Debug.Log("이벤트 지점 도착");

            eventDone = true;
            StartCoroutine(EventRoutine());
        }
    }

    private IEnumerator EventRoutine()
    {
        state = State.EventRunning;

        Debug.Log("EventRoutine 실행됨");

        bool monsterEvent = Random.Range(0f, 100f) < monsterBattleChance;

        if (monsterEvent && battleManager != null)
        {
            Debug.Log("전투 이벤트 실행");

            yield return StartCoroutine(battleManager.StartBattleEncounter());

            // 전투가 완전히 끝날 때까지 대기
            while (battleManager.IsBattleRunning())
            {
                yield return null;
            }
        }
        else
        {
            Debug.Log("전투 아님 / BattleManager 없음");
            yield return new WaitForSeconds(eventDuration);
        }

        state = State.Exploring;
    }

    private void CheckExit()
    {
        if (exitDone)
            return;

        bool touchedLeftExit =
            leftExitPoint != null &&
            Vector3.Distance(transform.position, leftExitPoint.position) < 0.25f;

        bool touchedRightExit =
            rightExitPoint != null &&
            Vector3.Distance(transform.position, rightExitPoint.position) < 0.25f;

        if (touchedLeftExit || touchedRightExit)
        {
            exitDone = true;
            state = State.Waiting;

            if (dungeonManager != null)
                dungeonManager.RefreshAll();

            if (uiManager != null)
                uiManager.ShowDirectionPanel();
        }
    }

    public void SelectNextRoom(MoveDirection dir)
    {
        if (state != State.Waiting)
            return;

        if (isTransitioning)
            return;

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

        transform.position = startPoint.position;

        eventDone = false;
        exitDone = false;

        if (fadeController != null)
            yield return fadeController.FadeIn();

        state = State.Exploring;
        isTransitioning = false;
    }
}