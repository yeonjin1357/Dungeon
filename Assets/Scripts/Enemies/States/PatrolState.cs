using UnityEngine;
using Dungeon.Combat;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 순찰 상태 - 무작위로 이동하며 순찰
    /// </summary>
    public class PatrolState : IEnemyState
    {
        private EnemyBase _enemy;
        private EnemyStateMachine _stateMachine;
        
        public PatrolState(EnemyBase enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }
        
        public void Enter(EnemyBase enemy)
        {
            Debug.Log($"{enemy.name} entered Patrol state");
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
                return;
            }
            
            // 랜덤한 방향으로 이동
            Vector2Int currentPos = _enemy.GridPosition;
            Vector2Int[] directions = {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
            
            // 랜덤한 방향 선택
            Vector2Int randomDir = directions[Random.Range(0, directions.Length)];
            Vector2Int newPos = currentPos + randomDir;
            
            // 이동 가능한지 확인
            if (_enemy.CanMoveTo(newPos))
            {
                _enemy.MoveToGrid(newPos);
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