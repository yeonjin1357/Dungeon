using UnityEngine;
using Dungeon.Combat;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 공격 상태 - 플레이어를 공격
    /// </summary>
    public class AttackState : IEnemyState
    {
        private EnemyBase _enemy;
        private EnemyStateMachine _stateMachine;
        
        public AttackState(EnemyBase enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }
        
        public void Enter(EnemyBase enemy)
        {
            Debug.Log($"{enemy.name} entered Attack state");
        }
        
        public void Execute(EnemyBase enemy)
        {
            // 실시간 업데이트는 필요 없음 (턴제)
        }
        
        public void ExecuteTurn()
        {
            float distance = _enemy.GetDistanceToPlayer();
            
            // 공격 범위를 벗어나면 추적 상태로 전환
            if (distance > _enemy.AttackRange)
            {
                _stateMachine.ChangeState(EnemyState.Chase);
                return;
            }
            
            // 플레이어 공격
            _enemy.Attack();
            
            // 턴 종료
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.EndCurrentTurn();
            }
        }
        
        public void Exit(EnemyBase enemy)
        {
            // 상태 종료 시 처리
        }
    }
}