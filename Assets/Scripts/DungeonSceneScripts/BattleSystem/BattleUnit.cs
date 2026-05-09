using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleUnit : MonoBehaviour
{
    [Header("Basic Stat")]
    public string unitName;
    public int maxHP = 20;
    public int currentHP = 20;
    public int attackPower = 10;
    public int accuracy = 70;
    public int evasion = 30;

    [Header("UI")]
    public Image hpBarFill;
    public TextMeshProUGUI hpText;
    public GameObject targetArrow;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public bool IsDead => currentHP <= 0;

    private Color originalColor;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        currentHP = maxHP;
        RefreshHPUI();

        if (targetArrow != null)
            targetArrow.SetActive(false);
    }

    public void Setup(string newName, int hp, int atk, int acc, int eva)
    {
        unitName = newName;
        maxHP = hp;
        currentHP = hp;
        attackPower = atk;
        accuracy = acc;
        evasion = eva;

        RefreshHPUI();

        if (targetArrow != null)
            targetArrow.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        RefreshHPUI();

        PlayHitAnimation();

        if (IsDead)
            PlayDeathAnimation();
    }

    public void RefreshHPUI()
    {
        if (hpBarFill != null)
            hpBarFill.fillAmount = (float)currentHP / maxHP;

        if (hpText != null)
            hpText.text = currentHP + " / " + maxHP;
    }

    public void SetArrow(bool active)
    {
        if (targetArrow != null && !IsDead)
            targetArrow.SetActive(active);
    }

    public void PlayAttackAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    public void PlayHitAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Hit");

        StartCoroutine(HitColorRoutine());
    }

    public void PlayDeathAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Die");
    }

    private IEnumerator HitColorRoutine()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }
}