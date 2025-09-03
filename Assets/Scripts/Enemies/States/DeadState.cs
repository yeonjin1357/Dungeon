using UnityEngine;
using Dungeon.Combat;

namespace Dungeon.Enemies.States
{
    /// <summary>
    /// 죽음 상태
    /// </summary>
    public class DeadState : IEnemyState
    {
        private EnemyBase _enemy;
        private EnemyStateMachine _stateMachine;
        
        public DeadState(EnemyBase enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }
        
        public void Enter(EnemyBase enemy)
        {
            Debug.Log($"{enemy.name} has died!");
            // 죽음 애니메이션 시작 (나중에 구현)
        }
        
        public void Execute(EnemyBase enemy)
        {
            // 죽음 상태에서는 아무것도 하지 않음
        }
        
        public void ExecuteTurn()
        {
            // 죽은 상태에서는 턴을 즉시 종료
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.EndCurrentTurn();
            }
        }
        
        public void Exit(EnemyBase enemy)
        {
            // 죽음 상태에서는 다른 상태로 전환되지 않음
        }
    }
}
