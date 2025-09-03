using UnityEngine;
using Dungeon.Combat;

namespace Dungeon.Enemies.States
{
    /// <summary>
    /// 도망 상태 - 플레이어로부터 멀어지려고 함
    /// </summary>
    public class FleeState : IEnemyState
    {
        private EnemyBase _enemy;
        private EnemyStateMachine _stateMachine;
        
        public FleeState(EnemyBase enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }
        
        public void Enter(EnemyBase enemy)
        {
            Debug.Log($"{enemy.name} is fleeing!");
        }
        
        public void Execute(EnemyBase enemy)
        {
            // 실시간 업데이트는 필요 없음 (턴제)
        }
        
        public void ExecuteTurn()
        {
            // 플레이어가 없으면 Idle로 전환
            if (_enemy.Player == null)
            {
                _stateMachine.ChangeState(EnemyState.Idle);
                EndTurn();
                return;
            }
            
            // 플레이어 반대 방향으로 이동
            Vector2Int currentPos = _enemy.GridPosition;
            Vector2Int playerPos = new Vector2Int(
                Mathf.RoundToInt(_enemy.Player.transform.position.x),
                Mathf.RoundToInt(_enemy.Player.transform.position.y)
            );
            
            Vector2Int fleeDirection = currentPos - playerPos;
            
            // 방향 정규화 (한 칸씩만 이동)
            if (Mathf.Abs(fleeDirection.x) > Mathf.Abs(fleeDirection.y))
            {
                fleeDirection.x = fleeDirection.x > 0 ? 1 : -1;
                fleeDirection.y = 0;
            }
            else if (fleeDirection.y != 0)
            {
                fleeDirection.y = fleeDirection.y > 0 ? 1 : -1;
                fleeDirection.x = 0;
            }
            else
            {
                // 같은 위치면 랜덤 방향으로 도망
                fleeDirection = GetRandomDirection();
            }
            
            Vector2Int targetPos = currentPos + fleeDirection;
            
            // 이동 가능한지 체크
            if (_enemy.CanMoveTo(targetPos))
            {
                _enemy.MoveToGrid(targetPos);
            }
            else
            {
                // 다른 방향 시도
                Vector2Int alternativeDir = GetAlternativeFleeDirection(fleeDirection);
                targetPos = currentPos + alternativeDir;
                
                if (_enemy.CanMoveTo(targetPos))
                {
                    _enemy.MoveToGrid(targetPos);
                }
                else
                {
                    // 이동할 수 없으면 턴 종료
                    EndTurn();
                }
            }
            
            // 충분히 멀어졌으면 Idle로 전환
            float distance = _enemy.GetDistanceToPlayer();
            if (distance > _enemy.DetectionRange * 1.5f)
            {
                _stateMachine.ChangeState(EnemyState.Idle);
            }
        }
        
        public void Exit(EnemyBase enemy)
        {
            // 도망 상태 종료
        }
        
        private void EndTurn()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.EndCurrentTurn();
            }
        }
        
        private Vector2Int GetRandomDirection()
        {
            int random = Random.Range(0, 4);
            switch (random)
            {
                case 0: return Vector2Int.up;
                case 1: return Vector2Int.down;
                case 2: return Vector2Int.left;
                case 3: return Vector2Int.right;
                default: return Vector2Int.zero;
            }
        }
        
        private Vector2Int GetAlternativeFleeDirection(Vector2Int originalDirection)
        {
            // 원래 방향이 막혔을 때 대체 방향 찾기
            if (originalDirection.x != 0)
            {
                // 좌우로 도망치려했으면 상하로 시도
                return Random.Range(0, 2) == 0 ? Vector2Int.up : Vector2Int.down;
            }
            else
            {
                // 상하로 도망치려했으면 좌우로 시도
                return Random.Range(0, 2) == 0 ? Vector2Int.left : Vector2Int.right;
            }
        }
    }
}