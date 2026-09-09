using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusEffectUIManager : MonoBehaviour
{
    // =========================================================
    // Target
    // =========================================================

    [Header("Target")]
    [SerializeField]
    private StatusEffectController targetController;


    // =========================================================
    // Icon UI
    // =========================================================

    [Header("Icon UI")]
    [SerializeField]
    private Transform iconContainer;

    [SerializeField]
    private GameObject iconPrefab;


    // =========================================================
    // Common Icons
    // =========================================================

    [Header("Common Icons")]

    [Tooltip("일반적인 긍정 버프에 사용")]
    [SerializeField]
    private Sprite buffUpIcon;

    [Tooltip("일반적인 부정 디버프에 사용")]
    [SerializeField]
    private Sprite buffDownIcon;


    // =========================================================
    // Special Icons
    // =========================================================

    [Header("Special Status Icons")]

    [SerializeField]
    private Sprite guardIcon;

    [SerializeField]
    private Sprite poisonIcon;

    [SerializeField]
    private Sprite stunIcon;

    [SerializeField]
    private Sprite corrosionIcon;


    // =========================================================
    // Hunger
    // =========================================================

    [Header("Hunger Icons")]

    [SerializeField]
    private Sprite hungerIcon;

    [SerializeField]
    private Sprite hungerDownIcon;


    // =========================================================
    // Frustration / Mental
    // =========================================================

    [Header("Frustration Icons")]

    [SerializeField]
    private Sprite frustrationIcon;

    [SerializeField]
    private Sprite frustrationDownIcon;


    // =========================================================
    // Tooltip
    // =========================================================

    [Header("Tooltip")]

    [SerializeField]
    private GameObject tooltipRoot;

    [SerializeField]
    private TextMeshProUGUI tooltipNameText;

    [SerializeField]
    private TextMeshProUGUI tooltipDescriptionText;

    [SerializeField]
    private TextMeshProUGUI tooltipDurationText;


    // =========================================================
    // Runtime
    // =========================================================

    private readonly List<StatusEffectIconUI> iconUIs =
        new List<StatusEffectIconUI>();


    // =========================================================
    // Unity
    // =========================================================

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


    // =========================================================
    // Target
    // =========================================================

    private void ResolveTarget()
    {
        if (targetController != null)
            return;

        StatusEffectController[] controllers =
            FindObjectsByType<StatusEffectController>(
                FindObjectsSortMode.None
            );

        foreach (StatusEffectController controller in controllers)
        {
            if (controller == null)
                continue;

            BattleUnit unit =
                controller.GetComponent<BattleUnit>();

            if (
                BattleManager.Instance != null &&
                BattleManager.Instance.playerUnit != null &&
                unit == BattleManager.Instance.playerUnit
            )
            {
                targetController = controller;
                return;
            }
        }

        Debug.LogWarning(
            "[StatusEffectUIManager] 플레이어 StatusEffectController를 찾지 못했습니다."
        );
    }


    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshUI()
    {
        ClearIcons();

        if (
            targetController == null ||
            iconContainer == null ||
            iconPrefab == null
        )
        {
            return;
        }

        foreach (
            ActiveStatusEffect effect
            in targetController.ActiveEffects)
        {
            if (
                effect == null ||
                effect.Data == null
            )
            {
                continue;
            }

            Sprite sprite =
                GetIcon(effect);

            if (sprite == null)
            {
                Debug.LogWarning(
                    "[StatusEffectUIManager] 아이콘을 찾지 못했습니다.\n" +
                    $"이름: {effect.Data.buffName}\n" +
                    $"Type: {effect.Data.effectType}"
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


    // =========================================================
    // Icon Select
    // =========================================================

    private Sprite GetIcon(
        ActiveStatusEffect effect)
    {
        if (
            effect == null ||
            effect.Data == null
        )
        {
            return null;
        }

        StatusEffectData data =
            effect.Data;


        // =====================================================
        // 1순위
        // 데이터 자체 아이콘
        // =====================================================

        if (data.icon != null)
        {
            return data.icon;
        }


        // =====================================================
        // 2순위
        // 특수 상태
        // =====================================================

        switch (data.effectType)
        {
            case StatusEffectType.Guard:

                return guardIcon;


            case StatusEffectType.Poison:

                return poisonIcon;


            case StatusEffectType.Stun:

                return stunIcon;
        }


        // =====================================================
        // 3순위
        // 이름 기준 특수 상태
        //
        // 아직 별도 StatusEffectType이 없는 상태까지 대응
        // =====================================================

        string buffName =
            data.buffName ?? string.Empty;


        // -----------------------------------------------------
        // 부식
        // -----------------------------------------------------

        if (
            buffName.Contains("부식")
        )
        {
            return corrosionIcon;
        }


        // -----------------------------------------------------
        // Hunger
        // -----------------------------------------------------

        if (
            buffName.Contains("배고픔")
        )
        {
            /*
             * 이름에 감소 / 악화 등이 포함되어 있으면
             * Hunger Down 사용
             */

            if (
                buffName.Contains("감소") ||
                buffName.Contains("악화") ||
                buffName.Contains("25") ||
                buffName.Contains("50")
            )
            {
                return hungerDownIcon;
            }

            return hungerIcon;
        }


        // -----------------------------------------------------
        // Frustration / Mental
        // -----------------------------------------------------

        if (
            buffName.Contains("좌절") ||
            buffName.Contains("정신력")
        )
        {
            if (
                buffName.Contains("감소") ||
                buffName.Contains("악화") ||
                buffName.Contains("25") ||
                buffName.Contains("50")
            )
            {
                return frustrationDownIcon;
            }

            return frustrationIcon;
        }


        // =====================================================
        // 4순위
        // 일반 Positive / Negative
        // =====================================================

        switch (data.tendency)
        {
            case StatusEffectTendency.Positive:

                return buffUpIcon;


            case StatusEffectTendency.Negative:

                return buffDownIcon;
        }


        return null;
    }


    // =========================================================
    // Clear
    // =========================================================

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


    // =========================================================
    // Tooltip
    // =========================================================

    public void ShowTooltip(
        ActiveStatusEffect effect)
    {
        if (
            effect == null ||
            effect.Data == null
        )
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