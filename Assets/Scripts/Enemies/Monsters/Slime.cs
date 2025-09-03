using UnityEngine;
using Dungeon.Enemies.States;

namespace Dungeon.Enemies.Monsters
{
    /// <summary>
    /// 슬라임 - 기본 근접 몬스터 (분열 능력 있음)
    /// </summary>
    public class Slime : EnemyBase
    {
        [Header("Slime Special")]
        [SerializeField] private bool _canSplit = true;
        [SerializeField] private int _splitHealthThreshold = 10;
        [SerializeField] private GameObject _miniSlimePrefab; // 작은 슬라임 프리팹
        
        protected override void Awake()
        {
            // 슬라임 기본 스탯 설정
            _maxHealth = 15;
            _attackPower = 3;
            _defense = 1;
            _moveSpeed = 0.5f; // 느린 이동 속도
            _expReward = 8;
            _detectionRange = 3f; // 짧은 감지 거리
            _attackRange = 1.5f;
            
            base.Awake();
        }
        
        /// <summary>
        /// 슬라임의 상태 초기화
        /// </summary>
        protected override void InitializeStates()
        {
            // IdleState 등록
            _stateMachine.RegisterState(EnemyState.Idle, new IdleState(this, _stateMachine));
            
            // PatrolState 등록 - 슬라임은 천천히 무작위로 움직임
            _stateMachine.RegisterState(EnemyState.Patrol, new PatrolState(this, _stateMachine));
            
            // ChaseState 등록 - 플레이어 추적
            _stateMachine.RegisterState(EnemyState.Chase, new ChaseState(this, _stateMachine));
            
            // AttackState 등록 - 근접 공격
            _stateMachine.RegisterState(EnemyState.Attack, new AttackState(this, _stateMachine));
            
            // DeadState 등록
            _stateMachine.RegisterState(EnemyState.Dead, new DeadState(this, _stateMachine));
            
            // 슬라임은 도망가지 않음
        }
        
        /// <summary>
        /// 슬라임 특수 능력 - 분열
        /// </summary>
        protected override void OnDamaged(int damage)
        {
            base.OnDamaged(damage);
            
            // 체력이 절반 이하가 되면 분열 시도
            if (_canSplit && _currentHealth <= _maxHealth / 2 && _currentHealth > 0)
            {
                TryToSplit();
                _canSplit = false; // 한 번만 분열
            }
        }
        
        private void TryToSplit()
        {
            // 분열 확률 50%
            if (Random.Range(0f, 1f) > 0.5f)
                return;
            
            // 미니 슬라임이 설정되어 있지 않으면 분열하지 않음
            if (_miniSlimePrefab == null)
            {
                Debug.LogWarning("Mini slime prefab not set for splitting");
                return;
            }
            
            // 2개의 미니 슬라임 생성
            for (int i = 0; i < 2; i++)
            {
                Vector2Int spawnOffset = new Vector2Int(
                    Random.Range(-1, 2),
                    Random.Range(-1, 2)
                );
                
                Vector2Int spawnPos = GridPosition + spawnOffset;
                
                // 생성 위치가 유효한지 확인
                if (CanMoveTo(spawnPos))
                {
                    Vector3 worldPos = new Vector3(spawnPos.x, spawnPos.y, 0);
                    GameObject miniSlime = Instantiate(_miniSlimePrefab, worldPos, Quaternion.identity);
                    
                    // 미니 슬라임 스탯 조정
                    Slime miniSlimeComponent = miniSlime.GetComponent<Slime>();
                    if (miniSlimeComponent != null)
                    {
                        miniSlimeComponent._canSplit = false; // 미니 슬라임은 분열 불가
                        miniSlimeComponent._maxHealth = 5;
                        miniSlimeComponent._currentHealth = 5;
                        miniSlimeComponent._attackPower = 1;
                        miniSlimeComponent._expReward = 3;
                    }
                    
                    Debug.Log("Slime split into mini slimes!");
                }
            }
        }
        
        /// <summary>
        /// 슬라임 공격 - 산성 공격
        /// </summary>
        public override void Attack()
        {
            base.Attack();
            
            // 10% 확률로 산성 데미지 (방어력 무시)
            if (Random.Range(0f, 1f) < 0.1f)
            {
                Debug.Log("Slime's acid attack ignores defense!");
                // TODO: 방어력 무시 데미지 구현
            }
        }
        
        /// <summary>
        /// 아이템 드롭
        /// </summary>
        protected override void DropItems()
        {
            // 30% 확률로 젤리 드롭
            if (Random.Range(0f, 1f) < 0.3f)
            {
                Debug.Log("Slime dropped jelly!");
                // TODO: 아이템 드롭 시스템 연동
            }
        }
    }
}