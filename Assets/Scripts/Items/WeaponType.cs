using System;

namespace Dungeon.Items
{
    /// <summary>
    /// 무기의 종류를 정의하는 열거형
    /// </summary>
    [Serializable]
    public enum WeaponType
    {
        /// <summary>
        /// 한손검 - 밸런스형 무기
        /// </summary>
        OneHandedSword,
        
        /// <summary>
        /// 양손검 - 높은 공격력, 느린 공격속도
        /// </summary>
        TwoHandedSword,
        
        /// <summary>
        /// 단검 - 빠른 공격속도, 크리티컬 특화
        /// </summary>
        Dagger,
        
        /// <summary>
        /// 창 - 긴 사거리, 관통 공격
        /// </summary>
        Spear,
        
        /// <summary>
        /// 도끼 - 높은 공격력, 방어구 관통
        /// </summary>
        Axe,
        
        /// <summary>
        /// 둔기 - 스턴 효과, 방어력 무시
        /// </summary>
        Mace,
        
        /// <summary>
        /// 활 - 원거리 공격
        /// </summary>
        Bow,
        
        /// <summary>
        /// 석궁 - 강력한 원거리, 느린 재장전
        /// </summary>
        Crossbow,
        
        /// <summary>
        /// 지팡이 - 마법 공격
        /// </summary>
        Staff,
        
        /// <summary>
        /// 완드 - 빠른 마법 공격
        /// </summary>
        Wand
    }

    /// <summary>
    /// 게임 내 스탯 종류를 정의하는 열거형
    /// </summary>
    [Serializable]
    public enum StatType
    {
        // 기본 스탯
        MaxHealth,          // 최대 체력
        MaxMana,            // 최대 마나
        HealthRegen,        // 체력 재생
        ManaRegen,          // 마나 재생
        
        // 공격 스탯
        AttackPower,        // 공격력
        MagicPower,         // 마법 공격력
        CriticalChance,     // 크리티컬 확률 (%)
        CriticalDamage,     // 크리티컬 데미지 배율
        AttackSpeed,        // 공격 속도
        
        // 방어 스탯
        Defense,            // 물리 방어력
        MagicResistance,    // 마법 저항력
        DodgeChance,        // 회피 확률 (%)
        BlockChance,        // 방어 확률 (%)
        DamageReduction,    // 데미지 감소 (%)
        
        // 이동 스탯
        MoveSpeed,          // 이동 속도
        
        // 기타 스탯
        Luck,               // 행운 (드롭률 증가)
        ExpBonus,           // 경험치 보너스 (%)
        GoldBonus           // 골드 획득 보너스 (%)
    }

    /// <summary>
    /// 무기 공격 타입
    /// </summary>
    [Serializable]
    public enum DamageType
    {
        Physical,   // 물리 데미지
        Magical,    // 마법 데미지
        True        // 고정 데미지 (방어 무시)
    }
}