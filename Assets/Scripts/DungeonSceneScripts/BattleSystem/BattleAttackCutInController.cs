using System.Collections;
using UnityEngine;

public class BattleAttackCutInController : MonoBehaviour
{
    [Header("Root")]
    public GameObject effectRoot;

    [Header("Background")]
    public Transform battleBackground;
    public float backgroundZoomScale = 1.15f;
    public float playerAttackBackgroundRotateAngle = -1.8f;
    public float dogAttackBackgroundRotateAngle = 1.8f;

    [Header("Speed Lines")]
    public GameObject speedLineA;
    public GameObject speedLineB;
    public float speedLineInterval = 0.1f;

    [Header("Player Attack Objects")]
    public RectTransform playerAttackImage;
    public RectTransform playerAttackShadow;
    public RectTransform dogHitImage;
    public RectTransform dogHitShadow;
    public RectTransform swordEffect;

    [Header("Dog Attack Objects")]
    public RectTransform dogAttackImage;
    public RectTransform dogAttackShadow;
    public RectTransform playerHitImage;
    public RectTransform playerHitShadow;

    [Header("Timing")]
    public float enterTime = 0.2f;
    public float hitMoveTime = 0.2f;
    public float holdTime = 0.8f;
    public float exitTime = 0.2f;

    [Header("Player Attack Position")]
    public Vector2 playerStart = new Vector2(-1200f, -60f);
    public Vector2 playerNear = new Vector2(-350f, -60f);
    public Vector2 playerHit = new Vector2(-350f, -60f);
    public Vector2 playerEnd = new Vector2(-1200f, -60f);

    public Vector2 dogHitStart = new Vector2(1200f, -60f);
    public Vector2 dogHitNear = new Vector2(350f, -60f);
    public Vector2 dogHitPos = new Vector2(350f, -60f);
    public Vector2 dogHitEnd = new Vector2(1200f, -60f);

    [Header("Dog Attack Position")]
    public Vector2 dogStart = new Vector2(1200f, -60f);
    public Vector2 dogNear = new Vector2(350f, -60f);
    public Vector2 dogAttackPos = new Vector2(350f, -60f);
    public Vector2 dogEnd = new Vector2(1200f, -60f);

    public Vector2 playerHitStart = new Vector2(-1200f, -60f);
    public Vector2 playerHitNear = new Vector2(-350f, -60f);
    public Vector2 playerHitPos = new Vector2(-350f, -60f);
    public Vector2 playerHitEnd = new Vector2(-1200f, -60f);

    [Header("Offsets")]
    public Vector2 attackerShadowOffset = new Vector2(0f, -20f);
    public Vector2 targetShadowOffset = new Vector2(0f, -20f);
    public Vector2 swordEffectOffset = new Vector2(120f, 90f);

    [Header("Player Attack Shadow Move")]
    public Vector2 playerAttackShadowMove = new Vector2(-80f, 0f);
    public Vector2 dogHitShadowMove = new Vector2(80f, 0f);

    [Header("Dog Attack Shadow Move")]
    public Vector2 dogAttackShadowMove = new Vector2(80f, 0f);
    public Vector2 playerHitShadowMove = new Vector2(-80f, 0f);

    [Header("Original Unit Restore")]
    public float originalUnitRestoreDistance = 0.5f;
    public float originalUnitRestoreTime = 0.2f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip playerAttackSound;
    public AudioClip dogAttackSound;

    private Coroutine speedLineRoutine;
    private Vector3 originalBgScale;
    private Quaternion originalBgRotation;
    private bool isPlaying;

    private void Awake()
    {
        if (effectRoot == null)
            effectRoot = gameObject;

        effectRoot.SetActive(false);
        HideAllCutInObjects();
        HideSpeedLines();
        SaveBackgroundOriginal();
    }

    public IEnumerator PlayPlayerAttackCutIn(BattleUnit attacker, BattleUnit target)
    {
        yield return PlayCutInRoutine(
            attacker,
            target,
            playerAttackImage,
            playerAttackShadow,
            dogHitImage,
            dogHitShadow,
            swordEffect,
            playerStart,
            playerNear,
            playerHit,
            playerEnd,
            dogHitStart,
            dogHitNear,
            dogHitPos,
            dogHitEnd,
            playerAttackShadowMove,
            dogHitShadowMove,
            true,
            playerAttackSound,
            playerAttackBackgroundRotateAngle
        );
    }

    public IEnumerator PlayEnemyAttackCutIn(BattleUnit attacker, BattleUnit target)
    {
        yield return PlayCutInRoutine(
            attacker,
            target,
            dogAttackImage,
            dogAttackShadow,
            playerHitImage,
            playerHitShadow,
            null,
            dogStart,
            dogNear,
            dogAttackPos,
            dogEnd,
            playerHitStart,
            playerHitNear,
            playerHitPos,
            playerHitEnd,
            dogAttackShadowMove,
            playerHitShadowMove,
            false,
            dogAttackSound,
            dogAttackBackgroundRotateAngle
        );
    }

    private IEnumerator PlayCutInRoutine(
        BattleUnit attacker,
        BattleUnit target,
        RectTransform attackerImage,
        RectTransform attackerShadow,
        RectTransform targetImage,
        RectTransform targetShadow,
        RectTransform effectImage,
        Vector2 attackerStart,
        Vector2 attackerNear,
        Vector2 attackerHit,
        Vector2 attackerEnd,
        Vector2 targetStart,
        Vector2 targetNear,
        Vector2 targetHit,
        Vector2 targetEnd,
        Vector2 attackerShadowMove,
        Vector2 targetShadowMove,
        bool useSwordEffect,
        AudioClip attackSound,
        float rotateAngle
    )
    {
        if (isPlaying)
            yield break;

        isPlaying = true;

        SaveBackgroundOriginal();

        Vector3 attackerOriginalPos = attacker.transform.position;
        Vector3 targetOriginalPos = target.transform.position;

        effectRoot.SetActive(true);
        HideAllCutInObjects();

        SetObjectActive(attackerImage, true);
        SetObjectActive(attackerShadow, true);
        SetObjectActive(targetImage, true);
        SetObjectActive(targetShadow, true);

        if (useSwordEffect)
            SetObjectActive(effectImage, true);

        HideOriginalUnit(attacker, true);
        HideOriginalUnit(target, true);

        ApplyBackgroundEffect(rotateAngle);

        SetPairPosition(
            attackerImage,
            attackerShadow,
            targetImage,
            targetShadow,
            effectImage,
            attackerStart,
            targetStart,
            useSwordEffect
        );

        speedLineRoutine = StartCoroutine(SpeedLineLoop());

        yield return MovePair(
            attackerImage,
            attackerShadow,
            targetImage,
            targetShadow,
            effectImage,
            attackerStart,
            attackerNear,
            targetStart,
            targetNear,
            useSwordEffect,
            enterTime
        );

        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);

        yield return MovePair(
            attackerImage,
            attackerShadow,
            targetImage,
            targetShadow,
            effectImage,
            attackerNear,
            attackerHit,
            targetNear,
            targetHit,
            useSwordEffect,
            hitMoveTime
        );

        yield return HoldImpact(
            attackerImage,
            attackerShadow,
            targetImage,
            targetShadow,
            effectImage,
            attackerHit,
            targetHit,
            attackerShadowMove,
            targetShadowMove,
            useSwordEffect
        );

        yield return MovePair(
            attackerImage,
            attackerShadow,
            targetImage,
            targetShadow,
            effectImage,
            attackerHit,
            attackerEnd,
            targetHit,
            targetEnd,
            useSwordEffect,
            exitTime
        );

        StopSpeedLines();
        ClearBackgroundEffect();
        HideAllCutInObjects();
        effectRoot.SetActive(false);

        yield return RestoreOriginalUnits(
            attacker,
            target,
            attackerOriginalPos,
            targetOriginalPos,
            attackerStart,
            targetStart
        );

        isPlaying = false;
    }

    private IEnumerator MovePair(
        RectTransform attackerImage,
        RectTransform attackerShadow,
        RectTransform targetImage,
        RectTransform targetShadow,
        RectTransform effectImage,
        Vector2 attackerFrom,
        Vector2 attackerTo,
        Vector2 targetFrom,
        Vector2 targetTo,
        bool useSwordEffect,
        float duration
    )
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / duration);

            Vector2 attackerPos = Vector2.Lerp(attackerFrom, attackerTo, ratio);
            Vector2 targetPos = Vector2.Lerp(targetFrom, targetTo, ratio);

            SetPairPosition(
                attackerImage,
                attackerShadow,
                targetImage,
                targetShadow,
                effectImage,
                attackerPos,
                targetPos,
                useSwordEffect
            );

            yield return null;
        }

        SetPairPosition(
            attackerImage,
            attackerShadow,
            targetImage,
            targetShadow,
            effectImage,
            attackerTo,
            targetTo,
            useSwordEffect
        );
    }

    private IEnumerator HoldImpact(
        RectTransform attackerImage,
        RectTransform attackerShadow,
        RectTransform targetImage,
        RectTransform targetShadow,
        RectTransform effectImage,
        Vector2 attackerBase,
        Vector2 targetBase,
        Vector2 attackerShadowMove,
        Vector2 targetShadowMove,
        bool useSwordEffect
    )
    {
        float t = 0f;

        while (t < holdTime)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / holdTime);

            SetAnchored(attackerImage, attackerBase);
            SetAnchored(targetImage, targetBase);

            SetAnchored(
                attackerShadow,
                attackerBase + attackerShadowOffset + attackerShadowMove * ratio
            );

            SetAnchored(
                targetShadow,
                targetBase + targetShadowOffset + targetShadowMove * ratio
            );

            if (useSwordEffect)
                SetAnchored(effectImage, attackerBase + swordEffectOffset);

            yield return null;
        }
    }

    private void SetPairPosition(
        RectTransform attackerImage,
        RectTransform attackerShadow,
        RectTransform targetImage,
        RectTransform targetShadow,
        RectTransform effectImage,
        Vector2 attackerPos,
        Vector2 targetPos,
        bool useSwordEffect
    )
    {
        SetAnchored(attackerImage, attackerPos);
        SetAnchored(attackerShadow, attackerPos + attackerShadowOffset);

        SetAnchored(targetImage, targetPos);
        SetAnchored(targetShadow, targetPos + targetShadowOffset);

        if (useSwordEffect)
            SetAnchored(effectImage, attackerPos + swordEffectOffset);
    }

    private IEnumerator RestoreOriginalUnits(
        BattleUnit attacker,
        BattleUnit target,
        Vector3 attackerOriginalPos,
        Vector3 targetOriginalPos,
        Vector2 attackerStart,
        Vector2 targetStart
    )
    {
        Vector3 attackerRestoreDir = attackerStart.x < 0f ? Vector3.right : Vector3.left;
        Vector3 targetRestoreDir = targetStart.x < 0f ? Vector3.right : Vector3.left;

        Vector3 attackerRestoreStart =
            attackerOriginalPos + attackerRestoreDir * originalUnitRestoreDistance;

        Vector3 targetRestoreStart =
            targetOriginalPos + targetRestoreDir * originalUnitRestoreDistance;

        attacker.transform.position = attackerRestoreStart;
        target.transform.position = targetRestoreStart;

        HideOriginalUnit(attacker, false);
        HideOriginalUnit(target, false);

        float t = 0f;

        while (t < originalUnitRestoreTime)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / originalUnitRestoreTime);

            attacker.transform.position =
                Vector3.Lerp(attackerRestoreStart, attackerOriginalPos, ratio);

            target.transform.position =
                Vector3.Lerp(targetRestoreStart, targetOriginalPos, ratio);

            yield return null;
        }

        attacker.transform.position = attackerOriginalPos;
        target.transform.position = targetOriginalPos;
    }

    private void HideAllCutInObjects()
    {
        SetObjectActive(playerAttackImage, false);
        SetObjectActive(playerAttackShadow, false);
        SetObjectActive(dogHitImage, false);
        SetObjectActive(dogHitShadow, false);
        SetObjectActive(swordEffect, false);

        SetObjectActive(dogAttackImage, false);
        SetObjectActive(dogAttackShadow, false);
        SetObjectActive(playerHitImage, false);
        SetObjectActive(playerHitShadow, false);
    }

    private void SetObjectActive(RectTransform rect, bool value)
    {
        if (rect != null)
            rect.gameObject.SetActive(value);
    }

    private void SaveBackgroundOriginal()
    {
        if (battleBackground == null) return;

        originalBgScale = battleBackground.localScale;
        originalBgRotation = battleBackground.localRotation;
    }

    private void ApplyBackgroundEffect(float rotateAngle)
    {
        if (battleBackground == null) return;

        battleBackground.localScale = originalBgScale * backgroundZoomScale;
        battleBackground.localRotation = Quaternion.Euler(0f, 0f, rotateAngle);
    }

    private void ClearBackgroundEffect()
    {
        if (battleBackground == null) return;

        battleBackground.localScale = originalBgScale;
        battleBackground.localRotation = originalBgRotation;
    }

    private void SetAnchored(RectTransform rect, Vector2 pos)
    {
        if (rect == null) return;
        rect.anchoredPosition = pos;
    }

    private IEnumerator SpeedLineLoop()
    {
        while (true)
        {
            if (speedLineA != null)
                speedLineA.SetActive(true);

            if (speedLineB != null)
                speedLineB.SetActive(false);

            yield return new WaitForSeconds(speedLineInterval);

            if (speedLineA != null)
                speedLineA.SetActive(false);

            if (speedLineB != null)
                speedLineB.SetActive(true);

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
        if (speedLineA != null)
            speedLineA.SetActive(false);

        if (speedLineB != null)
            speedLineB.SetActive(false);
    }

    private void HideOriginalUnit(BattleUnit unit, bool hide)
    {
        if (unit == null) return;

        SpriteRenderer[] renderers = unit.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in renderers)
            sr.enabled = !hide;
    }
}