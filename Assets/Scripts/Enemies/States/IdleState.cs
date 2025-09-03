using UnityEngine;
using Dungeon.Combat;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 대기 상태 - 제자리에서 대기하며 주변 감시
    /// </summary>
    public class IdleState : IEnemyState
    {
        private EnemyBase _enemy;
        private EnemyStateMachine _stateMachine;
        
        public IdleState(EnemyBase enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }
        
        public void Enter(EnemyBase enemy)
        {
            Debug.Log($"{enemy.name} entered Idle state");
        }
        
        public void Execute(EnemyBase enemy)
        {
            // 실시간 업데이트는 필요 없음 (턴제)
        }
        
        public void ExecuteTurn()
        {
            // 플레이어 감지 시 추적 상태로 전환
            if (_enemy.DetectPlayer())
            {
                _stateMachine.ChangeState(EnemyState.Chase);
            }
            
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