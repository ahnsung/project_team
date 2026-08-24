using UnityEngine;

public class StatusEffectTest : MonoBehaviour
{
    [SerializeField]
    private StatusEffectController target;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<StatusEffectController>();
        }
    }

    private void Update()
    {
        if (target == null)
            return;

        // G 키: Guard 추가
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGuard();
        }

        // H 키: 플레이어 턴 종료 타이밍 처리
        if (Input.GetKeyDown(KeyCode.H))
        {
            target.ProcessTiming(
                StatusEffectTiming.PlayerTeamEnd
            );
        }

        // J 키: 적 팀 종료 타이밍 처리
        if (Input.GetKeyDown(KeyCode.J))
        {
            target.ProcessTiming(
                StatusEffectTiming.EnemyTeamEnd
            );
        }

        // K 키: 전체 턴 종료 처리
        if (Input.GetKeyDown(KeyCode.K))
        {
            target.ProcessTiming(
                StatusEffectTiming.TurnEnd
            );
        }
    }

    private void AddGuard()
    {
        StatusEffectData guard =
            new StatusEffectData
            {
                id = 9001,
                buffName = "방어",
                description =
                    "적의 공격 피해를 50% 감소시키고 " +
                    "방어구 내구도 소모가 2배가 됩니다.",

                tendency =
                    StatusEffectTendency.Positive,

                effectType =
                    StatusEffectType.Guard,

                effectPower = 50,

                buffDuration = 1,

                whenDecreaseDuration =
                    StatusEffectTiming.EnemyTeamEnd,

                whenBuffEffect =
                    StatusEffectTiming.None,

                canStack = false,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            };

        target.AddStatusEffect(
            guard
        );

        Debug.Log(
            "[StatusEffectTest] Guard 추가"
        );
    }
}