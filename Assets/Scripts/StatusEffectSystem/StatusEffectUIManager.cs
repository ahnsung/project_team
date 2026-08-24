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

    [Header("Icons")]
    [SerializeField]
    private Sprite guardIcon;

    [SerializeField]
    private Sprite poisonIcon;

    [SerializeField]
    private Sprite stunIcon;

    [SerializeField]
    private Sprite statIncreaseIcon;

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
            tooltipRoot.SetActive(false);

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
            if (controller.GetComponent<BattleUnit>() == null)
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
                Destroy(iconObject);
                continue;
            }

            Sprite sprite =
                GetIcon(
                    effect.Data.effectType
                );

            iconUI.Setup(
                effect,
                this,
                sprite
            );

            iconUIs.Add(iconUI);
        }
    }

    private Sprite GetIcon(
        StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Guard:
                return guardIcon;

            case StatusEffectType.Poison:
                return poisonIcon;

            case StatusEffectType.Stun:
                return stunIcon;

            case StatusEffectType.StatIncrease:
                return statIncreaseIcon;

            default:
                return null;
        }
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
            tooltipRoot.SetActive(true);

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