using UnityEngine;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 적 상태 머신의 상태 인터페이스
    /// </summary>
    public interface IEnemyState
    {
        void Enter(EnemyBase enemy);
        void Execute(EnemyBase enemy);
        void ExecuteTurn();
        void Exit(EnemyBase enemy);
    }
}