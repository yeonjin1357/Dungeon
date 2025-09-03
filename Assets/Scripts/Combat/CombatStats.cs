using UnityEngine;

namespace Dungeon.Combat
{
    public class CombatStats : MonoBehaviour
    {
        #region Fields
        [Header("Health")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth;
        
        [Header("Combat Stats")]
        [SerializeField] private int _attackPower = 10;
        [SerializeField] private int _defense = 5;
        [SerializeField] private int _attackRange = 1;
        
        [Header("Advanced Stats")]
        [SerializeField] private int _accuracy = 90;
        [SerializeField] private int _evasion = 10;
        [SerializeField] private int _criticalChance = 10;
        [SerializeField] private int _speed = 10;
        
        [Header("Level & Experience")]
        [SerializeField] private int _level = 1;
        [SerializeField] private int _experience = 0;
        [SerializeField] private int _experienceToNextLevel = 100;
        
        public delegate void HealthEventDelegate(int currentHealth, int maxHealth);
        public event HealthEventDelegate OnHealthChanged;
        public event HealthEventDelegate OnDeath;
        
        public delegate void StatsEventDelegate();
        public event StatsEventDelegate OnStatsChanged;
        public event StatsEventDelegate OnLevelUp;
        #endregion
        
        #region Properties
        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;
        public int AttackPower => _attackPower;
        public int Defense => _defense;
        public int AttackRange => _attackRange;
        public int Accuracy => _accuracy;
        public int Evasion => _evasion;
        public int CriticalChance => _criticalChance;
        public int Speed => _speed;
        public int Level => _level;
        public int Experience => _experience;
        public int ExperienceToNextLevel => _experienceToNextLevel;
        
        public bool IsDead => _currentHealth <= 0;
        public float HealthPercentage => (float)_currentHealth / _maxHealth;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            InitializeStats();
        }
        
        private void Start()
        {
            // Full heal on start
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        #endregion
        
        #region Public Methods
        public void InitializeStats()
        {
            _currentHealth = _maxHealth;
            CalculateExperienceToNextLevel();
        }
        
        public void TakeDamage(int damage)
        {
            if (IsDead)
                return;
            
            damage = Mathf.Max(0, damage);
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            
            Debug.Log($"{gameObject.name} took {damage} damage. Health: {_currentHealth}/{_maxHealth}");
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void Heal(int amount)
        {
            if (IsDead)
                return;
            
            amount = Mathf.Max(0, amount);
            int previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            
            int actualHealing = _currentHealth - previousHealth;
            Debug.Log($"{gameObject.name} healed for {actualHealing}. Health: {_currentHealth}/{_maxHealth}");
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        public void FullHeal()
        {
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        public void ModifyAttackPower(int amount)
        {
            _attackPower = Mathf.Max(1, _attackPower + amount);
            OnStatsChanged?.Invoke();
        }
        
        public void ModifyDefense(int amount)
        {
            _defense = Mathf.Max(0, _defense + amount);
            OnStatsChanged?.Invoke();
        }
        
        public void ModifyMaxHealth(int amount)
        {
            _maxHealth = Mathf.Max(1, _maxHealth + amount);
            
            // Heal proportionally
            if (amount > 0)
            {
                _currentHealth += amount;
            }
            
            _currentHealth = Mathf.Clamp(_currentHealth, 1, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnStatsChanged?.Invoke();
        }
        
        public void GainExperience(int amount)
        {
            if (amount <= 0)
                return;
            
            _experience += amount;
            Debug.Log($"{gameObject.name} gained {amount} experience. Total: {_experience}/{_experienceToNextLevel}");
            
            // Check for level up
            while (_experience >= _experienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        public void SetStats(int maxHealth, int attackPower, int defense, int attackRange = 1)
        {
            _maxHealth = Mathf.Max(1, maxHealth);
            _currentHealth = _maxHealth;
            _attackPower = Mathf.Max(1, attackPower);
            _defense = Mathf.Max(0, defense);
            _attackRange = Mathf.Max(1, attackRange);
            
            OnStatsChanged?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        public void SetAdvancedStats(int accuracy, int evasion, int criticalChance, int speed)
        {
            _accuracy = Mathf.Clamp(accuracy, 0, 100);
            _evasion = Mathf.Clamp(evasion, 0, 100);
            _criticalChance = Mathf.Clamp(criticalChance, 0, 100);
            _speed = Mathf.Max(1, speed);
            
            OnStatsChanged?.Invoke();
        }
        #endregion
        
        #region Private Methods
        private void Die()
        {
            Debug.Log($"{gameObject.name} has died!");
            OnDeath?.Invoke(_currentHealth, _maxHealth);
            
            // Disable combat participation
            enabled = false;
        }
        
        private void LevelUp()
        {
            _experience -= _experienceToNextLevel;
            _level++;
            
            // Increase stats on level up
            int healthIncrease = 10 + (_level * 2);
            int attackIncrease = 2;
            int defenseIncrease = 1;
            
            _maxHealth += healthIncrease;
            _currentHealth += healthIncrease; // Heal on level up
            _attackPower += attackIncrease;
            _defense += defenseIncrease;
            
            // Small boost to advanced stats
            _accuracy = Mathf.Min(100, _accuracy + 1);
            _criticalChance = Mathf.Min(50, _criticalChance + 1);
            _speed += 1;
            
            CalculateExperienceToNextLevel();
            
            Debug.Log($"{gameObject.name} leveled up to {_level}!");
            
            OnLevelUp?.Invoke();
            OnStatsChanged?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        private void CalculateExperienceToNextLevel()
        {
            // Simple exponential formula
            _experienceToNextLevel = 100 * _level * (_level + 1) / 2;
        }
        #endregion
    }
}