using UnityEngine;
using System.Collections.Generic;
using Dungeon.Items;

namespace Dungeon.Player
{
    /// <summary>
    /// 플레이어의 스탯을 관리하는 클래스
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        #region Fields
        
        [Header("기본 정보")]
        [SerializeField] private int _level = 1;
        [SerializeField] private int _experience = 0;
        [SerializeField] private int _experienceToNextLevel = 100;
        
        [Header("체력/마나")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth = 100;
        [SerializeField] private int _maxMana = 50;
        [SerializeField] private int _currentMana = 50;
        
        [Header("전투 스탯")]
        [SerializeField] private int _baseAttackPower = 10;
        [SerializeField] private int _baseDefense = 5;
        [SerializeField] private int _baseMagicPower = 5;
        [SerializeField] private int _baseMagicResistance = 2;
        
        [Header("크리티컬/회피")]
        [SerializeField] private float _criticalChance = 5f; // %
        [SerializeField] private float _criticalDamageMultiplier = 150f; // %
        [SerializeField] private float _dodgeChance = 5f; // %
        [SerializeField] private float _blockChance = 0f; // %
        
        [Header("기타 스탯")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _attackSpeed = 1f;
        [SerializeField] private float _healthRegen = 0f;
        [SerializeField] private float _manaRegen = 0f;
        
        // 스탯 수정치를 저장하는 딕셔너리 (장비, 버프 등으로 인한 추가 스탯)
        private Dictionary<StatType, float> _statModifiers = new Dictionary<StatType, float>();
        
        // 임시 버프를 저장하는 딕셔너리
        private Dictionary<StatType, float> _temporaryBuffs = new Dictionary<StatType, float>();
        
        #endregion
        
        #region Properties
        
        public int Level => _level;
        public int Experience => _experience;
        public int ExperienceToNextLevel => _experienceToNextLevel;
        
        public int MaxHealth => _maxHealth + GetStatModifier(StatType.MaxHealth);
        public int CurrentHealth => _currentHealth;
        public int MaxMana => _maxMana + GetStatModifier(StatType.MaxMana);
        public int CurrentMana => _currentMana;
        
        public int AttackPower => _baseAttackPower + GetStatModifier(StatType.AttackPower);
        public int Defense => _baseDefense + GetStatModifier(StatType.Defense);
        public int MagicPower => _baseMagicPower + GetStatModifier(StatType.MagicPower);
        public int MagicResistance => _baseMagicResistance + GetStatModifier(StatType.MagicResistance);
        
        public float CriticalChance => _criticalChance + GetStatModifier(StatType.CriticalChance);
        public float CriticalDamageMultiplier => _criticalDamageMultiplier + GetStatModifier(StatType.CriticalDamage);
        public float DodgeChance => _dodgeChance + GetStatModifier(StatType.DodgeChance);
        public float BlockChance => _blockChance + GetStatModifier(StatType.BlockChance);
        
        public float MoveSpeed => _moveSpeed + GetStatModifier(StatType.MoveSpeed);
        public float AttackSpeed => _attackSpeed + GetStatModifier(StatType.AttackSpeed);
        public float HealthRegen => _healthRegen + GetStatModifier(StatType.HealthRegen);
        public float ManaRegen => _manaRegen + GetStatModifier(StatType.ManaRegen);
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeStatModifiers();
        }
        
        private void Start()
        {
            // 체력/마나를 최대치로 설정
            _currentHealth = MaxHealth;
            _currentMana = MaxMana;
        }
        
        private void Update()
        {
            // 체력/마나 재생
            if (_healthRegen > 0 && _currentHealth < MaxHealth)
            {
                RegenerateHealth(_healthRegen * Time.deltaTime);
            }
            
            if (_manaRegen > 0 && _currentMana < MaxMana)
            {
                RegenerateMana(_manaRegen * Time.deltaTime);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 스탯 수정치를 적용합니다 (장비, 버프 등)
        /// </summary>
        public void ModifyStat(StatType statType, float value)
        {
            if (!_statModifiers.ContainsKey(statType))
            {
                _statModifiers[statType] = 0;
            }
            
            _statModifiers[statType] += value;
            
            Debug.Log($"{statType} 스탯이 {value}만큼 변경되었습니다. 현재 수정치: {_statModifiers[statType]}");
            
            // 체력/마나 최대치가 변경되면 현재값도 조정
            if (statType == StatType.MaxHealth)
            {
                _currentHealth = Mathf.Min(_currentHealth, MaxHealth);
            }
            else if (statType == StatType.MaxMana)
            {
                _currentMana = Mathf.Min(_currentMana, MaxMana);
            }
        }
        
        /// <summary>
        /// 임시 버프를 적용합니다
        /// </summary>
        public void ApplyTemporaryBuff(StatType statType, float value, float duration)
        {
            if (!_temporaryBuffs.ContainsKey(statType))
            {
                _temporaryBuffs[statType] = 0;
            }
            
            _temporaryBuffs[statType] += value;
            
            // 지정된 시간 후 버프 제거
            StartCoroutine(RemoveBuffAfterDuration(statType, value, duration));
        }
        
        /// <summary>
        /// 데미지를 받습니다
        /// </summary>
        public void TakeDamage(int damage)
        {
            // 회피 체크
            if (Random.Range(0f, 100f) < DodgeChance)
            {
                Debug.Log("공격을 회피했습니다!");
                return;
            }
            
            // 방어 체크
            if (Random.Range(0f, 100f) < BlockChance)
            {
                Debug.Log("공격을 방어했습니다!");
                damage = Mathf.RoundToInt(damage * 0.5f); // 방어 시 데미지 절반
            }
            
            // 방어력 적용
            int finalDamage = Mathf.Max(1, damage - Defense);
            
            // 데미지 감소 적용
            float damageReduction = GetStatModifier(StatType.DamageReduction);
            if (damageReduction > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * (1f - damageReduction / 100f));
            }
            
            _currentHealth = Mathf.Max(0, _currentHealth - finalDamage);
            
            Debug.Log($"플레이어가 {finalDamage}의 데미지를 받았습니다. 남은 체력: {_currentHealth}/{MaxHealth}");
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// 체력을 회복합니다
        /// </summary>
        public void Heal(int amount)
        {
            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
            Debug.Log($"체력을 {amount} 회복했습니다. 현재 체력: {_currentHealth}/{MaxHealth}");
        }
        
        /// <summary>
        /// 마나를 소비합니다
        /// </summary>
        public bool ConsumeMana(int amount)
        {
            if (_currentMana >= amount)
            {
                _currentMana -= amount;
                Debug.Log($"마나를 {amount} 소비했습니다. 남은 마나: {_currentMana}/{MaxMana}");
                return true;
            }
            
            Debug.Log("마나가 부족합니다!");
            return false;
        }
        
        /// <summary>
        /// 마나를 회복합니다
        /// </summary>
        public void RestoreMana(int amount)
        {
            _currentMana = Mathf.Min(MaxMana, _currentMana + amount);
            Debug.Log($"마나를 {amount} 회복했습니다. 현재 마나: {_currentMana}/{MaxMana}");
        }
        
        /// <summary>
        /// 경험치를 획득합니다
        /// </summary>
        public void GainExperience(int amount)
        {
            _experience += amount;
            Debug.Log($"경험치를 {amount} 획득했습니다. ({_experience}/{_experienceToNextLevel})");
            
            // 레벨업 체크
            while (_experience >= _experienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        /// <summary>
        /// 레벨업을 처리합니다
        /// </summary>
        private void LevelUp()
        {
            _experience -= _experienceToNextLevel;
            _level++;
            _experienceToNextLevel = CalculateExperienceToNextLevel(_level);
            
            // 레벨업 시 스탯 증가
            _maxHealth += 10;
            _maxMana += 5;
            _baseAttackPower += 2;
            _baseDefense += 1;
            
            // 체력/마나 완전 회복
            _currentHealth = MaxHealth;
            _currentMana = MaxMana;
            
            Debug.Log($"레벨업! 현재 레벨: {_level}");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 스탯 수정치 딕셔너리를 초기화합니다
        /// </summary>
        private void InitializeStatModifiers()
        {
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                _statModifiers[statType] = 0f;
                _temporaryBuffs[statType] = 0f;
            }
        }
        
        /// <summary>
        /// 특정 스탯의 총 수정치를 반환합니다
        /// </summary>
        private int GetStatModifier(StatType statType)
        {
            float modifier = 0f;
            
            if (_statModifiers.ContainsKey(statType))
            {
                modifier += _statModifiers[statType];
            }
            
            if (_temporaryBuffs.ContainsKey(statType))
            {
                modifier += _temporaryBuffs[statType];
            }
            
            return Mathf.RoundToInt(modifier);
        }
        
        /// <summary>
        /// 체력을 서서히 재생합니다
        /// </summary>
        private void RegenerateHealth(float amount)
        {
            float newHealth = _currentHealth + amount;
            _currentHealth = Mathf.Min(Mathf.RoundToInt(newHealth), MaxHealth);
        }
        
        /// <summary>
        /// 마나를 서서히 재생합니다
        /// </summary>
        private void RegenerateMana(float amount)
        {
            float newMana = _currentMana + amount;
            _currentMana = Mathf.Min(Mathf.RoundToInt(newMana), MaxMana);
        }
        
        /// <summary>
        /// 다음 레벨까지 필요한 경험치를 계산합니다
        /// </summary>
        private int CalculateExperienceToNextLevel(int level)
        {
            // 레벨이 올라갈수록 더 많은 경험치 필요
            return 100 * level + 50 * (level - 1) * (level - 1);
        }
        
        /// <summary>
        /// 플레이어 사망을 처리합니다
        /// </summary>
        private void Die()
        {
            Debug.Log("플레이어가 사망했습니다!");
            
            // GameManager에 게임오버 알림
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.GameOver();
            }
            
            // 사망 애니메이션 재생 (있다면)
            var animator = GetComponent<PlayerAnimator>();
            if (animator != null)
            {
                animator.PlayDeathAnimation();
            }
        }
        
        /// <summary>
        /// 일정 시간 후 버프를 제거합니다
        /// </summary>
        private System.Collections.IEnumerator RemoveBuffAfterDuration(StatType statType, float value, float duration)
        {
            yield return new WaitForSeconds(duration);
            
            if (_temporaryBuffs.ContainsKey(statType))
            {
                _temporaryBuffs[statType] -= value;
                Debug.Log($"{statType} 버프가 종료되었습니다.");
            }
        }
        
        #endregion
        
        #region Save/Load
        
        /// <summary>
        /// 스탯 데이터를 저장합니다
        /// </summary>
        public PlayerStatsSaveData GetSaveData()
        {
            return new PlayerStatsSaveData
            {
                level = _level,
                experience = _experience,
                currentHealth = _currentHealth,
                currentMana = _currentMana,
                baseAttackPower = _baseAttackPower,
                baseDefense = _baseDefense,
                baseMagicPower = _baseMagicPower,
                baseMagicResistance = _baseMagicResistance
            };
        }
        
        /// <summary>
        /// 스탯 데이터를 불러옵니다
        /// </summary>
        public void LoadSaveData(PlayerStatsSaveData data)
        {
            _level = data.level;
            _experience = data.experience;
            _currentHealth = data.currentHealth;
            _currentMana = data.currentMana;
            _baseAttackPower = data.baseAttackPower;
            _baseDefense = data.baseDefense;
            _baseMagicPower = data.baseMagicPower;
            _baseMagicResistance = data.baseMagicResistance;
            
            _experienceToNextLevel = CalculateExperienceToNextLevel(_level);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 플레이어 스탯 저장 데이터
    /// </summary>
    [System.Serializable]
    public class PlayerStatsSaveData
    {
        public int level;
        public int experience;
        public int currentHealth;
        public int currentMana;
        public int baseAttackPower;
        public int baseDefense;
        public int baseMagicPower;
        public int baseMagicResistance;
    }
}