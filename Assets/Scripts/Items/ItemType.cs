using System;

namespace Dungeon.Items
{
    /// <summary>
    /// 게임 내 아이템의 종류를 정의하는 열거형
    /// </summary>
    [Serializable]
    public enum ItemType
    {
        /// <summary>
        /// 무기류 - 검, 활, 지팡이 등
        /// </summary>
        Weapon,
        
        /// <summary>
        /// 방어구류 - 투구, 갑옷, 장갑, 신발 등
        /// </summary>
        Armor,
        
        /// <summary>
        /// 소비 아이템 - 포션, 스크롤, 음식 등
        /// </summary>
        Consumable,
        
        /// <summary>
        /// 열쇠 아이템 - 던전 열쇠, 퀘스트 아이템 등
        /// </summary>
        Key,
        
        /// <summary>
        /// 기타 아이템 - 재료, 수집품 등
        /// </summary>
        Misc
    }

    /// <summary>
    /// 아이템 희귀도 정의
    /// </summary>
    [Serializable]
    public enum ItemRarity
    {
        Common,     // 일반
        Uncommon,   // 고급
        Rare,       // 희귀
        Epic,       // 영웅
        Legendary   // 전설
    }
}