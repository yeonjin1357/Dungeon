using UnityEngine;
using Dungeon.Combat;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 추적 상태 - 플레이어를 향해 이동
    /// </summary>
    public class ChaseState : IEnemyState
    {
        private EnemyBase _enemy;
        private EnemyStateMachine _stateMachine;
        
        public ChaseState(EnemyBase enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }
        
        public void Enter(EnemyBase enemy)
        {
            Debug.Log($"{enemy.name} entered Chase state");
        }
        
        public void Execute(EnemyBase enemy)
        {
            // 실시간 업데이트는 필요 없음 (턴제)
        }
        
        public void ExecuteTurn()
        {
            // 플레이어 감지
            if (_enemy.DetectPlayer())
            {
                float distance = _enemy.GetDistanceToPlayer();
                
                // 공격 범위 내에 있으면 공격 상태로 전환
                if (distance <= _enemy.AttackRange)
                {
                    _stateMachine.ChangeState(EnemyState.Attack);
                    return;
                }
                
                // 플레이어를 향해 이동
                Vector2Int nextPos = _enemy.GetNextPositionTowardsPlayer();
                if (_enemy.CanMoveTo(nextPos))
                {
                    _enemy.MoveToGrid(nextPos);
                }
            }
            else
            {
                // 플레이어를 놓치면 순찰 상태로 전환
                _stateMachine.ChangeState(EnemyState.Patrol);
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