using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleAttackCutInController : MonoBehaviour
{
    [Header("Root")]
    public GameObject effectRoot;

    [Header("Background")]
    public Transform battleBackground;
    public float backgroundZoomScale = 1.15f;
    public float backgroundRotateAngle = 1.8f;

    [Header("Speed Lines")]
    public GameObject speedLineA;
    public GameObject speedLineB;
    public float speedLineInterval = 0.1f;

    [Header("Cut In Images")]
    public Image cutInAttacker;
    public Image cutInTarget;
    public Image attackerShadow;
    public Image targetShadow;

    [Header("Timing")]
    public float enterTime = 0.2f;
    public float hitMoveTime = 0.2f;
    public float holdTime = 0.8f;
    public float exitTime = 0.2f;
    public float restoreTime = 0.2f;

    [Header("Player Attack Positions")]
    public Vector2 attackerStart = new Vector2(-1200f, -60f);
    public Vector2 attackerNear = new Vector2(-420f, -60f);
    public Vector2 attackerHit = new Vector2(-180f, -60f);
    public Vector2 attackerEnd = new Vector2(-1200f, -60f);

    public Vector2 targetStart = new Vector2(1200f, -60f);
    public Vector2 targetNear = new Vector2(420f, -60f);
    public Vector2 targetHit = new Vector2(180f, -60f);
    public Vector2 targetEnd = new Vector2(1200f, -60f);

    [Header("Shadow")]
    public Vector2 attackerShadowOffset = new Vector2(0f, -20f);
    public Vector2 targetShadowOffset = new Vector2(0f, -20f);
    public Vector2 impactShadowMove = new Vector2(-80f, 0f);

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip attackSound;

    private Coroutine speedLineRoutine;
    private Vector3 originalBgScale;
    private Quaternion originalBgRotation;
    private bool isPlaying;

    private void Awake()
    {
        if (effectRoot == null)
            effectRoot = gameObject;

        effectRoot.SetActive(false);
        HideSpeedLines();
        SetCutInActive(false);

        SaveBackgroundOriginal();
    }

    public IEnumerator PlayPlayerAttackCutIn(BattleUnit attacker, BattleUnit target)
    {
        if (isPlaying)
            yield break;

        isPlaying = true;

        SaveBackgroundOriginal();

        effectRoot.SetActive(true);

        HideOriginalUnit(attacker, true);
        HideOriginalUnit(target, true);

        ApplyBackgroundEffect();

        SetCutInActive(true);
        SetPairPosition(attackerStart, targetStart);

        speedLineRoutine = StartCoroutine(SpeedLineLoop());

        yield return MovePair(attackerStart, attackerNear, targetStart, targetNear, enterTime);

        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);

        yield return MovePair(attackerNear, attackerHit, targetNear, targetHit, hitMoveTime);

        yield return HoldImpact();

        yield return MovePair(attackerHit, attackerEnd, targetHit, targetEnd, exitTime);

        HideOriginalUnit(attacker, false);
        HideOriginalUnit(target, false);

        StopSpeedLines();
        ClearBackgroundEffect();

        yield return new WaitForSeconds(restoreTime);

        SetCutInActive(false);
        effectRoot.SetActive(false);

        isPlaying = false;
    }

    private void SaveBackgroundOriginal()
    {
        if (battleBackground == null) return;

        originalBgScale = battleBackground.localScale;
        originalBgRotation = battleBackground.localRotation;
    }

    private void ApplyBackgroundEffect()
    {
        if (battleBackground == null) return;

        battleBackground.localScale = originalBgScale * backgroundZoomScale;
        battleBackground.localRotation = Quaternion.Euler(0f, 0f, backgroundRotateAngle);
    }

    private void ClearBackgroundEffect()
    {
        if (battleBackground == null) return;

        battleBackground.localScale = originalBgScale;
        battleBackground.localRotation = originalBgRotation;
    }

    private IEnumerator MovePair(Vector2 attackerFrom, Vector2 attackerTo, Vector2 targetFrom, Vector2 targetTo, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / duration);

            Vector2 attackerPos = Vector2.Lerp(attackerFrom, attackerTo, ratio);
            Vector2 targetPos = Vector2.Lerp(targetFrom, targetTo, ratio);

            SetPairPosition(attackerPos, targetPos);

            yield return null;
        }

        SetPairPosition(attackerTo, targetTo);
    }

    private IEnumerator HoldImpact()
    {
        float t = 0f;

        Vector2 attackerBase = attackerHit;
        Vector2 targetBase = targetHit;

        while (t < holdTime)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / holdTime);

            SetAnchored(cutInAttacker, attackerBase);
            SetAnchored(cutInTarget, targetBase);

            SetAnchored(attackerShadow, attackerBase + attackerShadowOffset + impactShadowMove * ratio);
            SetAnchored(targetShadow, targetBase + targetShadowOffset + impactShadowMove * ratio);

            yield return null;
        }
    }

    private void SetPairPosition(Vector2 attackerPos, Vector2 targetPos)
    {
        SetAnchored(cutInAttacker, attackerPos);
        SetAnchored(attackerShadow, attackerPos + attackerShadowOffset);

        SetAnchored(cutInTarget, targetPos);
        SetAnchored(targetShadow, targetPos + targetShadowOffset);
    }

    private void SetAnchored(Image image, Vector2 pos)
    {
        if (image == null) return;

        RectTransform rt = image.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
    }

    private void SetCutInActive(bool value)
    {
        if (cutInAttacker != null) cutInAttacker.gameObject.SetActive(value);
        if (cutInTarget != null) cutInTarget.gameObject.SetActive(value);
        if (attackerShadow != null) attackerShadow.gameObject.SetActive(value);
        if (targetShadow != null) targetShadow.gameObject.SetActive(value);
    }

    private IEnumerator SpeedLineLoop()
    {
        while (true)
        {
            if (speedLineA != null) speedLineA.SetActive(true);
            if (speedLineB != null) speedLineB.SetActive(false);

            yield return new WaitForSeconds(speedLineInterval);

            if (speedLineA != null) speedLineA.SetActive(false);
            if (speedLineB != null) speedLineB.SetActive(true);

            yield return new WaitForSeconds(speedLineInterval);
        }
    }

    private void StopSpeedLines()
    {
        if (speedLineRoutine != null)
            StopCoroutine(speedLineRoutine);

        speedLineRoutine = null;
        HideSpeedLines();
    }

    private void HideSpeedLines()
    {
        if (speedLineA != null) speedLineA.SetActive(false);
        if (speedLineB != null) speedLineB.SetActive(false);
    }

    private void HideOriginalUnit(BattleUnit unit, bool hide)
    {
        if (unit == null) return;

        SpriteRenderer[] renderers = unit.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in renderers)
            sr.enabled = !hide;
    }
}