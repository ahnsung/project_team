public enum StatusEffectTiming
{
    // 상태이상이 걸린 본인의 순서
    SelfTurnStart = 0,
    SelfTurnEnd = 1,

    // 플레이어 팀 순서
    PlayerTeamStart = 2,
    PlayerTeamEnd = 3,

    // 적 팀 순서
    EnemyTeamStart = 4,
    EnemyTeamEnd = 5,

    // 전체 턴
    TurnStart = 6,
    TurnEnd = 7,

    // 지속시간 감소 없음
    None = 8
}