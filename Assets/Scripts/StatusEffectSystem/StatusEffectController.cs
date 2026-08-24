using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    [Header("Runtime Status Effects")]
    [SerializeField]
    private List<ActiveStatusEffect> activeEffects =
        new List<ActiveStatusEffect>();

    public event Action OnStatusEffectsChanged;

    public IReadOnlyList<ActiveStatusEffect> ActiveEffects
        => activeEffects;

    private void Awake()
    {
        if (activeEffects == null)
        {
            activeEffects =
                new List<ActiveStatusEffect>();
        }

        RemoveInvalidEffects();
    }

    // =========================================================
    // 상태이상 추가
    // =========================================================

    public bool AddStatusEffect(
        StatusEffectData data)
    {
        if (data == null)
        {
            Debug.LogWarning(
                "[StatusEffectController] 추가하려는 " +
                "StatusEffectData가 null입니다."
            );

            return false;
        }

        if (data.effectType ==
            StatusEffectType.None)
        {
            Debug.LogWarning(
                "[StatusEffectController] EffectType이 None인 " +
                "상태이상은 추가하지 않습니다."
            );

            return false;
        }

        ActiveStatusEffect sameEffect =
            FindSameEffect(data);

        /*
         * canStack == true
         * 같은 ID의 상태이상이 이미 있으면 지속시간을 합산한다.
         */
        if (sameEffect != null &&
            data.canStack)
        {
            sameEffect.AddStack(
                data.buffDuration
            );

            Debug.Log(
                "[StatusEffectController] 상태이상 중첩: " +
                data.buffName +
                " / 남은 지속시간: " +
                sameEffect.RemainingDuration
            );

            NotifyChanged();

            return true;
        }

        ActiveStatusEffect newEffect =
            new ActiveStatusEffect(
                data
            );

        activeEffects.Add(
            newEffect
        );

        Debug.Log(
            "[StatusEffectController] 상태이상 추가: " +
            data.buffName +
            " / 지속시간: " +
            (
                newEffect.IsInfinite
                    ? "무한"
                    : newEffect.RemainingDuration.ToString()
            )
        );

        NotifyChanged();

        return true;
    }

    // =========================================================
    // 상태이상 조회
    // =========================================================

    public bool HasStatusEffect(
        StatusEffectType type)
    {
        return GetStatusEffect(type) != null;
    }

    public ActiveStatusEffect GetStatusEffect(
        StatusEffectType type)
    {
        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (effect == null ||
                effect.Data == null)
            {
                continue;
            }

            if (effect.Data.effectType ==
                type)
            {
                return effect;
            }
        }

        return null;
    }

    public List<ActiveStatusEffect>
        GetStatusEffects(
            StatusEffectType type)
    {
        List<ActiveStatusEffect> result =
            new List<ActiveStatusEffect>();

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (effect == null ||
                effect.Data == null)
            {
                continue;
            }

            if (effect.Data.effectType ==
                type)
            {
                result.Add(
                    effect
                );
            }
        }

        return result;
    }

    public bool HasStatusEffectById(
        int id)
    {
        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (effect == null ||
                effect.Data == null)
            {
                continue;
            }

            if (effect.Data.id == id)
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // 전투 수치 보정 API
    // =========================================================

    public float GetAttackPowerMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            int power =
                effect.Data.effectPower;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.AttackPowerUp:

                    multiplier +=
                        power / 100f;

                    break;

                case StatusEffectType.AttackPowerDown:

                    multiplier -=
                        power / 100f;

                    break;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }

    public float GetDefenseMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            int power =
                effect.Data.effectPower;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.DefenseUp:

                    multiplier +=
                        power / 100f;

                    break;

                case StatusEffectType.DefenseDown:

                    multiplier -=
                        power / 100f;

                    break;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }

    public int GetAccuracyBonus()
    {
        int result = 0;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.AccuracyUp:

                    result +=
                        effect.Data.effectPower;

                    break;

                case StatusEffectType.AccuracyDown:

                    result -=
                        effect.Data.effectPower;

                    break;
            }
        }

        return result;
    }

    public int GetEvasionBonus()
    {
        int result = 0;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.EvasionUp:

                    result +=
                        effect.Data.effectPower;

                    break;

                case StatusEffectType.EvasionDown:

                    result -=
                        effect.Data.effectPower;

                    break;
            }
        }

        return result;
    }

    public float GetDamageTakenMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            int power =
                effect.Data.effectPower;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.DamageTakenUp:

                    multiplier +=
                        power / 100f;

                    break;

                case StatusEffectType.DamageTakenDown:

                    multiplier -=
                        power / 100f;

                    break;
            }
        }

        /*
         * 기존 Guard는 별도 타입이지만,
         * 받는 피해 보정 API에서도 같이 반영 가능하게 한다.
         * 현재 BattleManager에서 Guard를 직접 50% 처리하고 있으므로
         * BattleManager를 수정하기 전까지는 중복 적용하면 안 된다.
         *
         * 따라서 Guard는 여기서는 적용하지 않는다.
         */

        return Mathf.Max(
            0f,
            multiplier
        );
    }

    public float GetHealingMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            int power =
                effect.Data.effectPower;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.HealingUp:

                    multiplier +=
                        power / 100f;

                    break;

                case StatusEffectType.HealingDown:

                    multiplier -=
                        power / 100f;

                    break;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }

    public int GetStrengthBonus()
    {
        return GetStatBonus(
            StatusEffectType.StrengthUp,
            StatusEffectType.StrengthDown
        );
    }

    public int GetDexterityBonus()
    {
        return GetStatBonus(
            StatusEffectType.DexterityUp,
            StatusEffectType.DexterityDown
        );
    }

    public int GetConstitutionBonus()
    {
        return GetStatBonus(
            StatusEffectType.ConstitutionUp,
            StatusEffectType.ConstitutionDown
        );
    }

    public int GetIntelligenceBonus()
    {
        return GetStatBonus(
            StatusEffectType.IntelligenceUp,
            StatusEffectType.IntelligenceDown
        );
    }

    private int GetStatBonus(
        StatusEffectType upType,
        StatusEffectType downType)
    {
        int result = 0;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.effectType ==
                upType)
            {
                result +=
                    effect.Data.effectPower;
            }
            else if (
                effect.Data.effectType ==
                downType)
            {
                result -=
                    effect.Data.effectPower;
            }
        }

        return result;
    }

    public bool CanAct()
    {
        return !HasStatusEffect(
            StatusEffectType.Stun
        );
    }

    public bool CanAttack()
    {
        return CanAct();
    }

    // =========================================================
    // 특정 전투 타이밍 처리
    // =========================================================

    public void ProcessTiming(
        StatusEffectTiming timing)
    {
        if (activeEffects == null ||
            activeEffects.Count == 0)
        {
            return;
        }

        List<ActiveStatusEffect> snapshot =
            new List<ActiveStatusEffect>(
                activeEffects
            );

        foreach (
            ActiveStatusEffect effect
            in snapshot)
        {
            if (effect == null ||
                effect.Data == null)
            {
                continue;
            }

            /*
             * 기획서 규칙:
             * 효과 발동과 지속시간 감소가 같은 타이밍이면
             * 효과 발동을 먼저 처리한다.
             */
            if (effect.Data.whenBuffEffect ==
                timing)
            {
                TriggerStatusEffect(
                    effect,
                    timing
                );
            }

            /*
             * 효과 발동으로 이미 제거됐을 수 있다.
             */
            if (!activeEffects.Contains(
                    effect))
            {
                continue;
            }

            if (effect.Data.whenDecreaseDuration ==
                timing)
            {
                DecreaseDuration(
                    effect
                );
            }
        }

        RemoveExpiredEffects();
        RemoveBySpecialConditions();

        NotifyChanged();
    }

    // =========================================================
    // 상태이상 효과 발동
    // =========================================================

    private void TriggerStatusEffect(
        ActiveStatusEffect effect,
        StatusEffectTiming timing)
    {
        if (effect == null ||
            effect.Data == null)
        {
            return;
        }

        StatusEffectData data =
            effect.Data;

        switch (data.effectType)
        {
            case StatusEffectType.Stun:

                // 실제 행동 차단은 BattleManager에서 처리.
                break;

            case StatusEffectType.Poison:

                ApplyPoisonDamage(
                    data.effectPower
                );

                break;

            case StatusEffectType.StatIncrease:

                /*
                 * 기존 테스트용 타입.
                 * 앞으로 세부 스탯 타입
                 * StrengthUp / DexterityUp 등을 사용한다.
                 */
                break;

            case StatusEffectType.Guard:

                /*
                 * 현재 Guard는 BattleManager에서
                 * 피해 50%, 방어구 내구도 2배를 처리.
                 */
                break;

            case StatusEffectType.AttackPowerUp:
            case StatusEffectType.AttackPowerDown:
            case StatusEffectType.DefenseUp:
            case StatusEffectType.DefenseDown:
            case StatusEffectType.AccuracyUp:
            case StatusEffectType.AccuracyDown:
            case StatusEffectType.EvasionUp:
            case StatusEffectType.EvasionDown:
            case StatusEffectType.DamageTakenUp:
            case StatusEffectType.DamageTakenDown:
            case StatusEffectType.HealingUp:
            case StatusEffectType.HealingDown:
            case StatusEffectType.StrengthUp:
            case StatusEffectType.StrengthDown:
            case StatusEffectType.DexterityUp:
            case StatusEffectType.DexterityDown:
            case StatusEffectType.ConstitutionUp:
            case StatusEffectType.ConstitutionDown:
            case StatusEffectType.IntelligenceUp:
            case StatusEffectType.IntelligenceDown:

                /*
                 * 이 타입들은 ProcessTiming에서 별도의 즉발 효과를
                 * 발동하는 것이 아니라,
                 * 지속 중 전투 계산 시 GetXXX() API로 조회한다.
                 */
                break;
        }

        if (data.whenRemove ==
            StatusEffectRemoveType.EffectTriggered)
        {
            RemoveStatusEffect(
                effect
            );
        }
    }

    // =========================================================
    // 중독
    // =========================================================

    private void ApplyPoisonDamage(
        int amount)
    {
        int damage =
            Mathf.Max(
                0,
                amount
            );

        if (damage <= 0)
            return;

        BattleUnit unit =
            GetComponent<BattleUnit>();

        bool isPlayer =
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null &&
            unit ==
            BattleManager.Instance.playerUnit;

        if (isPlayer)
        {
            if (unit != null &&
                !unit.IsDead)
            {
                unit.TakeDamage(
                    damage
                );
            }

            if (PlayerResourceManager.Instance != null)
            {
                PlayerResourceManager.Instance
                    .ChangeHealth(
                        -damage,
                        "중독 피해"
                    );
            }

            Debug.Log(
                "[StatusEffectController] 플레이어 중독 피해: " +
                damage
            );

            return;
        }

        if (unit != null &&
            !unit.IsDead)
        {
            unit.TakeDamage(
                damage
            );

            Debug.Log(
                "[StatusEffectController] " +
                unit.unitName +
                " 중독 피해: " +
                damage
            );
        }
    }

    // =========================================================
    // 지속시간
    // =========================================================

    private void DecreaseDuration(
        ActiveStatusEffect effect)
    {
        if (effect == null ||
            effect.Data == null ||
            effect.IsInfinite)
        {
            return;
        }

        int before =
            effect.RemainingDuration;

        effect.DecreaseDuration();

        Debug.Log(
            "[StatusEffectController] 지속시간 감소: " +
            effect.Data.buffName +
            " / " +
            before +
            " -> " +
            effect.RemainingDuration
        );
    }

    // =========================================================
    // 제거
    // =========================================================

    public bool RemoveStatusEffect(
        ActiveStatusEffect effect)
    {
        if (effect == null)
            return false;

        bool removed =
            activeEffects.Remove(
                effect
            );

        if (!removed)
            return false;

        if (effect.Data != null)
        {
            Debug.Log(
                "[StatusEffectController] 상태이상 제거: " +
                effect.Data.buffName
            );
        }

        NotifyChanged();

        return true;
    }

    public int RemoveStatusEffect(
        StatusEffectType type)
    {
        int removedCount =
            activeEffects.RemoveAll(
                effect =>
                    effect != null &&
                    effect.Data != null &&
                    effect.Data.effectType ==
                    type
            );

        if (removedCount > 0)
        {
            Debug.Log(
                "[StatusEffectController] 상태이상 제거: " +
                type +
                " / " +
                removedCount +
                "개"
            );

            NotifyChanged();
        }

        return removedCount;
    }

    public void ClearAllStatusEffects()
    {
        if (activeEffects.Count == 0)
            return;

        activeEffects.Clear();

        Debug.Log(
            "[StatusEffectController] 모든 상태이상을 제거했습니다."
        );

        NotifyChanged();
    }

    private void RemoveExpiredEffects()
    {
        int removedCount =
            activeEffects.RemoveAll(
                effect =>
                    effect == null ||
                    effect.Data == null ||
                    (
                        effect.Data.whenRemove ==
                        StatusEffectRemoveType.DurationEnded &&
                        effect.IsExpired
                    )
            );

        if (removedCount > 0)
        {
            Debug.Log(
                "[StatusEffectController] 지속시간이 끝난 상태이상 " +
                removedCount +
                "개 제거"
            );
        }
    }

    private void RemoveBySpecialConditions()
    {
        BattleUnit unit =
            GetComponent<BattleUnit>();

        bool isPlayer =
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null &&
            unit ==
            BattleManager.Instance.playerUnit;

        if (!isPlayer ||
            PlayerResourceManager.Instance == null)
        {
            return;
        }

        float hungerRatio =
            0f;

        float mentalRatio =
            0f;

        if (PlayerResourceManager.Instance.MaxHunger > 0)
        {
            hungerRatio =
                (float)
                PlayerResourceManager.Instance.CurrentHunger /
                PlayerResourceManager.Instance.MaxHunger;
        }

        if (PlayerResourceManager.Instance.MaxMental > 0)
        {
            mentalRatio =
                (float)
                PlayerResourceManager.Instance.CurrentMental /
                PlayerResourceManager.Instance.MaxMental;
        }

        activeEffects.RemoveAll(
            effect =>
            {
                if (effect == null ||
                    effect.Data == null)
                {
                    return true;
                }

                switch (
                    effect.Data.whenRemove)
                {
                    case StatusEffectRemoveType.HungerAbove70:

                        return hungerRatio > 0.70f;

                    case StatusEffectRemoveType.HungerAbove50:

                        return hungerRatio > 0.50f;

                    case StatusEffectRemoveType.HungerAbove25:

                        return hungerRatio > 0.25f;

                    case StatusEffectRemoveType.MentalAbove75:

                        return mentalRatio > 0.75f;

                    case StatusEffectRemoveType.MentalAbove50:

                        return mentalRatio > 0.50f;

                    case StatusEffectRemoveType.MentalAbove25:

                        return mentalRatio > 0.25f;
                }

                return false;
            }
        );
    }

    // =========================================================
    // 내부 검색 / 검증
    // =========================================================

    private ActiveStatusEffect FindSameEffect(
        StatusEffectData data)
    {
        if (data == null)
            return null;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (effect == null ||
                effect.Data == null)
            {
                continue;
            }

            /*
             * 문서상 같은 이름의 상태이상이라도
             * ID가 다르면 별개의 데이터가 될 수 있으므로
             * ID 기준으로 판단한다.
             */
            if (effect.Data.id ==
                data.id)
            {
                return effect;
            }
        }

        return null;
    }

    private bool IsValidEffect(
        ActiveStatusEffect effect)
    {
        return
            effect != null &&
            effect.Data != null &&
            !effect.IsExpired;
    }

    private void RemoveInvalidEffects()
    {
        if (activeEffects == null)
        {
            activeEffects =
                new List<ActiveStatusEffect>();

            return;
        }

        activeEffects.RemoveAll(
            effect =>
                effect == null ||
                effect.Data == null
        );
    }

    private void NotifyChanged()
    {
        OnStatusEffectsChanged?.Invoke();
    }

    // =========================================================
    // DEBUG TEST
    // =========================================================

    [ContextMenu("DEBUG - 중독 3턴 / 10 피해 부여")]
    private void DebugAddPoison()
    {
        StatusEffectData poison =
            new StatusEffectData
            {
                id = 9101,

                buffName =
                    "중독",

                description =
                    "턴 종료마다 10의 피해를 받습니다.",

                tendency =
                    StatusEffectTendency.Negative,

                effectType =
                    StatusEffectType.Poison,

                effectPower =
                    10,

                buffDuration =
                    3,

                whenDecreaseDuration =
                    StatusEffectTiming.TurnEnd,

                whenBuffEffect =
                    StatusEffectTiming.TurnEnd,

                canStack =
                    true,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            };

        AddStatusEffect(
            poison
        );
    }

    [ContextMenu("DEBUG - 공격력 +25% / 3턴 부여")]
    private void DebugAddAttackPowerUp()
    {
        StatusEffectData buff =
            new StatusEffectData
            {
                id = 9201,

                buffName =
                    "공격력 증가",

                description =
                    "공격력이 25% 증가합니다.",

                tendency =
                    StatusEffectTendency.Positive,

                effectType =
                    StatusEffectType.AttackPowerUp,

                effectPower =
                    25,

                buffDuration =
                    3,

                whenDecreaseDuration =
                    StatusEffectTiming.TurnEnd,

                whenBuffEffect =
                    StatusEffectTiming.None,

                canStack =
                    false,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            };

        AddStatusEffect(
            buff
        );

        Debug.Log(
            "[StatusEffectController] 현재 공격력 배율: " +
            GetAttackPowerMultiplier()
        );
    }

    [ContextMenu("DEBUG - 명중률 -15 / 2턴 부여")]
    private void DebugAddAccuracyDown()
    {
        StatusEffectData debuff =
            new StatusEffectData
            {
                id = 9202,

                buffName =
                    "명중률 감소",

                description =
                    "명중률이 15 감소합니다.",

                tendency =
                    StatusEffectTendency.Negative,

                effectType =
                    StatusEffectType.AccuracyDown,

                effectPower =
                    15,

                buffDuration =
                    2,

                whenDecreaseDuration =
                    StatusEffectTiming.TurnEnd,

                whenBuffEffect =
                    StatusEffectTiming.None,

                canStack =
                    false,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            };

        AddStatusEffect(
            debuff
        );

        Debug.Log(
            "[StatusEffectController] 현재 명중 보정: " +
            GetAccuracyBonus()
        );
    }
}