using UnityEngine;
using System.Collections.Generic;

namespace Dungeon.Items.Interfaces
{
    /// <summary>
    /// 장착 가능한 장비의 슬롯 타입
    /// </summary>
    public enum EquipmentSlot
    {
        None,
        Weapon,     // 무기
        Head,       // 머리
        Body,       // 몸통
        Hands,      // 장갑
        Feet,       // 신발
        Accessory1, // 액세서리1 (반지 등)
        Accessory2  // 액세서리2 (목걸이 등)
    }

    /// <summary>
    /// 장착 가능한 아이템이 구현해야 하는 인터페이스
    /// </summary>
    public interface IEquippable
    {
        /// <summary>
        /// 장비 슬롯 타입
        /// </summary>
        EquipmentSlot SlotType { get; }
        
        /// <summary>
        /// 장착 가능한지 확인
        /// </summary>
        /// <param name="wearer">장착하려는 GameObject</param>
        /// <returns>장착 가능 여부</returns>
        bool CanEquip(GameObject wearer);
        
        /// <summary>
        /// 아이템 장착
        /// </summary>
        /// <param name="wearer">장착하는 GameObject</param>
        void Equip(GameObject wearer);
        
        /// <summary>
        /// 아이템 장착 해제
        /// </summary>
        /// <param name="wearer">장착 해제하는 GameObject</param>
        void Unequip(GameObject wearer);
        
        /// <summary>
        /// 장착 시 적용되는 스탯 보너스
        /// </summary>
        Dictionary<string, float> GetStatBonuses();
        
        /// <summary>
        /// 장착 필요 레벨
        /// </summary>
        int RequiredLevel { get; }
        
        /// <summary>
        /// 장착 필요 클래스 (null이면 모든 클래스 가능)
        /// </summary>
        string RequiredClass { get; }
    }
}