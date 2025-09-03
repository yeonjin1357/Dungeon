using UnityEngine;
using System.Collections;

namespace Dungeon.Combat
{
    public class CombatSystem : MonoBehaviour
    {
        #region Singleton
        private static CombatSystem _instance;
        public static CombatSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CombatSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CombatSystem");
                        _instance = go.AddComponent<CombatSystem>();
                    }
                }
                return _instance;
            }
        }
        #endregion
        
        #region Fields
        [Header("Combat Settings")]
        [SerializeField] private float _attackAnimationTime = 0.3f;
        [SerializeField] private float _damageNumberDuration = 1f;
        [SerializeField] private bool _showDamageNumbers = true;
        
        [Header("Damage Formula")]
        [SerializeField] private float _criticalMultiplier = 1.5f;
        [SerializeField] private float _baseCriticalChance = 0.1f;
        [SerializeField] private float _baseHitChance = 0.9f;
        
        [Header("UI References")]
        [SerializeField] private GameObject _damageNumberPrefab;
        [SerializeField] private Transform _damageNumberContainer;
        
        public delegate void CombatEventDelegate(GameObject attacker, GameObject target, int damage);
        public event CombatEventDelegate OnAttack;
        public event CombatEventDelegate OnDamageDealt;
        public event CombatEventDelegate OnEntityDeath;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        #endregion
        
        #region Public Methods
        public void PerformAttack(GameObject attacker, GameObject target)
        {
            if (attacker == null || target == null)
            {
                Debug.LogWarning("Cannot perform attack: Attacker or target is null");
                return;
            }
            
            CombatStats attackerStats = attacker.GetComponent<CombatStats>();
            CombatStats targetStats = target.GetComponent<CombatStats>();
            
            if (attackerStats == null || targetStats == null)
            {
                Debug.LogWarning("Cannot perform attack: Missing CombatStats component");
                return;
            }
            
            StartCoroutine(AttackCoroutine(attacker, target, attackerStats, targetStats));
        }
        
        public int CalculateDamage(CombatStats attacker, CombatStats defender)
        {
            return CalculateDamage(attacker, defender, out bool wasCritical);
        }
        
        public int CalculateDamage(CombatStats attacker, CombatStats defender, out bool wasCritical)
        {
            wasCritical = false;
            
            // Check hit chance
            float hitChance = CalculateHitChance(attacker, defender);
            if (Random.Range(0f, 1f) > hitChance)
            {
                Debug.Log("Attack missed!");
                return 0;
            }
            
            // Calculate base damage
            int baseDamage = attacker.AttackPower - defender.Defense;
            baseDamage = Mathf.Max(1, baseDamage); // Minimum 1 damage
            
            // Check for critical hit
            float critChance = CalculateCriticalChance(attacker);
            bool isCritical = Random.Range(0f, 1f) < critChance;
            
            if (isCritical)
            {
                baseDamage = Mathf.RoundToInt(baseDamage * _criticalMultiplier);
                wasCritical = true;
                Debug.Log("Critical hit!");
            }
            
            // Apply random variance (±10%)
            float variance = Random.Range(0.9f, 1.1f);
            int finalDamage = Mathf.RoundToInt(baseDamage * variance);
            
            return finalDamage;
        }
        
        public void DealDamage(GameObject target, int damage)
        {
            DealDamage(null, target, damage, false);
        }
        
        public void DealDamage(GameObject attacker, GameObject target, int damage)
        {
            DealDamage(attacker, target, damage, false);
        }
        
        public void DealDamage(GameObject attacker, GameObject target, int damage, bool isCritical)
        {
            if (target == null)
                return;
            
            CombatStats targetStats = target.GetComponent<CombatStats>();
            if (targetStats == null)
                return;
            
            targetStats.TakeDamage(damage);
            
            // Show damage number
            if (_showDamageNumbers)
            {
                ShowDamageNumber(target.transform.position, damage, isCritical);
            }
            
            OnDamageDealt?.Invoke(attacker, target, damage);
            
            // Check if target died
            if (targetStats.CurrentHealth <= 0)
            {
                HandleDeath(target);
            }
        }
        
        public bool IsInRange(Vector2Int attackerPos, Vector2Int targetPos, int range)
        {
            int distance = Mathf.Abs(attackerPos.x - targetPos.x) + Mathf.Abs(attackerPos.y - targetPos.y);
            return distance <= range;
        }
        
        public bool CanAttack(GameObject attacker, GameObject target)
        {
            if (attacker == null || target == null)
                return false;
            
            CombatStats attackerStats = attacker.GetComponent<CombatStats>();
            CombatStats targetStats = target.GetComponent<CombatStats>();
            
            if (attackerStats == null || targetStats == null)
                return false;
            
            if (targetStats.CurrentHealth <= 0)
                return false;
            
            // Check range
            Vector2Int attackerPos = GetGridPosition(attacker);
            Vector2Int targetPos = GetGridPosition(target);
            
            return IsInRange(attackerPos, targetPos, attackerStats.AttackRange);
        }
        #endregion
        
        #region Private Methods
        private IEnumerator AttackCoroutine(GameObject attacker, GameObject target, CombatStats attackerStats, CombatStats targetStats)
        {
            OnAttack?.Invoke(attacker, target, 0);
            
            // Play attack animation
            var attackerAnimator = attacker.GetComponent<Player.PlayerAnimator>();
            if (attackerAnimator != null)
            {
                Vector2 direction = (target.transform.position - attacker.transform.position).normalized;
                attackerAnimator.PlayAttackAnimation(direction);
            }
            
            yield return new WaitForSeconds(_attackAnimationTime);
            
            // Calculate and deal damage
            int damage = CalculateDamage(attackerStats, targetStats, out bool wasCritical);
            
            if (damage > 0)
            {
                DealDamage(attacker, target, damage, wasCritical);
                
                // Play hurt animation on target
                var targetAnimator = target.GetComponent<Player.PlayerAnimator>();
                if (targetAnimator != null)
                {
                    targetAnimator.PlayHurtAnimation();
                }
            }
            else
            {
                ShowMissText(target.transform.position);
            }
        }
        
        private float CalculateHitChance(CombatStats attacker, CombatStats defender)
        {
            float hitChance = _baseHitChance;
            
            // Modify based on accuracy and evasion
            hitChance += attacker.Accuracy * 0.01f;
            hitChance -= defender.Evasion * 0.01f;
            
            return Mathf.Clamp01(hitChance);
        }
        
        private float CalculateCriticalChance(CombatStats attacker)
        {
            float critChance = _baseCriticalChance;
            critChance += attacker.CriticalChance * 0.01f;
            
            return Mathf.Clamp01(critChance);
        }
        
        private void HandleDeath(GameObject entity)
        {
            Debug.Log($"{entity.name} has died!");
            
            OnEntityDeath?.Invoke(null, entity, 0);
            
            // Play death animation
            var animator = entity.GetComponent<Player.PlayerAnimator>();
            if (animator != null)
            {
                animator.PlayDeathAnimation();
            }
            
            // Remove from turn system
            ITurnBased turnBased = entity.GetComponent<ITurnBased>();
            if (turnBased != null)
            {
                TurnManager.Instance.UnregisterEntity(turnBased);
            }
            
            // Destroy after delay
            Destroy(entity, 2f);
        }
        
        private Vector2Int GetGridPosition(GameObject entity)
        {
            var gridPos = entity.GetComponent<Player.GridPosition>();
            if (gridPos != null)
            {
                return gridPos.CurrentPosition;
            }
            
            // Fallback to world position
            Vector3 worldPos = entity.transform.position;
            return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        }
        
        private void ShowDamageNumber(Vector3 position, int damage, bool isCritical = false)
        {
            if (_damageNumberPrefab == null)
            {
                Debug.LogWarning("Damage number prefab is not assigned!");
                return;
            }
            
            // Create damage number UI
            GameObject damageNumberObj = Instantiate(_damageNumberPrefab, _damageNumberContainer);
            UI.DamageNumberUI damageNumberUI = damageNumberObj.GetComponent<UI.DamageNumberUI>();
            
            if (damageNumberUI != null)
            {
                // Initialize with damage value and duration
                damageNumberUI.Initialize(damage, _damageNumberDuration, isCritical);
                damageNumberUI.SetWorldPosition(position + Vector3.up * 0.5f); // Offset slightly above target
            }
            else
            {
                Debug.LogWarning("DamageNumberUI component not found on prefab!");
                Destroy(damageNumberObj);
            }
        }
        
        private void ShowMissText(Vector3 position)
        {
            Debug.Log($"Miss! at position {position}");
            
            // TODO: Implement miss text UI
        }
        #endregion
    }
}