using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [Header("Info")]
    public string unitName;

    [Header("Stat")]
    public int maxHP = 30;
    public int currentHP = 30;

    public int attackPower = 10;
    public int accuracy = 90;
    public int evasion = 5;

    [Header("State")]
    public bool IsDead => currentHP <= 0;

    [Header("UI")]
    public Image hpBarFill;
    public GameObject targetArrow;

    [Header("Render")]
    public SpriteRenderer spriteRenderer;

    [Header("Animation")]
    public BattleSpriteAnimator spriteAnimator;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteAnimator == null)
            spriteAnimator = GetComponent<BattleSpriteAnimator>();

        RefreshHPUI();

        SetArrow(false);
    }

    public void Setup(
        string newName,
        int newMaxHP,
        int newAttack,
        int newAccuracy,
        int newEvasion
    )
    {
        unitName = newName;

        maxHP = newMaxHP;
        currentHP = maxHP;

        attackPower = newAttack;
        accuracy = newAccuracy;
        evasion = newEvasion;

        RefreshHPUI();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        currentHP -= damage;

        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        RefreshHPUI();

        Debug.Log(unitName + " 데미지 : " + damage);

        if (currentHP <= 0)
        {
            StartCoroutine(DeathRoutine());
        }
        else
        {
            PlayHitAnimation();
        }
    }

    private IEnumerator DeathRoutine()
    {
        PlayHitAnimation();

        yield return new WaitForSeconds(0.2f);

        PlayDeathAnimation();

        yield return new WaitForSeconds(0.25f);

        gameObject.SetActive(false);
    }

    public void RefreshHPUI()
    {
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = (float)currentHP / maxHP;
        }
    }

    public void SetArrow(bool active)
    {
        if (targetArrow != null)
            targetArrow.SetActive(active);
    }

    public void PlayAttackAnimation()
    {
        if (spriteAnimator != null)
            spriteAnimator.PlayAttack();
    }

    public void PlayHitAnimation()
    {
        if (spriteAnimator != null)
            spriteAnimator.PlayHit();
    }

    public void PlayDeathAnimation()
    {
        if (spriteAnimator != null)
            spriteAnimator.PlayDead();
    }
}