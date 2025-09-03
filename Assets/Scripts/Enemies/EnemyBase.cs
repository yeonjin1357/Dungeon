using UnityEngine;
using Dungeon.Combat;
using Dungeon.Player;
using System.Collections;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 모든 적의 베이스 클래스
    /// </summary>
    [RequireComponent(typeof(GridPosition))]
    public abstract class EnemyBase : MonoBehaviour, ITurnBased
    {
        #region Fields
        [Header("Enemy Stats")]
        [SerializeField] protected int _maxHealth = 10;
        [SerializeField] protected int _attackPower = 2;
        [SerializeField] protected int _defense = 1;
        [SerializeField] protected float _moveSpeed = 0.3f;
        [SerializeField] protected int _expReward = 10;

        [Header("Detection")]
        [SerializeField] protected float _detectionRange = 5f;
        [SerializeField] protected float _attackRange = 1.5f;
        [SerializeField] protected LayerMask _obstacleLayer;

        protected EnemyStateMachine _stateMachine;
        protected int _currentHealth;
        protected bool _isActive = true;
        protected bool _isMoving = false;
        protected GridPosition _gridPosition;
        protected Transform _playerTransform;
        protected PlayerController _playerController;

        // 턴 관련
        protected TurnManager _turnManager;
        protected bool _hasTakenTurn = false;
        #endregion

        #region Properties
        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;
        public int AttackPower => _attackPower;
        public int Defense => _defense;
        public float MoveSpeed => _moveSpeed;
        public bool IsMoving => _isMoving;
        public GridPosition GridPositionComponent => _gridPosition;
        public float AttackRange => _attackRange;
        public float DetectionRange => _detectionRange;
        public Transform Player => _playerTransform;

        // ITurnBased 구현
        public bool IsActive => _isActive && _currentHealth > 0;
        public int TurnPriority => 10; // 적의 기본 우선순위
        public Vector2Int GridPosition => _gridPosition ? _gridPosition.CurrentPosition : Vector2Int.zero;
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            _gridPosition = GetComponent<GridPosition>();
            _currentHealth = _maxHealth;
            _stateMachine = new EnemyStateMachine(this);
            InitializeStates();
        }

        protected virtual void Start()
        {
            _turnManager = TurnManager.Instance;
            _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (_playerTransform != null)
            {
                _playerController = _playerTransform.GetComponent<PlayerController>();
            }

            // 턴 매니저에 등록
            if (_turnManager != null)
            {
                _turnManager.RegisterEntity(this);
            }

            // 초기 상태 설정
            _stateMachine.Initialize(EnemyState.Idle);
        }

        protected virtual void Update()
        {
            if (!IsActive) return;
            
            // 상태 머신 업데이트 (턴제가 아닌 실시간 로직용)
            _stateMachine.UpdateState();
        }

        protected virtual void OnDestroy()
        {
            // 턴 매니저에서 제거
            if (_turnManager != null)
            {
                _turnManager.UnregisterEntity(this);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // 감지 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);

            // 공격 범위 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
        #endregion

        #region ITurnBased Implementation
        public virtual void ExecuteTurn()
        {
            if (!IsActive) return;
            
            _hasTakenTurn = false;
            
            // 플레이어 감지
            if (DetectPlayer())
            {
                float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
                
                // 공격 범위 내에 있으면 공격
                if (distanceToPlayer <= _attackRange)
                {
                    _stateMachine.ChangeState(EnemyState.Attack);
                }
                // 추적
                else
                {
                    _stateMachine.ChangeState(EnemyState.Chase);
                }
            }
            else
            {
                // 플레이어를 감지하지 못하면 대기 또는 순찰
                if (_stateMachine.CurrentStateType != EnemyState.Patrol)
                {
                    _stateMachine.ChangeState(EnemyState.Idle);
                }
            }
        }

        public virtual void OnTurnStart()
        {
            _hasTakenTurn = false;
        }

        public virtual void OnTurnEnd()
        {
            _hasTakenTurn = true;
        }
        #endregion

        #region State Machine
        /// <summary>
        /// 상태 초기화 - 파생 클래스에서 구현
        /// </summary>
        protected abstract void InitializeStates();

        /// <summary>
        /// 상태 변경
        /// </summary>
        public void ChangeState(EnemyState newState)
        {
            _stateMachine.ChangeState(newState);
        }
        #endregion

        #region Detection
        /// <summary>
        /// 플레이어 감지
        /// </summary>
        public virtual bool DetectPlayer()
        {
            if (_playerTransform == null) return false;

            float distance = Vector2.Distance(transform.position, _playerTransform.position);
            
            // 거리 체크
            if (distance > _detectionRange) return false;

            // 시야 체크 (벽에 막혀있는지)
            Vector2 direction = (_playerTransform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, _obstacleLayer);
            
            // 장애물이 없으면 플레이어 감지
            return hit.collider == null;
        }

        /// <summary>
        /// 플레이어와의 거리 계산
        /// </summary>
        public float GetDistanceToPlayer()
        {
            if (_playerTransform == null) return float.MaxValue;
            return Vector2.Distance(transform.position, _playerTransform.position);
        }
        #endregion

        #region Movement
        /// <summary>
        /// 그리드 이동
        /// </summary>
        public virtual void MoveToGrid(Vector2Int targetPosition)
        {
            if (_isMoving) return;
            StartCoroutine(MoveCoroutine(targetPosition));
        }

        protected IEnumerator MoveCoroutine(Vector2Int targetPosition)
        {
            _isMoving = true;
            
            Vector3 startPos = transform.position;
            Vector3 endPos = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            
            float elapsed = 0f;
            while (elapsed < _moveSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            transform.position = endPos;
            _gridPosition.SetGridPosition(targetPosition);
            _isMoving = false;
        }
        #endregion

        #region Combat
        /// <summary>
        /// 데미지 받기
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - _defense);
            _currentHealth -= actualDamage;
            
            Debug.Log($"{name} took {actualDamage} damage. Health: {_currentHealth}/{_maxHealth}");
            
            // 피격 효과
            OnDamaged(actualDamage);
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 공격하기
        /// </summary>
        public virtual void Attack()
        {
            if (_playerController != null)
            {
                _playerController.TakeDamage(_attackPower);
            }
        }

        /// <summary>
        /// 피격 시 효과
        /// </summary>
        protected virtual void OnDamaged(int damage)
        {
            // 피격 애니메이션, 사운드 등
            // 파생 클래스에서 구현
        }

        /// <summary>
        /// 죽음 처리
        /// </summary>
        protected virtual void Die()
        {
            _isActive = false;
            _stateMachine.ChangeState(EnemyState.Dead);
            
            // 경험치 제공
            if (_playerController != null)
            {
                // _playerController.GainExperience(_expReward);
            }
            
            // 아이템 드롭
            DropItems();
            
            // 오브젝트 제거
            StartCoroutine(DeathSequence());
        }

        protected virtual IEnumerator DeathSequence()
        {
            // 죽음 애니메이션 재생
            yield return new WaitForSeconds(0.5f);
            
            // 턴 매니저에서 제거
            if (_turnManager != null)
            {
                _turnManager.UnregisterEntity(this);
            }
            
            Destroy(gameObject);
        }

        /// <summary>
        /// 아이템 드롭
        /// </summary>
        protected virtual void DropItems()
        {
            // 파생 클래스에서 구현
        }
        #endregion

        #region Utility
        /// <summary>
        /// 플레이어 방향으로 한 칸 이동
        /// </summary>
        public Vector2Int GetNextPositionTowardsPlayer()
        {
            if (_playerTransform == null) return GridPosition;
            
            Vector2Int playerPos = new Vector2Int(
                Mathf.RoundToInt(_playerTransform.position.x),
                Mathf.RoundToInt(_playerTransform.position.y)
            );
            
            Vector2Int currentPos = GridPosition;
            Vector2Int direction = playerPos - currentPos;
            
            // 대각선 이동 방지 - 한 축씩만 이동
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                direction.x = Mathf.Clamp(direction.x, -1, 1);
                direction.y = 0;
            }
            else
            {
                direction.x = 0;
                direction.y = Mathf.Clamp(direction.y, -1, 1);
            }
            
            return currentPos + direction;
        }

        /// <summary>
        /// 위치가 이동 가능한지 체크
        /// </summary>
        public bool CanMoveTo(Vector2Int position)
        {
            // 벽 체크
            Collider2D hit = Physics2D.OverlapPoint(position, _obstacleLayer);
            if (hit != null) return false;
            
            // 다른 적 체크
            Collider2D[] enemies = Physics2D.OverlapPointAll(position);
            foreach (var col in enemies)
            {
                if (col.gameObject != gameObject && col.GetComponent<EnemyBase>() != null)
                {
                    return false;
                }
            }
            
            return true;
        }
        #endregion
    }
}