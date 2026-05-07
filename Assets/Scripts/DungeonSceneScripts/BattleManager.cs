using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [Header("Player")]
    public BattleUnit playerUnit;

    [Header("Enemies")]
    public List<BattleUnit> enemies = new List<BattleUnit>();

    [Header("UI")]
    public BattleUIManager uiManager;

    [Header("Floating Text")]
    public TextMeshProUGUI floatingTextPrefab;
    public Canvas worldCanvas;

    [Header("Battle Setting")]
    public int battleTurn = 1;
    public float actionDelay = 0.6f;
    public int runSuccessPercent = 50;

    [Header("Objects To Hide During Battle")]
    public GameObject directionPanel;
    public GameObject inventoryPanel;

    private BattleState state = BattleState.None;
    private bool battleRunning = false;

    public System.Action OnBattleFinished;

    private void Start()
    {
        if (uiManager != null)
            uiManager.HideBattleUI();

        foreach (var enemy in enemies)
        {
            if (enemy != null)
                enemy.gameObject.SetActive(false);
        }
    }

    public void StartBattle()
    {
        battleRunning = true;
        battleTurn = 1;
        state = BattleState.PlayerTurn;

        if (uiManager != null)
        {
            uiManager.ShowBattleUI();
            uiManager.SetTurnText(battleTurn);
            uiManager.SetMessage("Battle Start!");
        }

        if (directionPanel != null)
            directionPanel.SetActive(false);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(true);
                enemy.currentHP = enemy.maxHP;
                enemy.RefreshHPUI();
                enemy.SetArrow(false);
                enemy.ResetColor();
            }
        }
    }

    public bool IsBattleRunning()
    {
        return battleRunning;
    }

    public void OnClickBattleButton()
    {
        if (state != BattleState.PlayerTurn) return;

        if (uiManager != null)
            uiManager.ShowActionMenu();
    }

    public void OnClickAttackButton()
    {
        if (state != BattleState.PlayerTurn) return;

        state = BattleState.SelectingTarget;

        if (uiManager != null)
            uiManager.SetMessage("Select enemy.");
    }

    public void HoverEnemy(BattleUnit enemy)
    {
        if (state != BattleState.SelectingTarget) return;
        if (enemy == null || enemy.IsDead) return;

        enemy.SetArrow(true);
    }

    public void ExitHoverEnemy(BattleUnit enemy)
    {
        if (enemy == null) return;
        enemy.SetArrow(false);
    }

    public void ClickEnemy(BattleUnit enemy)
    {
        if (state != BattleState.SelectingTarget) return;
        if (enemy == null || enemy.IsDead) return;

        StartCoroutine(PlayerAttackRoutine(enemy));
    }

    private IEnumerator PlayerAttackRoutine(BattleUnit target)
    {
        state = BattleState.EnemyTurn;

        ClearEnemyArrows();

        bool hit = RollHit(playerUnit.accuracy, target.evasion);

        if (hit)
        {
            target.TakeDamage(playerUnit.attackPower);
            target.SetHitColor();
            ShowFloatingText(target.transform.position, playerUnit.attackPower.ToString());

            if (uiManager != null)
                uiManager.SetMessage("Player hit!");
        }
        else
        {
            ShowFloatingText(target.transform.position, "MISS");

            if (uiManager != null)
                uiManager.SetMessage("Player missed!");
        }

        yield return new WaitForSeconds(actionDelay);

        target.ResetColor();

        if (AllEnemiesDead())
        {
            EndBattle("Win!");
            yield break;
        }

        yield return StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        state = BattleState.EnemyTurn;

        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            yield return new WaitForSeconds(actionDelay);

            bool hit = RollHit(enemy.accuracy, playerUnit.evasion);

            if (hit)
            {
                playerUnit.TakeDamage(enemy.attackPower);
                playerUnit.SetHitColor();
                ShowFloatingText(playerUnit.transform.position, enemy.attackPower.ToString());

                if (uiManager != null)
                    uiManager.SetMessage(enemy.unitName + " hit!");
            }
            else
            {
                ShowFloatingText(playerUnit.transform.position, "MISS");

                if (uiManager != null)
                    uiManager.SetMessage(enemy.unitName + " missed!");
            }

            yield return new WaitForSeconds(actionDelay);

            playerUnit.ResetColor();

            if (playerUnit.IsDead)
            {
                EndBattle("Player Dead...");
                yield break;
            }
        }

        battleTurn++;

        if (uiManager != null)
        {
            uiManager.SetTurnText(battleTurn);
            uiManager.ShowMainBattleMenu();
            uiManager.SetMessage("Player Turn.");
        }

        state = BattleState.PlayerTurn;
    }

    public void OnClickRunButton()
    {
        if (state != BattleState.PlayerTurn) return;

        StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        state = BattleState.EnemyTurn;

        int roll = Random.Range(0, 100);

        if (roll < runSuccessPercent)
        {
            if (uiManager != null)
                uiManager.SetMessage("Run success!");

            yield return new WaitForSeconds(actionDelay);

            EndBattle("Run Success");
        }
        else
        {
            if (uiManager != null)
                uiManager.SetMessage("Run failed!");

            yield return new WaitForSeconds(actionDelay);

            yield return StartCoroutine(EnemyTurnRoutine());
        }
    }

    private bool RollHit(int attackerAccuracy, int targetEvasion)
    {
        int finalHitChance = attackerAccuracy - targetEvasion;
        finalHitChance = Mathf.Clamp(finalHitChance, 10, 95);

        int roll = Random.Range(0, 100);
        return roll < finalHitChance;
    }

    private bool AllEnemiesDead()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
                return false;
        }

        return true;
    }

    private void EndBattle(string message)
    {
        battleRunning = false;
        state = BattleState.BattleEnd;

        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.SetMessage(message);

        StartCoroutine(EndBattleRoutine());
    }

    private IEnumerator EndBattleRoutine()
    {
        yield return new WaitForSeconds(1f);

        foreach (var enemy in enemies)
        {
            if (enemy != null)
                enemy.gameObject.SetActive(false);
        }

        if (uiManager != null)
            uiManager.HideBattleUI();

        state = BattleState.None;

        OnBattleFinished?.Invoke();
    }

    private void ClearEnemyArrows()
    {
        foreach (var enemy in enemies)
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