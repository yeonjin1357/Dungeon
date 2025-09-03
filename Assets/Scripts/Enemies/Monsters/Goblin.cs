using UnityEngine;
using Dungeon.Enemies.States;

namespace Dungeon.Enemies.Monsters
{
    /// <summary>
    /// 고블린 - 중급 근접 몬스터 (그룹 전투 보너스)
    /// </summary>
    public class Goblin : EnemyBase
    {
        [Header("Goblin Special")]
        [SerializeField] private float _groupBonusRange = 5f;
        [SerializeField] private int _groupBonusDamage = 2;
        [SerializeField] private bool _hasCalledReinforcements = false;
        
        protected override void Awake()
        {
            // 고블린 기본 스탯 설정
            _maxHealth = 8;
            _attackPower = 4;
            _defense = 1;
            _moveSpeed = 0.4f;
            _expReward = 12;
            _detectionRange = 5f;
            _attackRange = 1.5f;
            
            base.Awake();
        }
        
        /// <summary>
        /// 고블린의 상태 초기화
        /// </summary>
        protected override void InitializeStates()
        {
            // IdleState 등록
            _stateMachine.RegisterState(EnemyState.Idle, new IdleState(this, _stateMachine));
            
            // PatrolState 등록 - 고블린은 적극적으로 순찰
            _stateMachine.RegisterState(EnemyState.Patrol, new PatrolState(this, _stateMachine));
            
            // ChaseState 등록 - 플레이어 추적
            _stateMachine.RegisterState(EnemyState.Chase, new ChaseState(this, _stateMachine));
            
            // AttackState 등록 - 근접 공격
            _stateMachine.RegisterState(EnemyState.Attack, new AttackState(this, _stateMachine));
            
            // FleeState 등록 - 체력이 낮아지면 도망
            _stateMachine.RegisterState(EnemyState.Flee, new FleeState(this, _stateMachine));
            
            // DeadState 등록
            _stateMachine.RegisterState(EnemyState.Dead, new DeadState(this, _stateMachine));
        }
        
        /// <summary>
        /// 고블린 특수 능력 - 그룹 전투 보너스
        /// </summary>
        public override void Attack()
        {
            int bonusDamage = GetGroupBonus();
            int totalDamage = _attackPower + bonusDamage;
            
            if (bonusDamage > 0)
            {
                Debug.Log($"Goblin attacks with group bonus! (+{bonusDamage} damage)");
            }
            
            // 실제 공격 수행
            if (_playerController != null)
            {
                _playerController.TakeDamage(totalDamage);
            }
        }
        
        /// <summary>
        /// 주변 고블린 수에 따른 보너스 계산
        /// </summary>
        private int GetGroupBonus()
        {
            int nearbyGoblins = 0;
            
            // 주변의 고블린 검색
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, _groupBonusRange);
            
            foreach (Collider2D col in nearbyEnemies)
            {
                if (col.gameObject != gameObject)
                {
                    Goblin otherGoblin = col.GetComponent<Goblin>();
                    if (otherGoblin != null && otherGoblin.IsActive)
                    {
                        nearbyGoblins++;
                    }
                }
            }
            
            // 주변 고블린당 보너스 데미지
            return nearbyGoblins * _groupBonusDamage;
        }
        
        /// <summary>
        /// 지원 요청
        /// </summary>
        protected override void OnDamaged(int damage)
        {
            base.OnDamaged(damage);
            
            // 체력이 30% 이하가 되면 지원 요청
            if (!_hasCalledReinforcements && _currentHealth <= _maxHealth * 0.3f)
            {
                CallForReinforcements();
                _hasCalledReinforcements = true;
            }
            
            // 체력이 20% 이하가 되면 도망
            if (_currentHealth <= _maxHealth * 0.2f)
            {
                _stateMachine.ChangeState(EnemyState.Flee);
            }
        }
        
        /// <summary>
        /// 주변 고블린들에게 경고
        /// </summary>
        private void CallForReinforcements()
        {
            Debug.Log("Goblin calls for reinforcements!");
            
            // 주변 고블린들을 플레이어 쪽으로 유도
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, _detectionRange * 1.5f);
            
            foreach (Collider2D col in nearbyEnemies)
            {
                if (col.gameObject != gameObject)
                {
                    Goblin otherGoblin = col.GetComponent<Goblin>();
                    if (otherGoblin != null && otherGoblin.IsActive)
                    {
                        // 다른 고블린들을 추적 상태로 전환
                        otherGoblin.ChangeState(EnemyState.Chase);
                    }
                }
            }
        }
        
        /// <summary>
        /// 아이템 드롭
        /// </summary>
        protected override void DropItems()
        {
            float dropRoll = Random.Range(0f, 1f);
            
            if (dropRoll < 0.2f)
            {
                Debug.Log("Goblin dropped a small dagger!");
                // TODO: 단검 드롭
            }
            else if (dropRoll < 0.35f)
            {
                Debug.Log("Goblin dropped some coins!");
                // TODO: 코인 드롭
            }
        }
        
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // 그룹 보너스 범위 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _groupBonusRange);
        }
    }
}