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

    [Header("Guard Status")]
    [Range(1, 100)]
    public int guardDamageReductionPercent = 50;

    [Min(1)]
    public int guardArmorDurabilityMultiplier = 2;

    private const int GuardStatusId = 9001;
    private const int GuardStunStatusId = 9002;

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

            StatusEffectController enemyStatusController =
                enemyObject.GetComponent<StatusEffectController>();

            if (enemyStatusController == null)
            {
                enemyStatusController =
                    enemyObject.AddComponent<StatusEffectController>();
            }

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

    public void OnClickGuardButton()
    {
        if (!battleRunning ||
            state != BattleState.PlayerTurn)
        {
            return;
        }

        StatusEffectController playerStatusController =
            GetPlayerStatusController();

        if (playerStatusController == null)
        {
            Debug.LogError(
                "[BattleManager] 플레이어 StatusEffectController가 없습니다."
            );

            return;
        }

        playerStatusController.AddStatusEffect(
            CreateGuardStatusData()
        );

        StartCoroutine(
            PlayerGuardRoutine()
        );
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

    private IEnumerator PlayerGuardRoutine()
    {
        state = BattleState.EnemyTurn;

        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.HideBattleUI();

        Debug.Log(
            "[BattleManager] 방어 사용 - 이번 적 팀 공격에 방어 효과 적용"
        );

        yield return StartCoroutine(
            EnemyTurnRoutine(true)
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

        // ==========================================
        // 플레이어 상태이상 Controller
        // ==========================================

        StatusEffectController playerStatus =
            playerUnit != null
                ? playerUnit.GetComponent<StatusEffectController>()
                : null;

        // ==========================================
        // 명중률 계산
        // ==========================================

        int playerAccuracy =
            PlayerStats.Instance != null
                ? PlayerStats.Instance
                    .GetFinalAccuracy(target.evasion)
                : playerUnit != null
                    ? playerUnit.accuracy
                    : 90;

        // 상태이상 명중률 보정
        if (playerStatus != null)
        {
            playerAccuracy +=
                playerStatus.GetAccuracyBonus();
        }

        // 확률 안전 처리
        playerAccuracy =
            Mathf.Clamp(
                playerAccuracy,
                0,
                100
            );

        bool hit =
            Random.Range(0, 100) <
            playerAccuracy;

        Debug.Log(
            "[Battle] 플레이어 최종 명중률: " +
            playerAccuracy
        );

        // ==========================================
        // 공격 연출
        // ==========================================

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

        // ==========================================
        // 공격 적중
        // ==========================================

        if (hit)
        {
            int baseDamage =
                PlayerStats.Instance != null
                    ? PlayerStats.Instance
                        .GetFinalAttackDamage()
                    : playerUnit != null
                        ? Mathf.Max(
                            1,
                            playerUnit.attackPower
                        )
                        : 1;

            // ======================================
            // 공격력 상태이상
            // ======================================

            float attackMultiplier = 1f;

            if (playerStatus != null)
            {
                attackMultiplier =
                    playerStatus
                        .GetAttackPowerMultiplier();
            }

            int damage =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        baseDamage *
                        attackMultiplier
                    )
                );

            // ======================================
            // 적이 받는 피해 상태이상
            // ======================================

            StatusEffectController targetStatus =
                target != null
                    ? target.GetComponent<
                        StatusEffectController>()
                    : null;

            if (targetStatus != null)
            {
                damage =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(
                            damage *
                            targetStatus
                                .GetDamageTakenMultiplier()
                        )
                    );
            }

            Debug.Log(
                "[Battle] 공격 데미지 계산: " +
                baseDamage +
                " x " +
                attackMultiplier +
                " = " +
                damage
            );

            target.TakeDamage(
                damage
            );

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

        // ==========================================
        // 무기 내구도
        // ==========================================

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

        StatusEffectController playerStatusController =
            GetPlayerStatusController();

        BattleUnit[] aliveEnemies =
            enemies.ToArray();

        foreach (
            BattleUnit enemy
            in aliveEnemies)
        {
            if (!IsValidLivingEnemy(enemy))
                continue;

            StatusEffectController enemyStatusController =
                GetOrAddStatusController(enemy);

            // ==========================================
            // 적 턴 시작 상태이상 처리
            // ==========================================

            if (enemyStatusController != null)
            {
                enemyStatusController.ProcessTiming(
                    StatusEffectTiming.SelfTurnStart
                );
            }

            bool enemyIsStunned =
                enemyStatusController != null &&
                enemyStatusController.HasStatusEffect(
                    StatusEffectType.Stun
                );

            if (enemyIsStunned)
            {
                Debug.Log(
                    "[BattleManager] " +
                    enemy.unitName +
                    "은(는) 기절하여 행동할 수 없습니다."
                );

                ShowFloatingText(
                    enemy.transform.position,
                    "STUN"
                );

                yield return new WaitForSeconds(
                    actionDelay
                );

                if (enemyStatusController != null)
                {
                    enemyStatusController.ProcessTiming(
                        StatusEffectTiming.SelfTurnEnd
                    );
                }

                continue;
            }

            // ==========================================
            // 적 공격 연출
            // ==========================================

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

            // ==========================================
            // 적 명중률 + 플레이어 회피 상태이상
            // ==========================================

            int finalEnemyAccuracy =
                enemy.accuracy;

            if (playerStatusController != null)
            {
                int evasionBonus =
                    playerStatusController
                        .GetEvasionBonus();

                /*
                 * 플레이어 EvasionUp
                 * → 적 명중률 감소
                 *
                 * 플레이어 EvasionDown
                 * → 적 명중률 증가
                 */

                finalEnemyAccuracy -=
                    evasionBonus;
            }

            finalEnemyAccuracy =
                Mathf.Clamp(
                    finalEnemyAccuracy,
                    0,
                    100
                );

            Debug.Log(
                "[BattleManager] 적 최종 명중률: " +
                finalEnemyAccuracy
            );

            bool hit =
                RollEnemyHit(
                    finalEnemyAccuracy
                );

            // ==========================================
            // Guard 확인
            // ==========================================

            bool guardWasActive =
                playerStatusController != null &&
                playerStatusController.HasStatusEffect(
                    StatusEffectType.Guard
                );

            bool applyGuardStunAfterTurn =
                false;

            // ==========================================
            // 적중
            // ==========================================

            if (hit)
            {
                // --------------------------------------
                // 적 기본 공격력
                // --------------------------------------

                int damage =
                    Mathf.Max(
                        1,
                        enemy.attackPower
                    );

                // --------------------------------------
                // 적 공격력 상태이상
                // --------------------------------------

                if (enemyStatusController != null)
                {
                    float attackMultiplier =
                        enemyStatusController
                            .GetAttackPowerMultiplier();

                    damage =
                        Mathf.Max(
                            1,
                            Mathf.RoundToInt(
                                damage *
                                attackMultiplier
                            )
                        );

                    Debug.Log(
                        "[BattleManager] 적 공격력 상태이상: " +
                        enemy.attackPower +
                        " x " +
                        attackMultiplier +
                        " = " +
                        damage
                    );
                }

                // --------------------------------------
                // 배고픔 25% 이하 패널티
                // --------------------------------------

                if (PlayerResourceManager.Instance != null &&
                    PlayerResourceManager.Instance
                        .IsHungerAllDecreasePenaltyActive())
                {
                    damage =
                        Mathf.RoundToInt(
                            damage * 1.5f
                        );

                    Debug.Log(
                        "[BattleManager] " +
                        "배고픔 패널티 적용 → 피해 1.5배"
                    );
                }

                // --------------------------------------
                // 플레이어 방어력 상태이상
                // --------------------------------------

                if (playerStatusController != null)
                {
                    float defenseMultiplier =
                        playerStatusController
                            .GetDefenseMultiplier();

                    /*
                     * DefenseUp
                     * → 방어 배율 증가
                     * → 받는 피해 감소
                     *
                     * DefenseDown
                     * → 방어 배율 감소
                     * → 받는 피해 증가
                     */

                    if (defenseMultiplier > 0f)
                    {
                        damage =
                            Mathf.Max(
                                1,
                                Mathf.RoundToInt(
                                    damage /
                                    defenseMultiplier
                                )
                            );
                    }

                    Debug.Log(
                        "[BattleManager] " +
                        "플레이어 방어 상태이상 배율: " +
                        defenseMultiplier
                    );
                }

                // --------------------------------------
                // 받는 피해량 상태이상
                // --------------------------------------

                if (playerStatusController != null)
                {
                    float damageTakenMultiplier =
                        playerStatusController
                            .GetDamageTakenMultiplier();

                    damage =
                        Mathf.Max(
                            1,
                            Mathf.RoundToInt(
                                damage *
                                damageTakenMultiplier
                            )
                        );

                    Debug.Log(
                        "[BattleManager] " +
                        "받는 피해 상태이상 배율: " +
                        damageTakenMultiplier
                    );
                }

                // --------------------------------------
                // Guard
                // --------------------------------------

                if (guardWasActive)
                {
                    float guardMultiplier =
                        Mathf.Clamp01(
                            1f -
                            guardDamageReductionPercent /
                            100f
                        );

                    damage =
                        Mathf.Max(
                            1,
                            Mathf.CeilToInt(
                                damage *
                                guardMultiplier
                            )
                        );

                    applyGuardStunAfterTurn =
                        true;

                    Debug.Log(
                        "[BattleManager] 방어 적용 - " +
                        "받는 피해 감소 / " +
                        "방어구 내구도 소모 " +
                        guardArmorDurabilityMultiplier +
                        "배"
                    );
                }

                // --------------------------------------
                // 최종 피해 적용
                // --------------------------------------

                if (playerUnit != null)
                {
                    playerUnit.TakeDamage(
                        damage
                    );
                }

                if (PlayerResourceManager.Instance != null)
                {
                    PlayerResourceManager.Instance
                        .ChangeHealth(
                            -damage,
                            guardWasActive
                                ? "적 공격 피해 (방어 적용)"
                                : "적 공격 피해"
                        );
                }

                if (playerUnit != null)
                {
                    ShowFloatingText(
                        playerUnit.transform.position,
                        damage.ToString()
                    );
                }

                // --------------------------------------
                // 방어구 내구도
                // --------------------------------------

                ConsumePlayerArmorDurability(
                    guardWasActive
                );
            }
            else
            {
                // ======================================
                // MISS
                // ======================================

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

            // ==========================================
            // 적 턴 종료
            // ==========================================

            if (enemyStatusController != null)
            {
                enemyStatusController.ProcessTiming(
                    StatusEffectTiming.SelfTurnEnd
                );
            }

            // ==========================================
            // Guard 반격 기절
            // ==========================================

            if (applyGuardStunAfterTurn &&
                enemyStatusController != null)
            {
                enemyStatusController.AddStatusEffect(
                    CreateGuardStunStatusData()
                );
            }

            // ==========================================
            // 플레이어 사망
            // ==========================================

            if (playerUnit != null &&
                playerUnit.IsDead)
            {
                EndBattle();
                yield break;
            }
        }

        // ==========================================
        // 적 팀 종료 상태이상
        // ==========================================

        RemoveDeadEnemies();

        if (playerStatusController != null)
        {
            playerStatusController.ProcessTiming(
                StatusEffectTiming.EnemyTeamEnd
            );
        }

        foreach (
            BattleUnit enemy
            in enemies)
        {
            StatusEffectController enemyStatusController =
                GetStatusController(enemy);

            if (enemyStatusController != null)
            {
                enemyStatusController.ProcessTiming(
                    StatusEffectTiming.EnemyTeamEnd
                );
            }
        }

        // ==========================================
        // 전체 턴 종료
        // ==========================================

        ProcessBattleTurnEndStatusEffects();

        RemoveDeadEnemies();

        if (playerUnit != null &&
            playerUnit.IsDead)
        {
            EndBattle();
            yield break;
        }

        // ==========================================
        // 전투 승리
        // ==========================================

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn(
                        "전투 승리"
                    );
            }

            EndBattle();
            yield break;
        }

        // ==========================================
        // 던전 턴 증가
        // ==========================================

        if (addTurnAtEnd &&
            DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .AddTurn(
                    "전투 라운드 종료"
                );
        }

        ReturnToPlayerTurnIfPossible();
    }


    private void ProcessBattleTurnEndStatusEffects()
    {
        StatusEffectController playerStatusController =
            GetPlayerStatusController();

        if (playerStatusController != null)
        {
            playerStatusController.ProcessTiming(
                StatusEffectTiming.TurnEnd
            );
        }

        BattleUnit[] snapshot = enemies.ToArray();

        foreach (BattleUnit enemy in snapshot)
        {
            if (enemy == null)
                continue;

            StatusEffectController controller =
                GetStatusController(enemy);

            if (controller != null)
            {
                controller.ProcessTiming(
                    StatusEffectTiming.TurnEnd
                );
            }
        }
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

    private void ConsumePlayerArmorDurability(
        bool guardActive)
    {
        if (EquipmentManager.Instance == null)
            return;

        int finalCost =
            armorDurabilityCost;

        if (guardActive)
        {
            finalCost *=
                Mathf.Max(
                    1,
                    guardArmorDurabilityMultiplier
                );
        }

        EquipmentManager.Instance
            .ConsumeArmorDurability(
                finalCost
            );
    }

    private StatusEffectController GetPlayerStatusController()
    {
        if (playerUnit == null)
            return null;

        StatusEffectController controller =
            playerUnit.GetComponent<StatusEffectController>();

        if (controller == null)
        {
            controller =
                playerUnit.GetComponentInParent<StatusEffectController>();
        }

        if (controller == null)
        {
            controller =
                playerUnit.GetComponentInChildren<StatusEffectController>();
        }

        return controller;
    }

    private StatusEffectController GetStatusController(
        BattleUnit unit)
    {
        if (unit == null)
            return null;

        return unit.GetComponent<StatusEffectController>();
    }

    private StatusEffectController GetOrAddStatusController(
        BattleUnit unit)
    {
        if (unit == null)
            return null;

        StatusEffectController controller =
            unit.GetComponent<StatusEffectController>();

        if (controller == null)
        {
            controller =
                unit.gameObject.AddComponent<StatusEffectController>();
        }

        return controller;
    }

    private StatusEffectData CreateGuardStatusData()
    {
        return new StatusEffectData
        {
            id = GuardStatusId,
            buffName = "방어",
            description =
                "이번 적 팀의 공격 피해를 50% 감소시키고, " +
                "피격 시 방어구 내구도 소모가 2배가 됩니다. " +
                "방어 중 공격한 적은 다음 행동 1회를 기절합니다.",
            tendency = StatusEffectTendency.Positive,
            effectType = StatusEffectType.Guard,
            effectPower = guardDamageReductionPercent,
            buffDuration = 1,
            whenDecreaseDuration = StatusEffectTiming.EnemyTeamEnd,
            whenBuffEffect = StatusEffectTiming.None,
            canStack = false,
            whenRemove = StatusEffectRemoveType.DurationEnded
        };
    }

    private StatusEffectData CreateGuardStunStatusData()
    {
        return new StatusEffectData
        {
            id = GuardStunStatusId,
            buffName = "기절",
            description =
                "다음 자신의 행동 1회를 수행할 수 없습니다.",
            tendency = StatusEffectTendency.Negative,
            effectType = StatusEffectType.Stun,
            effectPower = 0,
            buffDuration = 1,
            whenDecreaseDuration = StatusEffectTiming.SelfTurnEnd,
            whenBuffEffect = StatusEffectTiming.None,
            canStack = true,
            whenRemove = StatusEffectRemoveType.DurationEnded
        };
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