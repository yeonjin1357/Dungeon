using UnityEngine;
using Dungeon.Enemies.States;

namespace Dungeon.Enemies.Monsters
{
    /// <summary>
    /// 쥐 - 기본 근접 몬스터
    /// </summary>
    public class Rat : EnemyBase
    {
        protected override void Awake()
        {
            // 쥐 기본 스탯 설정
            _maxHealth = 5;
            _attackPower = 2;
            _defense = 0;
            _moveSpeed = 0.3f;
            _expReward = 5;
            _detectionRange = 4f;
            _attackRange = 1.5f;
            
            base.Awake();
        }
        
        /// <summary>
        /// 쥐의 상태 초기화
        /// </summary>
        protected override void InitializeStates()
        {
            // IdleState 등록
            _stateMachine.RegisterState(EnemyState.Idle, new IdleState(this, _stateMachine));
            
            // PatrolState 등록 - 쥐는 가끔 랜덤하게 움직임
            _stateMachine.RegisterState(EnemyState.Patrol, new PatrolState(this, _stateMachine));
            
            // ChaseState 등록 - 플레이어 추적
            // FleeState 등록 - 도망 상태
            _stateMachine.RegisterState(EnemyState.Flee, new FleeState(this, _stateMachine));
            
            _stateMachine.RegisterState(EnemyState.Chase, new ChaseState(this, _stateMachine));
            
            // AttackState 등록 - 근접 공격
            _stateMachine.RegisterState(EnemyState.Attack, new AttackState(this, _stateMachine));
            
            // DeadState 등록
            _stateMachine.RegisterState(EnemyState.Dead, new DeadState(this, _stateMachine));
        }
        
        /// <summary>
        /// 쥐만의 특수 공격 패턴 (선택적 오버라이드)
        /// </summary>
        public override void Attack()
        {
            base.Attack();
            
            // 쥐는 낮은 확률로 독 공격 추가 (나중에 구현)
            // if (Random.Range(0f, 1f) < 0.1f)
            // {
            //     ApplyPoison();
            // }
        }
        
        /// <summary>
        /// 아이템 드롭
        /// </summary>
        protected override void DropItems()
        {
            // 25% 확률로 치즈 드롭 (나중에 아이템 시스템 연동)
            if (Random.Range(0f, 1f) < 0.25f)
            {
                Debug.Log("Rat dropped cheese!");
                // TODO: 아이템 드롭 시스템 연동
            }
        }
        
        protected override void OnDamaged(int damage)
        {
            base.OnDamaged(damage);
            
            // 쥐는 체력이 낮아지면 도망칠 수 있음
            if (_currentHealth <= _maxHealth * 0.3f && Random.Range(0f, 1f) < 0.3f)
            {
                _stateMachine.ChangeState(EnemyState.Flee);
            }
        }
    }
}