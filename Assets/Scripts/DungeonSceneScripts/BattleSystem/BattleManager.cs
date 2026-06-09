using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private enum BattleState
    {
        None,
        PlayerTurn,
        SelectingTarget,
        EnemyTurn,
        BattleEnd
    }

    [Header("Cut In")]
    public BattleAttackCutInController cutInController;

    [Header("Player")]
    public BattleUnit playerUnit;

    [Header("Monster Spawn")]
    public BattleMonsterData[] monsterPool;
    public Transform enemyGroup;
    public Transform[] enemySpawnPoints;
    public int minEnemyCount = 1;
    public int maxEnemyCount = 1;

    [Header("UI")]
    public BattleUIManager uiManager;
    public GameObject encounterPanel;
    public TextMeshProUGUI encounterText;

    [Header("Floating Text")]
    public TextMeshProUGUI floatingTextPrefab;
    public Canvas worldCanvas;

    [Header("Battle Timing")]
    public float encounterMessageTime = 1f;
    public float enemyAttackDelay = 0.6f;
    public float afterHitDelay = 0.2f;
    public float actionDelay = 0.6f;

    [Header("Battle Setting")]
    public int runSuccessPercent = 50;

    private BattleState state = BattleState.None;
    private bool battleRunning = false;

    private readonly List<BattleUnit> enemies = new List<BattleUnit>();

    public bool IsBattleRunning()
    {
        return battleRunning;
    }

    private void Start()
    {
        if (uiManager != null)
            uiManager.HideBattleUI();

        if (encounterPanel != null)
            encounterPanel.SetActive(false);

        if (enemyGroup != null)
            enemyGroup.gameObject.SetActive(false);
    }

    public IEnumerator StartBattleEncounter()
    {
        battleRunning = true;

        if (uiManager != null)
            uiManager.HideBattleUI();

        if (encounterPanel != null)
            encounterPanel.SetActive(true);

        if (encounterText != null)
            encounterText.text = "전투 발생!";

        yield return new WaitForSeconds(encounterMessageTime);

        if (encounterPanel != null)
            encounterPanel.SetActive(false);

        SpawnEnemies();
        StartBattle();
    }

    private void StartBattle()
    {
        state = BattleState.PlayerTurn;

        if (uiManager != null)
        {
            uiManager.ShowBattleUI();
            uiManager.ShowMainBattleMenu();
        }
    }

    private void SpawnEnemies()
    {
        ClearEnemies();

        if (enemyGroup != null)
            enemyGroup.gameObject.SetActive(true);

        if (monsterPool == null || monsterPool.Length == 0)
        {
            Debug.LogError("Monster Pool 비어있음");
            return;
        }

        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogError("Enemy Spawn Points 비어있음");
            return;
        }

        int count = Random.Range(minEnemyCount, maxEnemyCount + 1);
        count = Mathf.Clamp(count, 1, enemySpawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            BattleMonsterData data = monsterPool[Random.Range(0, monsterPool.Length)];

            if (data == null || data.monsterPrefab == null)
                continue;

            GameObject enemyObj = Instantiate(
                data.monsterPrefab,
                enemySpawnPoints[i].position,
                Quaternion.identity,
                enemyGroup
            );

            BattleUnit unit = enemyObj.GetComponent<BattleUnit>();

            if (unit == null)
                unit = enemyObj.AddComponent<BattleUnit>();

            unit.Setup(
                data.monsterName,
                data.maxHP,
                data.attackPower,
                data.accuracy,
                data.evasion
            );

            BattleEnemyClick click = enemyObj.GetComponent<BattleEnemyClick>();

            if (click == null)
                click = enemyObj.AddComponent<BattleEnemyClick>();

            click.enemyUnit = unit;
            click.battleManager = this;

            if (enemyObj.GetComponent<Collider2D>() == null)
                enemyObj.AddComponent<BoxCollider2D>();

            enemies.Add(unit);
        }
    }

    private void ClearEnemies()
    {
        enemies.Clear();

        if (enemyGroup == null)
            return;

        for (int i = enemyGroup.childCount - 1; i >= 0; i--)
        {
            Transform child = enemyGroup.GetChild(i);

            if (child.GetComponent<BattleUnit>() != null)
                Destroy(child.gameObject);
        }
    }

    public void OnClickBattleButton()
    {
        if (state != BattleState.PlayerTurn)
            return;

        if (uiManager != null)
            uiManager.ShowActionMenu();
    }

    public void OnClickAttackButton()
    {
        if (state != BattleState.PlayerTurn)
            return;

        state = BattleState.SelectingTarget;
    }

    public void OnClickRunButton()
    {
        if (state != BattleState.PlayerTurn)
            return;

        StartCoroutine(RunRoutine());
    }

    public void HoverEnemy(BattleUnit enemy)
    {
        if (state != BattleState.SelectingTarget)
            return;

        if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy)
            return;

        enemy.SetArrow(true);
    }

    public void ExitHoverEnemy(BattleUnit enemy)
    {
        if (enemy == null)
            return;

        enemy.SetArrow(false);
    }

    public void ClickEnemy(BattleUnit enemy)
    {
        if (state != BattleState.SelectingTarget)
            return;

        if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy)
            return;

        StartCoroutine(PlayerAttackRoutine(enemy));
    }

    private IEnumerator PlayerAttackRoutine(BattleUnit target)
    {
        state = BattleState.EnemyTurn;

        ClearEnemyArrows();

        int playerAccuracy = PlayerStats.Instance != null
            ? PlayerStats.Instance.GetFinalAccuracy(target.evasion)
            : playerUnit.accuracy;

        bool hit = Random.Range(0, 100) < playerAccuracy;

        if (cutInController != null)
        {
            yield return cutInController.PlayPlayerAttackCutIn(playerUnit, target);
        }
        else
        {
            playerUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(0.6f);
        }

        if (hit)
        {
            int damage = PlayerStats.Instance != null
                ? PlayerStats.Instance.GetFinalAttackDamage()
                : playerUnit.attackPower;

            target.TakeDamage(damage);
            ShowFloatingText(target.transform.position, damage.ToString());
        }
        else
        {
            ShowFloatingText(target.transform.position, "MISS");
        }

        yield return new WaitForSeconds(afterHitDelay);

        RemoveDeadEnemies();

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
                DungeonManager.Instance.AddTurn("전투 승리");

            EndBattle();
            yield break;
        }

        yield return StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        state = BattleState.EnemyTurn;

        RemoveDeadEnemies();

        BattleUnit[] aliveEnemies = enemies.ToArray();

        foreach (BattleUnit enemy in aliveEnemies)
        {
            if (enemy == null)
                continue;

            if (enemy.IsDead || !enemy.gameObject.activeInHierarchy)
                continue;

            enemy.PlayAttackAnimation();
            yield return new WaitForSeconds(enemyAttackDelay);

            bool hit = RollEnemyHit(enemy.accuracy);

            if (hit)
            {
                int damage = enemy.attackPower;

                if (PlayerResourceManager.Instance != null)
                {
                    // 배고픔 25% 이하 패널티: 모든 자원 감소량/피해량 50% 증가
                    if (PlayerResourceManager.Instance.IsHungerAllDecreasePenaltyActive())
                        damage = Mathf.RoundToInt(damage * 1.5f);
                }

                playerUnit.TakeDamage(damage);

                if (PlayerResourceManager.Instance != null)
                    PlayerResourceManager.Instance.ChangeHealth(-damage, "적 공격 피해");

                ShowFloatingText(playerUnit.transform.position, damage.ToString());
            }
            else
            {
                ShowFloatingText(playerUnit.transform.position, "MISS");
            }

            yield return new WaitForSeconds(afterHitDelay);

            if (playerUnit.IsDead)
            {
                EndBattle();
                yield break;
            }
        }

        RemoveDeadEnemies();

        if (DungeonManager.Instance != null)
            DungeonManager.Instance.AddTurn("전투 라운드 종료");

        if (uiManager != null)
            uiManager.ShowMainBattleMenu();

        state = BattleState.PlayerTurn;
    }

    private IEnumerator RunRoutine()
    {
        state = BattleState.EnemyTurn;

        int finalRunPercent = PlayerStats.Instance != null
            ? PlayerStats.Instance.GetRunSuccessPercent()
            : runSuccessPercent;

        int roll = Random.Range(0, 100);

        if (roll < finalRunPercent)
        {
            yield return new WaitForSeconds(actionDelay);

            if (DungeonManager.Instance != null)
                DungeonManager.Instance.AddTurn("도망 성공");

            EndBattle();
        }
        else
        {
            yield return new WaitForSeconds(actionDelay);
            yield return StartCoroutine(EnemyTurnRoutine());
        }
    }

    private bool RollEnemyHit(int enemyAccuracy)
    {
        if (PlayerStats.Instance == null)
            return Random.Range(0, 100) < Mathf.Clamp(enemyAccuracy, 10, 95);

        int playerEvasionChance = PlayerStats.Instance.GetFinalEvasion(enemyAccuracy);
        int enemyHitChance = 100 - playerEvasionChance;
        enemyHitChance = Mathf.Clamp(enemyHitChance, 5, 95);

        return Random.Range(0, 100) < enemyHitChance;
    }

    private void RemoveDeadEnemies()
    {
        enemies.RemoveAll(enemy =>
            enemy == null ||
            enemy.IsDead ||
            !enemy.gameObject.activeInHierarchy
        );
    }

    private bool AllEnemiesDead()
    {
        RemoveDeadEnemies();
        return enemies.Count == 0;
    }

    private void EndBattle()
    {
        if (state == BattleState.BattleEnd)
            return;

        state = BattleState.BattleEnd;
        ClearEnemyArrows();

        StartCoroutine(EndBattleRoutine());
    }

    private IEnumerator EndBattleRoutine()
    {
        yield return new WaitForSeconds(1f);

        ClearEnemies();

        if (enemyGroup != null)
            enemyGroup.gameObject.SetActive(false);

        if (uiManager != null)
            uiManager.HideBattleUI();

        state = BattleState.None;
        battleRunning = false;
    }

    private void ClearEnemyArrows()
    {
        foreach (BattleUnit enemy in enemies)
        {
            if (enemy != null)
                enemy.SetArrow(false);
        }
    }

    private void ShowFloatingText(Vector3 worldPos, string text)
    {
        if (floatingTextPrefab == null || worldCanvas == null)
            return;

        TextMeshProUGUI obj = Instantiate(floatingTextPrefab, worldCanvas.transform);
        obj.text = text;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos + Vector3.up * 1.5f);
        obj.transform.position = screenPos;

        Destroy(obj.gameObject, 0.8f);
    }
}