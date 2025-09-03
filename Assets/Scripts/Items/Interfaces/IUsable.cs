using UnityEngine;

namespace Dungeon.Items.Interfaces
{
    /// <summary>
    /// 사용 가능한 아이템이 구현해야 하는 인터페이스
    /// </summary>
    public interface IUsable
    {
        /// <summary>
        /// 아이템을 사용할 수 있는지 확인
        /// </summary>
        /// <param name="user">아이템을 사용하려는 GameObject</param>
        /// <returns>사용 가능 여부</returns>
        bool CanUse(GameObject user);
        
        /// <summary>
        /// 아이템 사용
        /// </summary>
        /// <param name="user">아이템을 사용하는 GameObject</param>
        /// <returns>사용 성공 여부</returns>
        bool Use(GameObject user);
        
        /// <summary>
        /// 아이템 사용 후 효과
        /// </summary>
        /// <param name="user">아이템을 사용한 GameObject</param>
        void OnUseComplete(GameObject user);
        
        /// <summary>
        /// 사용 후 아이템이 소비되는지 여부
        /// </summary>
        bool IsConsumable { get; }
        
        /// <summary>
        /// 재사용 대기시간 (초 단위, 0이면 즉시 재사용 가능)
        /// </summary>
        float CooldownTime { get; }
    }
}