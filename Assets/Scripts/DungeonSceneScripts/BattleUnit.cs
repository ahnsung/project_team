using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public bool IsDead => currentHP <= 0;

    private Color originalColor;

    private void Awake()
    {
        currentHP = maxHP;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (targetArrow != null)
            targetArrow.SetActive(false);

        RefreshHPUI();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        RefreshHPUI();
    }

    public void RefreshHPUI()
    {
        if (hpBarFill != null)
            hpBarFill.fillAmount = (float)currentHP / maxHP;

        if (hpText != null)
            hpText.text = currentHP.ToString();
    }

    public void SetArrow(bool active)
    {
        if (targetArrow != null && !IsDead)
            targetArrow.SetActive(active);
    }

    public void SetHitColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;
    }

    public void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}