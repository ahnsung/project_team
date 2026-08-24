public enum StatusEffectType
{
    None = 0,

    // 현재 구현된 상태이상
    Stun = 1,
    Poison = 2,
    StatIncrease = 3,

    // 방어 상태
    Guard = 9,

    // =============================
    // 전투 능력치 버프 / 디버프
    // =============================

    AttackPowerUp = 10,
    AttackPowerDown = 11,

    DefenseUp = 12,
    DefenseDown = 13,

    AccuracyUp = 14,
    AccuracyDown = 15,

    EvasionUp = 16,
    EvasionDown = 17,

    // 받는 피해량 자체를 조정
    DamageTakenUp = 18,
    DamageTakenDown = 19,

    // 회복량 보정
    HealingUp = 20,
    HealingDown = 21,

    // =============================
    // 기본 스탯
    // =============================

    StrengthUp = 30,
    StrengthDown = 31,

    DexterityUp = 32,
    DexterityDown = 33,

    ConstitutionUp = 34,
    ConstitutionDown = 35,

    IntelligenceUp = 36,
    IntelligenceDown = 37
}