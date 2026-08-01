using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

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

    [Min(1)]
    public int minEnemyCount = 1;

    [Min(1)]
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

    [Header("Equipment Durability")]
    [Min(0)]
    public int weaponDurabilityCost = 10;

    [Min(0)]
    public int armorDurabilityCost = 10;

    private BattleState state =
        BattleState.None;

    private bool battleRunning;

    private readonly List<BattleUnit> enemies =
        new List<BattleUnit>();

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool IsBattleRunning()
    {
        return battleRunning;
    }

    public bool CanPlayerUseItem()
    {
        return battleRunning &&
               state == BattleState.PlayerTurn;
    }

    public bool CanPlayerSwapWeapon()
    {
        return battleRunning &&
               state == BattleState.PlayerTurn;
    }

    public IEnumerator StartBattleEncounter()
    {
        if (battleRunning)
            yield break;

        battleRunning = true;
        state = BattleState.None;

        if (uiManager != null)
            uiManager.HideBattleUI();

        if (encounterPanel != null)
            encounterPanel.SetActive(true);

        if (encounterText != null)
            encounterText.text = "전투 발생!";

        yield return new WaitForSeconds(
            encounterMessageTime
        );

        if (encounterPanel != null)
            encounterPanel.SetActive(false);

        SpawnEnemies();

        if (enemies.Count == 0)
        {
            Debug.LogError(
                "[BattleManager] 생성된 몬스터가 없습니다."
            );

            EndBattleImmediately();
            yield break;
        }

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

        if (monsterPool == null ||
            monsterPool.Length == 0)
        {
            Debug.LogError(
                "[BattleManager] Monster Pool이 비어 있습니다."
            );

            return;
        }

        if (enemySpawnPoints == null ||
            enemySpawnPoints.Length == 0)
        {
            Debug.LogError(
                "[BattleManager] Enemy Spawn Points가 비어 있습니다."
            );

            return;
        }

        int safeMinimum =
            Mathf.Max(1, minEnemyCount);

        int safeMaximum =
            Mathf.Max(safeMinimum, maxEnemyCount);

        int count =
            Random.Range(
                safeMinimum,
                safeMaximum + 1
            );

        count =
            Mathf.Clamp(
                count,
                1,
                enemySpawnPoints.Length
            );

        for (int i = 0; i < count; i++)
        {
            BattleMonsterData data =
                GetRandomValidMonsterData();

            if (data == null ||
                data.monsterPrefab == null)
            {
                Debug.LogWarning(
                    "[BattleManager] 사용할 수 있는 몬스터 데이터가 없습니다."
                );

                continue;
            }

            Transform spawnPoint =
                enemySpawnPoints[i];

            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    "[BattleManager] Enemy Spawn Point가 비어 있습니다."
                );

                continue;
            }

            GameObject enemyObject =
                Instantiate(
                    data.monsterPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    enemyGroup
                );

            BattleUnit unit =
                enemyObject.GetComponent<BattleUnit>();

            if (unit == null)
                unit = enemyObject.AddComponent<BattleUnit>();

            unit.Setup(
                data.monsterName,
                data.maxHP,
                data.attackPower,
                data.accuracy,
                data.evasion
            );

            BattleEnemyClick enemyClick =
                enemyObject.GetComponent<BattleEnemyClick>();

            if (enemyClick == null)
            {
                enemyClick =
                    enemyObject.AddComponent<BattleEnemyClick>();
            }

            enemyClick.enemyUnit = unit;
            enemyClick.battleManager = this;

            Collider2D collider =
                enemyObject.GetComponent<Collider2D>();

            if (collider == null)
            {
                BoxCollider2D boxCollider =
                    enemyObject.AddComponent<BoxCollider2D>();

                boxCollider.isTrigger = true;
            }

            enemies.Add(unit);
        }
    }

    private BattleMonsterData GetRandomValidMonsterData()
    {
        if (monsterPool == null ||
            monsterPool.Length == 0)
        {
            return null;
        }

        List<BattleMonsterData> validData =
            new List<BattleMonsterData>();

        foreach (
            BattleMonsterData data
            in monsterPool)
        {
            if (data != null &&
                data.monsterPrefab != null)
            {
                validData.Add(data);
            }
        }

        if (validData.Count == 0)
            return null;

        return validData[
            Random.Range(0, validData.Count)
        ];
    }

    private void ClearEnemies()
    {
        enemies.Clear();

        if (enemyGroup == null)
            return;

        for (
            int i = enemyGroup.childCount - 1;
            i >= 0;
            i--)
        {
            Transform child =
                enemyGroup.GetChild(i);

            if (child != null)
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

        StartCoroutine(
            RunRoutine()
        );
    }

    public void HoverEnemy(
        BattleUnit enemy)
    {
        if (state != BattleState.SelectingTarget)
            return;

        if (!IsValidLivingEnemy(enemy))
            return;

        enemy.SetArrow(true);
    }

    public void ExitHoverEnemy(
        BattleUnit enemy)
    {
        if (enemy == null)
            return;

        enemy.SetArrow(false);
    }

    public void ClickEnemy(
        BattleUnit enemy)
    {
        if (state != BattleState.SelectingTarget)
            return;

        if (!IsValidLivingEnemy(enemy))
            return;

        StartCoroutine(
            PlayerAttackRoutine(enemy)
        );
    }

    public void OnPlayerUsedItem()
    {
        if (!battleRunning)
            return;

        if (state != BattleState.PlayerTurn)
            return;

        StartCoroutine(
            PlayerUseItemRoutine()
        );
    }

    private IEnumerator PlayerUseItemRoutine()
    {
        state = BattleState.EnemyTurn;

        if (uiManager != null)
            uiManager.HideBattleUI();

        yield return StartCoroutine(
            EnemyTurnRoutine(false)
        );

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .AddTurn("아이템 사용");
        }

        ReturnToPlayerTurnIfPossible();
    }

    private IEnumerator PlayerAttackRoutine(
        BattleUnit target)
    {
        state = BattleState.EnemyTurn;

        ClearEnemyArrows();

        int playerAccuracy =
            PlayerStats.Instance != null
                ? PlayerStats.Instance
                    .GetFinalAccuracy(target.evasion)
                : playerUnit != null
                    ? playerUnit.accuracy
                    : 90;

        bool hit =
            Random.Range(0, 100) <
            playerAccuracy;

        if (cutInController != null &&
            playerUnit != null)
        {
            yield return cutInController
                .PlayPlayerAttackCutIn(
                    playerUnit,
                    target
                );
        }
        else
        {
            if (playerUnit != null)
                playerUnit.PlayAttackAnimation();

            yield return new WaitForSeconds(
                0.6f
            );
        }

        if (hit)
        {
            int damage =
                PlayerStats.Instance != null
                    ? PlayerStats.Instance
                        .GetFinalAttackDamage()
                    : playerUnit != null
                        ? Mathf.Max(
                            1,
                            playerUnit.attackPower
                        )
                        : 1;

            target.TakeDamage(damage);

            ShowFloatingText(
                target.transform.position,
                damage.ToString()
            );
        }
        else
        {
            ShowFloatingText(
                target.transform.position,
                "MISS"
            );
        }

        ConsumePlayerWeaponDurability();

        yield return new WaitForSeconds(
            afterHitDelay
        );

        RemoveDeadEnemies();

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn("전투 승리");
            }

            EndBattle();
            yield break;
        }

        yield return StartCoroutine(
            EnemyTurnRoutine(true)
        );
    }

    private IEnumerator EnemyTurnRoutine(
        bool addTurnAtEnd)
    {
        state = BattleState.EnemyTurn;

        RemoveDeadEnemies();

        BattleUnit[] aliveEnemies =
            enemies.ToArray();

        foreach (
            BattleUnit enemy
            in aliveEnemies)
        {
            if (!IsValidLivingEnemy(enemy))
                continue;

            if (cutInController != null &&
                playerUnit != null)
            {
                yield return cutInController
                    .PlayEnemyAttackCutIn(
                        enemy,
                        playerUnit
                    );
            }
            else
            {
                enemy.PlayAttackAnimation();

                yield return new WaitForSeconds(
                    enemyAttackDelay
                );
            }

            bool hit =
                RollEnemyHit(enemy.accuracy);

            if (hit)
            {
                int damage =
                    Mathf.Max(
                        1,
                        enemy.attackPower
                    );

                if (PlayerResourceManager.Instance != null &&
                    PlayerResourceManager.Instance
                        .IsHungerAllDecreasePenaltyActive())
                {
                    damage =
                        Mathf.RoundToInt(
                            damage * 1.5f
                        );
                }

                if (playerUnit != null)
                    playerUnit.TakeDamage(damage);

                if (PlayerResourceManager.Instance != null)
                {
                    PlayerResourceManager.Instance
                        .ChangeHealth(
                            -damage,
                            "적 공격 피해"
                        );
                }

                if (playerUnit != null)
                {
                    ShowFloatingText(
                        playerUnit.transform.position,
                        damage.ToString()
                    );
                }

                ConsumePlayerArmorDurability();
            }
            else
            {
                if (playerUnit != null)
                {
                    ShowFloatingText(
                        playerUnit.transform.position,
                        "MISS"
                    );
                }
            }

            yield return new WaitForSeconds(
                afterHitDelay
            );

            if (playerUnit != null &&
                playerUnit.IsDead)
            {
                EndBattle();
                yield break;
            }
        }

        RemoveDeadEnemies();

        if (addTurnAtEnd &&
            DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .AddTurn("전투 라운드 종료");
        }

        ReturnToPlayerTurnIfPossible();
    }

    private void ConsumePlayerWeaponDurability()
    {
        if (EquipmentManager.Instance == null)
            return;

        EquipmentManager.Instance
            .ConsumeMainWeaponDurability(
                weaponDurabilityCost
            );
    }

    private void ConsumePlayerArmorDurability()
    {
        if (EquipmentManager.Instance == null)
            return;

        EquipmentManager.Instance
            .ConsumeArmorDurability(
                armorDurabilityCost
            );
    }

    private void ReturnToPlayerTurnIfPossible()
    {
        if (!battleRunning ||
            state == BattleState.BattleEnd)
        {
            return;
        }

        state = BattleState.PlayerTurn;

        if (uiManager != null)
        {
            uiManager.ShowBattleUI();
            uiManager.ShowMainBattleMenu();
        }
    }

    private IEnumerator RunRoutine()
    {
        state = BattleState.EnemyTurn;

        int finalRunPercent =
            PlayerStats.Instance != null
                ? PlayerStats.Instance
                    .GetRunSuccessPercent()
                : runSuccessPercent;

        int roll =
            Random.Range(0, 100);

        if (roll < finalRunPercent)
        {
            yield return new WaitForSeconds(
                actionDelay
            );

            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn("도망 성공");
            }

            EndBattle();
        }
        else
        {
            yield return new WaitForSeconds(
                actionDelay
            );

            yield return StartCoroutine(
                EnemyTurnRoutine(true)
            );
        }
    }

    private bool RollEnemyHit(
        int enemyAccuracy)
    {
        if (PlayerStats.Instance == null)
        {
            return Random.Range(0, 100) <
                   Mathf.Clamp(
                       enemyAccuracy,
                       10,
                       95
                   );
        }

        int playerEvasionChance =
            PlayerStats.Instance
                .GetFinalEvasion(
                    enemyAccuracy
                );

        int enemyHitChance =
            100 - playerEvasionChance;

        enemyHitChance =
            Mathf.Clamp(
                enemyHitChance,
                5,
                95
            );

        return Random.Range(0, 100) <
               enemyHitChance;
    }

    private bool IsValidLivingEnemy(
        BattleUnit enemy)
    {
        return enemy != null &&
               !enemy.IsDead &&
               enemy.gameObject.activeInHierarchy;
    }

    private void RemoveDeadEnemies()
    {
        enemies.RemoveAll(
            enemy =>
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

        StartCoroutine(
            EndBattleRoutine()
        );
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

    private void EndBattleImmediately()
    {
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
        foreach (
            BattleUnit enemy
            in enemies)
        {
            if (enemy != null)
                enemy.SetArrow(false);
        }
    }

    private void ShowFloatingText(
        Vector3 worldPosition,
        string text)
    {
        if (floatingTextPrefab == null ||
            worldCanvas == null)
        {
            return;
        }

        TextMeshProUGUI floatingText =
            Instantiate(
                floatingTextPrefab,
                worldCanvas.transform
            );

        floatingText.text = text;

        Camera mainCamera =
            Camera.main;

        if (mainCamera != null)
        {
            Vector3 screenPosition =
                mainCamera.WorldToScreenPoint(
                    worldPosition +
                    Vector3.up * 1.5f
                );

            floatingText.transform.position =
                screenPosition;
        }

        Destroy(
            floatingText.gameObject,
            0.8f
        );
    }
}