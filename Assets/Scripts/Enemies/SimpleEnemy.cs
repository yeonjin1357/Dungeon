using UnityEngine;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 테스트용 간단한 적 구현
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleEnemy : EnemyBase
    {
        [Header("Simple Enemy Settings")]
        [SerializeField] private Color _enemyColor = Color.red;
        
        // 캐싱된 WaitForSeconds
        private readonly WaitForSeconds _flashDuration = new WaitForSeconds(0.1f);
        
        protected override void Awake()
        {
            base.Awake();
            
            // 스프라이트 색상 설정
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = _enemyColor;
            }
        }

        /// <summary>
        /// 상태 초기화
        /// </summary>
        protected override void InitializeStates()
        {
            // 각 상태 인스턴스 생성 및 등록
            _stateMachine.RegisterState(EnemyState.Idle, new IdleState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyState.Patrol, new PatrolState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyState.Chase, new ChaseState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyState.Attack, new AttackState(this, _stateMachine));
        }

        /// <summary>
        /// 피격 시 효과 - 색상 변경
        /// </summary>
        protected override void OnDamaged(int damage)
        {
            base.OnDamaged(damage);
            
            // 피격 시 잠시 흰색으로 변경
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashWhite(spriteRenderer));
            }
        }

        private System.Collections.IEnumerator FlashWhite(SpriteRenderer spriteRenderer)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return _flashDuration;
            spriteRenderer.color = originalColor;
        }

        /// <summary>
        /// 아이템 드롭 - 확률적으로 아이템 드롭
        /// </summary>
        protected override void DropItems()
        {
            // 20% 확률로 아이템 드롭 (나중에 구현)
            if (Random.Range(0f, 1f) < 0.2f)
            {
                Debug.Log($"{name} dropped an item!");
                // TODO: 실제 아이템 드롭 구현
            }
        }
    }
}