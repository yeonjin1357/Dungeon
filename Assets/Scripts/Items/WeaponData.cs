using UnityEngine;
using System.Collections.Generic;
using Dungeon.Items.Interfaces;
using Dungeon.Player;

namespace Dungeon.Items
{
    /// <summary>
    /// 무기 아이템 데이터를 저장하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Dungeon/Items/Weapon", order = 1)]
    public class WeaponData : ItemData, IEquippable
    {
        [Header("무기 기본 정보")]
        [SerializeField] private WeaponType _weaponType = WeaponType.OneHandedSword;
        [SerializeField] private DamageType _damageType = DamageType.Physical;
        
        [Header("무기 스탯")]
        [SerializeField] private int _baseAttackPower = 10;
        [SerializeField] private float _attackSpeed = 1.0f; // 초당 공격 횟수
        [SerializeField] private float _attackRange = 1.0f; // 타일 단위
        
        [Header("크리티컬 속성")]
        [SerializeField] [Range(0, 100)] private float _criticalChance = 5f; // %
        [SerializeField] [Range(100, 500)] private float _criticalDamageMultiplier = 150f; // %
        
        [Header("추가 스탯 보너스")]
        [SerializeField] private List<StatBonus> _statBonuses = new List<StatBonus>();
        
        [Header("특수 효과")]
        [SerializeField] private bool _hasPiercing = false; // 관통 공격
        [SerializeField] private bool _hasLifeSteal = false; // 생명력 흡수
        [SerializeField] [Range(0, 50)] private float _lifeStealPercent = 0f; // %
        
        #region Properties
        
        public WeaponType WeaponType => _weaponType;
        public DamageType DamageType => _damageType;
        public int BaseAttackPower => _baseAttackPower;
        public float AttackSpeed => _attackSpeed;
        public float AttackRange => _attackRange;
        public float CriticalChance => _criticalChance;
        public float CriticalDamageMultiplier => _criticalDamageMultiplier;
        public bool HasPiercing => _hasPiercing;
        public bool HasLifeSteal => _hasLifeSteal;
        public float LifeStealPercent => _lifeStealPercent;
        
        #endregion
        
        #region IEquippable Implementation
        
        public EquipmentSlot SlotType => EquipmentSlot.Weapon;
        
        // IEquippable 인터페이스에서 요구하는 속성들 (ItemData에서 상속)
        public new int RequiredLevel => base.RequiredLevel;
        public new string RequiredClass => base.RequiredClass;
        
        public bool CanEquip(GameObject wearer)
        {
            if (wearer == null) return false;
            
            // 레벨 체크 (PlayerStats 컴포넌트가 있다고 가정)
            var stats = wearer.GetComponent<PlayerStats>();
            if (stats != null && stats.Level < RequiredLevel)
            {
                Debug.Log($"레벨이 부족합니다. 필요 레벨: {RequiredLevel}");
                return false;
            }
            
            // 클래스 체크 (필요시)
            if (!string.IsNullOrEmpty(RequiredClass))
            {
                // 클래스 체크 로직 (추후 구현)
                // var playerClass = wearer.GetComponent<PlayerClass>();
                // if (playerClass != null && playerClass.ClassName != RequiredClass)
                //     return false;
            }
            
            return true;
        }
        
        public void Equip(GameObject wearer)
        {
            if (!CanEquip(wearer)) return;
            
            Debug.Log($"{ItemName} 무기를 장착했습니다!");
            
            // 스탯 적용
            var stats = wearer.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // 기본 공격력 적용
                stats.ModifyStat(StatType.AttackPower, _baseAttackPower);
                
                // 추가 스탯 보너스 적용
                foreach (var bonus in _statBonuses)
                {
                    stats.ModifyStat(bonus.statType, bonus.value);
                }
            }
            
            // 무기 모델 변경 (있다면)
            // var weaponHolder = wearer.GetComponentInChildren<WeaponHolder>();
            // if (weaponHolder != null && WorldPrefab != null)
            // {
            //     weaponHolder.SetWeapon(WorldPrefab);
            // }
        }
        
        public void Unequip(GameObject wearer)
        {
            Debug.Log($"{ItemName} 무기를 해제했습니다!");
            
            // 스탯 제거
            var stats = wearer.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // 기본 공격력 제거
                stats.ModifyStat(StatType.AttackPower, -_baseAttackPower);
                
                // 추가 스탯 보너스 제거
                foreach (var bonus in _statBonuses)
                {
                    stats.ModifyStat(bonus.statType, -bonus.value);
                }
            }
            
            // 무기 모델 제거 (있다면)
            // var weaponHolder = wearer.GetComponentInChildren<WeaponHolder>();
            // if (weaponHolder != null)
            // {
            //     weaponHolder.RemoveWeapon();
            // }
        }
        
        public Dictionary<string, float> GetStatBonuses()
        {
            var bonuses = new Dictionary<string, float>();
            
            // 기본 공격력
            bonuses["AttackPower"] = _baseAttackPower;
            bonuses["AttackSpeed"] = _attackSpeed;
            bonuses["CriticalChance"] = _criticalChance;
            bonuses["CriticalDamage"] = _criticalDamageMultiplier;
            
            // 추가 스탯 보너스
            foreach (var bonus in _statBonuses)
            {
                var key = bonus.statType.ToString();
                if (bonuses.ContainsKey(key))
                {
                    bonuses[key] += bonus.value;
                }
                else
                {
                    bonuses[key] = bonus.value;
                }
            }
            
            return bonuses;
        }
        
        #endregion
        
        #region Override Methods
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // 무기는 스택 불가능
            if (IsStackable)
            {
                Debug.LogWarning("무기는 스택할 수 없습니다. IsStackable을 false로 설정합니다.");
                // 에디터에서만 작동하는 직접 수정은 피하고 경고만 표시
            }
            
            // 공격력 최소값 보정
            if (_baseAttackPower < 1)
                _baseAttackPower = 1;
            
            // 공격 속도 최소값 보정
            if (_attackSpeed < 0.1f)
                _attackSpeed = 0.1f;
            
            // 공격 범위 최소값 보정
            if (_attackRange < 1.0f)
                _attackRange = 1.0f;
            
            // 생명력 흡수 설정 검증
            if (_hasLifeSteal && _lifeStealPercent <= 0)
            {
                _lifeStealPercent = 10f; // 기본값
            }
            else if (!_hasLifeSteal)
            {
                _lifeStealPercent = 0f;
            }
        }
        
        public override string GetTooltipText()
        {
            string tooltip = base.GetTooltipText();
            
            tooltip += "\n<color=white>━━━━━━━━━━━━━━━━</color>\n";
            
            // 무기 타입
            tooltip += $"Type: {_weaponType}\n";
            tooltip += $"Damage Type: <color=orange>{_damageType}</color>\n\n";
            
            // 기본 스탯
            tooltip += "<color=yellow>Base Stats:</color>\n";
            tooltip += $"  Attack Power: <color=red>{_baseAttackPower}</color>\n";
            tooltip += $"  Attack Speed: {_attackSpeed:F1}/sec\n";
            tooltip += $"  Attack Range: {_attackRange:F1} tiles\n";
            
            // 크리티컬 정보
            if (_criticalChance > 0)
            {
                tooltip += $"  Critical Chance: <color=cyan>{_criticalChance:F1}%</color>\n";
                tooltip += $"  Critical Damage: <color=cyan>{_criticalDamageMultiplier:F0}%</color>\n";
            }
            
            // 추가 스탯 보너스
            if (_statBonuses.Count > 0)
            {
                tooltip += "\n<color=lime>Bonus Stats:</color>\n";
                foreach (var bonus in _statBonuses)
                {
                    string sign = bonus.value >= 0 ? "+" : "";
                    tooltip += $"  {sign}{bonus.value} {bonus.statType}\n";
                }
            }
            
            // 특수 효과
            if (_hasPiercing || _hasLifeSteal)
            {
                tooltip += "\n<color=magenta>Special Effects:</color>\n";
                if (_hasPiercing)
                    tooltip += "  • Piercing Attack\n";
                if (_hasLifeSteal)
                    tooltip += $"  • Life Steal: {_lifeStealPercent:F0}%\n";
            }
            
            return tooltip;
        }
        
        #endregion
        
        #region Editor Methods
        
#if UNITY_EDITOR
        [ContextMenu("Auto Balance Weapon Stats")]
        private void AutoBalanceStats()
        {
            // 무기 타입에 따른 자동 스탯 밸런싱
            switch (_weaponType)
            {
                case WeaponType.Dagger:
                    _baseAttackPower = 5 + (RequiredLevel * 2);
                    _attackSpeed = 2.0f;
                    _criticalChance = 15f;
                    _attackRange = 1.0f;
                    break;
                    
                case WeaponType.OneHandedSword:
                    _baseAttackPower = 10 + (RequiredLevel * 3);
                    _attackSpeed = 1.0f;
                    _criticalChance = 5f;
                    _attackRange = 1.0f;
                    break;
                    
                case WeaponType.TwoHandedSword:
                    _baseAttackPower = 20 + (RequiredLevel * 5);
                    _attackSpeed = 0.6f;
                    _criticalChance = 10f;
                    _attackRange = 1.5f;
                    break;
                    
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                    _baseAttackPower = 8 + (RequiredLevel * 3);
                    _attackSpeed = _weaponType == WeaponType.Bow ? 1.2f : 0.8f;
                    _criticalChance = 10f;
                    _attackRange = 5.0f;
                    break;
                    
                case WeaponType.Staff:
                case WeaponType.Wand:
                    _baseAttackPower = 15 + (RequiredLevel * 4);
                    _attackSpeed = 0.8f;
                    _criticalChance = 5f;
                    _attackRange = 3.0f;
                    _damageType = DamageType.Magical;
                    break;
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        
        #endregion
    }
    
    /// <summary>
    /// 스탯 보너스 구조체
    /// </summary>
    [System.Serializable]
    public struct StatBonus
    {
        public StatType statType;
        public float value;
        
        public StatBonus(StatType type, float val)
        {
            statType = type;
            value = val;
        }
    }
}
