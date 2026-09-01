using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusEffectUIManager : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private StatusEffectController targetController;

    [Header("Icon")]
    [SerializeField]
    private Transform iconContainer;

    [SerializeField]
    private GameObject iconPrefab;

    [Header("Basic Status Icons")]
    [SerializeField]
    private Sprite poisonIcon;

    [SerializeField]
    private Sprite stunIcon;

    [Header("Attack Icons")]
    [SerializeField]
    private Sprite attackPowerUpIcon;

    [SerializeField]
    private Sprite attackPowerDownIcon;

    [Header("Defense Icons")]
    [SerializeField]
    private Sprite defenseUpIcon;

    [SerializeField]
    private Sprite defenseDownIcon;

    [Header("Accuracy Icons")]
    [SerializeField]
    private Sprite accuracyUpIcon;

    [SerializeField]
    private Sprite accuracyDownIcon;

    [Header("Evasion Icons")]
    [SerializeField]
    private Sprite evasionUpIcon;

    [SerializeField]
    private Sprite evasionDownIcon;

    [Header("Damage Taken Icons")]
    [SerializeField]
    private Sprite damageTakenUpIcon;

    [SerializeField]
    private Sprite damageTakenDownIcon;

    [Header("Healing Icons")]
    [SerializeField]
    private Sprite healingUpIcon;

    [SerializeField]
    private Sprite healingDownIcon;

    [Header("Stat Icons")]
    [SerializeField]
    private Sprite strengthUpIcon;

    [SerializeField]
    private Sprite strengthDownIcon;

    [SerializeField]
    private Sprite dexterityUpIcon;

    [SerializeField]
    private Sprite dexterityDownIcon;

    [SerializeField]
    private Sprite constitutionUpIcon;

    [SerializeField]
    private Sprite constitutionDownIcon;

    [SerializeField]
    private Sprite intelligenceUpIcon;

    [SerializeField]
    private Sprite intelligenceDownIcon;

    [Header("Tooltip")]
    [SerializeField]
    private GameObject tooltipRoot;

    [SerializeField]
    private TextMeshProUGUI tooltipNameText;

    [SerializeField]
    private TextMeshProUGUI tooltipDescriptionText;

    [SerializeField]
    private TextMeshProUGUI tooltipDurationText;

    private readonly List<StatusEffectIconUI> iconUIs =
        new List<StatusEffectIconUI>();

    private void Start()
    {
        ResolveTarget();

        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }

        if (targetController != null)
        {
            targetController.OnStatusEffectsChanged +=
                RefreshUI;
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (targetController != null)
        {
            targetController.OnStatusEffectsChanged -=
                RefreshUI;
        }
    }

    private void ResolveTarget()
    {
        if (targetController != null)
            return;

        StatusEffectController[] controllers =
            FindObjectsByType<StatusEffectController>(
                FindObjectsSortMode.None
            );

        foreach (
            StatusEffectController controller
            in controllers)
        {
            if (controller == null)
                continue;

            BattleUnit unit =
                controller.GetComponent<BattleUnit>();

            if (BattleManager.Instance != null &&
                BattleManager.Instance.playerUnit != null &&
                unit == BattleManager.Instance.playerUnit)
            {
                targetController = controller;
                return;
            }
        }
    }

    public void RefreshUI()
    {
        ClearIcons();

        if (targetController == null ||
            iconContainer == null ||
            iconPrefab == null)
        {
            return;
        }

        foreach (
            ActiveStatusEffect effect
            in targetController.ActiveEffects)
        {
            if (effect == null ||
                effect.Data == null)
            {
                continue;
            }

            /*
             * Guard는 전투 행동으로 사용되는 일시 상태이므로
             * 플레이어 상태이상 아이콘 목록에는 표시하지 않는다.
             */
            if (effect.Data.effectType ==
                StatusEffectType.Guard)
            {
                continue;
            }

            Sprite sprite =
                GetIcon(effect);

            /*
             * 아이콘이 없는 데이터는
             * DurationText만 덩그러니 생성하지 않는다.
             */
            if (sprite == null)
            {
                Debug.LogWarning(
                    "[StatusEffectUIManager] 아이콘이 없어 " +
                    "UI 표시를 건너뜁니다: " +
                    effect.Data.buffName +
                    " / " +
                    effect.Data.effectType
                );

                continue;
            }

            GameObject iconObject =
                Instantiate(
                    iconPrefab,
                    iconContainer
                );

            StatusEffectIconUI iconUI =
                iconObject.GetComponent<
                    StatusEffectIconUI
                >();

            if (iconUI == null)
            {
                Debug.LogError(
                    "[StatusEffectUIManager] " +
                    "StatusEffectIconPrefab에 " +
                    "StatusEffectIconUI가 없습니다."
                );

                Destroy(iconObject);
                continue;
            }

            iconUI.Setup(
                effect,
                this,
                sprite
            );

            iconUIs.Add(
                iconUI
            );
        }
    }

    private Sprite GetIcon(
        ActiveStatusEffect effect)
    {
        if (effect == null ||
            effect.Data == null)
        {
            return null;
        }

        /*
         * 나중에 기획 데이터에서 직접 아이콘을 지정하면
         * Inspector 매핑보다 그 아이콘을 우선 사용한다.
         */
        if (effect.Data.icon != null)
        {
            return effect.Data.icon;
        }

        switch (
            effect.Data.effectType)
        {
            case StatusEffectType.Poison:
                return poisonIcon;

            case StatusEffectType.Stun:
                return stunIcon;

            case StatusEffectType.AttackPowerUp:
                return attackPowerUpIcon;

            case StatusEffectType.AttackPowerDown:
                return attackPowerDownIcon;

            case StatusEffectType.DefenseUp:
                return defenseUpIcon;

            case StatusEffectType.DefenseDown:
                return defenseDownIcon;

            case StatusEffectType.AccuracyUp:
                return accuracyUpIcon;

            case StatusEffectType.AccuracyDown:
                return accuracyDownIcon;

            case StatusEffectType.EvasionUp:
                return evasionUpIcon;

            case StatusEffectType.EvasionDown:
                return evasionDownIcon;

            case StatusEffectType.DamageTakenUp:
                return damageTakenUpIcon;

            case StatusEffectType.DamageTakenDown:
                return damageTakenDownIcon;

            case StatusEffectType.HealingUp:
                return healingUpIcon;

            case StatusEffectType.HealingDown:
                return healingDownIcon;

            case StatusEffectType.StrengthUp:
                return strengthUpIcon;

            case StatusEffectType.StrengthDown:
                return strengthDownIcon;

            case StatusEffectType.DexterityUp:
                return dexterityUpIcon;

            case StatusEffectType.DexterityDown:
                return dexterityDownIcon;

            case StatusEffectType.ConstitutionUp:
                return constitutionUpIcon;

            case StatusEffectType.ConstitutionDown:
                return constitutionDownIcon;

            case StatusEffectType.IntelligenceUp:
                return intelligenceUpIcon;

            case StatusEffectType.IntelligenceDown:
                return intelligenceDownIcon;
        }

        return null;
    }

    private void ClearIcons()
    {
        foreach (
            StatusEffectIconUI iconUI
            in iconUIs)
        {
            if (iconUI != null)
            {
                Destroy(
                    iconUI.gameObject
                );
            }
        }

        iconUIs.Clear();
    }

    public void ShowTooltip(
        ActiveStatusEffect effect)
    {
        if (effect == null ||
            effect.Data == null)
        {
            return;
        }

        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(true);
        }

        if (tooltipNameText != null)
        {
            tooltipNameText.text =
                effect.Data.buffName;
        }

        if (tooltipDescriptionText != null)
        {
            tooltipDescriptionText.text =
                effect.Data.description;
        }

        if (tooltipDurationText != null)
        {
            tooltipDurationText.text =
                effect.IsInfinite
                    ? "남은 턴: ∞"
                    : "남은 턴: " +
                      effect.RemainingDuration;
        }
    }

    public void HideTooltip()
    {
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }
    }
}