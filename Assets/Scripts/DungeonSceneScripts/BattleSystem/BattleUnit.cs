using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [Header("Info")]
    public string unitName;

    [Header("Stats")]
    public int maxHP = 10;
    public int currentHP = 10;
    public int attackPower = 2;
    public int accuracy = 90;
    public int evasion = 10;

    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    public Image hpBarFill;
    public GameObject targetArrow;

    public bool IsDead => currentHP <= 0;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (targetArrow != null)
            targetArrow.SetActive(false);
    }

    public void Setup(string name, int hp, int attack, int acc, int eva)
    {
        unitName = name;
        maxHP = hp;
        currentHP = hp;
        attackPower = attack;
        accuracy = acc;
        evasion = eva;

        UpdateHPUI();

        if (targetArrow != null)
            targetArrow.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        PlayHitAnimation();
        UpdateHPUI();

        if (currentHP <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);

        UpdateHPUI();
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
    }

    public void SetArrow(bool value)
    {
        if (targetArrow != null)
            targetArrow.SetActive(value);
    }

    private void UpdateHPUI()
    {
        if (hpBarFill == null) return;

        hpBarFill.fillAmount = (float)currentHP / maxHP;
    }

    private void Die()
    {
        if (animator != null)
            animator.SetTrigger("Die");

        gameObject.SetActive(false);
    }
}