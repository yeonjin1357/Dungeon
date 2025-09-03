using UnityEngine;
using System.Collections.Generic;
using Dungeon.Items.Interfaces;
using Dungeon.Player;

namespace Dungeon.Items
{
    /// <summary>
    /// 방어구 타입을 정의하는 열거형
    /// </summary>
    [System.Serializable]
    public enum ArmorType
    {
        Cloth,      // 천 방어구 (마법사용)
        Leather,    // 가죽 방어구 (도적용)
        Mail,       // 사슬 방어구 (중형)
        Plate       // 판금 방어구 (전사용)
    }
    
    /// <summary>
    /// 방어구 아이템 데이터를 저장하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Armor", menuName = "Dungeon/Items/Armor", order = 2)]
    public class ArmorData : ItemData, IEquippable
    {
        [Header("방어구 기본 정보")]
        [SerializeField] private EquipmentSlot _equipmentSlot = EquipmentSlot.Body;
        [SerializeField] private ArmorType _armorType = ArmorType.Leather;
        
        [Header("방어 스탯")]
        [SerializeField] private int _baseDefense = 5;
        [SerializeField] private int _magicResistance = 0;
        
        [Header("회피 및 방어 확률")]
        [SerializeField] [Range(0, 50)] private float _dodgeChance = 0f; // %
        [SerializeField] [Range(0, 50)] private float _blockChance = 0f; // %
        [SerializeField] [Range(0, 50)] private float _damageReduction = 0f; // %
        
        [Header("추가 스탯 보너스")]
        [SerializeField] private List<StatBonus> _statBonuses = new List<StatBonus>();
        
        [Header("세트 효과")]
        [SerializeField] private string _setName = ""; // 세트 아이템 이름
        [SerializeField] private int _setPiecesRequired = 0; // 세트 효과 발동에 필요한 개수
        [SerializeField] private List<StatBonus> _setBonuses = new List<StatBonus>(); // 세트 보너스
        
        [Header("특수 효과")]
        [SerializeField] private bool _hasReflectDamage = false; // 반사 데미지
        [SerializeField] [Range(0, 50)] private float _reflectPercent = 0f; // %
        [SerializeField] private bool _hasThorns = false; // 가시 효과
        [SerializeField] private int _thornsDamage = 0;
        
        #region Properties
        
        public ArmorType ArmorType => _armorType;
        public int BaseDefense => _baseDefense;
        public int MagicResistance => _magicResistance;
        public float DodgeChance => _dodgeChance;
        public float BlockChance => _blockChance;
        public float DamageReduction => _damageReduction;
        public string SetName => _setName;
        public int SetPiecesRequired => _setPiecesRequired;
        public bool HasReflectDamage => _hasReflectDamage;
        public float ReflectPercent => _reflectPercent;
        public bool HasThorns => _hasThorns;
        public int ThornsDamage => _thornsDamage;
        
        #endregion
        
        #region IEquippable Implementation
        
        public EquipmentSlot SlotType => _equipmentSlot;
        
        // IEquippable 인터페이스에서 요구하는 속성들 (ItemData에서 상속)
        public new int RequiredLevel => base.RequiredLevel;
        public new string RequiredClass => base.RequiredClass;
        
        public bool CanEquip(GameObject wearer)
        {
            if (wearer == null) return false;
            
            // 레벨 체크
            var stats = wearer.GetComponent<PlayerStats>();
            if (stats != null && stats.Level < RequiredLevel)
            {
                Debug.Log($"레벨이 부족합니다. 필요 레벨: {RequiredLevel}");
                return false;
            }
            
            // 클래스 체크 (필요시)
            if (!string.IsNullOrEmpty(RequiredClass))
            {
                // 클래스별 방어구 제한 체크
                // var playerClass = wearer.GetComponent<PlayerClass>();
                // if (playerClass != null)
                // {
                //     // 예: 판금 방어구는 전사만 착용 가능
                //     if (_armorType == ArmorType.Plate && playerClass.ClassName != "Warrior")
                //         return false;
                // }
            }
            
            // 이미 같은 슬롯에 장비가 있는지 체크
            // var equipment = wearer.GetComponent<EquipmentManager>();
            // if (equipment != null && equipment.IsSlotOccupied(_equipmentSlot))
            // {
            //     Debug.Log($"{_equipmentSlot} 슬롯에 이미 장비가 있습니다.");
            //     // 교체 가능하므로 true 반환
            // }
            
            return true;
        }
        
        public void Equip(GameObject wearer)
        {
            if (!CanEquip(wearer)) return;
            
            Debug.Log($"{ItemName} 방어구를 장착했습니다!");
            
            // 스탯 적용
            var stats = wearer.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // 기본 방어력 적용
                stats.ModifyStat(StatType.Defense, _baseDefense);
                stats.ModifyStat(StatType.MagicResistance, _magicResistance);
                
                // 회피/방어 확률 적용
                if (_dodgeChance > 0)
                    stats.ModifyStat(StatType.DodgeChance, _dodgeChance);
                if (_blockChance > 0)
                    stats.ModifyStat(StatType.BlockChance, _blockChance);
                if (_damageReduction > 0)
                    stats.ModifyStat(StatType.DamageReduction, _damageReduction);
                
                // 추가 스탯 보너스 적용
                foreach (var bonus in _statBonuses)
                {
                    stats.ModifyStat(bonus.statType, bonus.value);
                }
                
                // 세트 효과 체크 및 적용
                CheckAndApplySetBonus(wearer, true);
            }
            
            // 방어구 모델 변경 (있다면)
            // var armorRenderer = wearer.GetComponentInChildren<ArmorRenderer>();
            // if (armorRenderer != null && WorldPrefab != null)
            // {
            //     armorRenderer.SetArmor(_equipmentSlot, WorldPrefab);
            // }
        }
        
        public void Unequip(GameObject wearer)
        {
            Debug.Log($"{ItemName} 방어구를 해제했습니다!");
            
            // 스탯 제거
            var stats = wearer.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // 기본 방어력 제거
                stats.ModifyStat(StatType.Defense, -_baseDefense);
                stats.ModifyStat(StatType.MagicResistance, -_magicResistance);
                
                // 회피/방어 확률 제거
                if (_dodgeChance > 0)
                    stats.ModifyStat(StatType.DodgeChance, -_dodgeChance);
                if (_blockChance > 0)
                    stats.ModifyStat(StatType.BlockChance, -_blockChance);
                if (_damageReduction > 0)
                    stats.ModifyStat(StatType.DamageReduction, -_damageReduction);
                
                // 추가 스탯 보너스 제거
                foreach (var bonus in _statBonuses)
                {
                    stats.ModifyStat(bonus.statType, -bonus.value);
                }
                
                // 세트 효과 제거
                CheckAndApplySetBonus(wearer, false);
            }
            
            // 방어구 모델 제거 (있다면)
            // var armorRenderer = wearer.GetComponentInChildren<ArmorRenderer>();
            // if (armorRenderer != null)
            // {
            //     armorRenderer.RemoveArmor(_equipmentSlot);
            // }
        }
        
        public Dictionary<string, float> GetStatBonuses()
        {
            var bonuses = new Dictionary<string, float>();
            
            // 기본 방어 스탯
            bonuses["Defense"] = _baseDefense;
            bonuses["MagicResistance"] = _magicResistance;
            
            if (_dodgeChance > 0)
                bonuses["DodgeChance"] = _dodgeChance;
            if (_blockChance > 0)
                bonuses["BlockChance"] = _blockChance;
            if (_damageReduction > 0)
                bonuses["DamageReduction"] = _damageReduction;
            
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
        
        #region Set Bonus Methods
        
        private void CheckAndApplySetBonus(GameObject wearer, bool isEquipping)
        {
            if (string.IsNullOrEmpty(_setName) || _setPiecesRequired <= 0)
                return;
            
            // 세트 아이템 개수 체크 (EquipmentManager 구현 필요)
            // var equipment = wearer.GetComponent<EquipmentManager>();
            // if (equipment != null)
            // {
            //     int setPieces = equipment.CountSetPieces(_setName);
            //     
            //     if (isEquipping && setPieces >= _setPiecesRequired)
            //     {
            //         // 세트 보너스 적용
            //         ApplySetBonuses(wearer, true);
            //     }
            //     else if (!isEquipping && setPieces < _setPiecesRequired)
            //     {
            //         // 세트 보너스 제거
            //         ApplySetBonuses(wearer, false);
            //     }
            // }
        }
        
        private void ApplySetBonuses(GameObject wearer, bool apply)
        {
            var stats = wearer.GetComponent<PlayerStats>();
            if (stats == null) return;
            
            foreach (var bonus in _setBonuses)
            {
                float value = apply ? bonus.value : -bonus.value;
                stats.ModifyStat(bonus.statType, value);
            }
            
            if (apply)
                Debug.Log($"{_setName} 세트 효과가 활성화되었습니다!");
            else
                Debug.Log($"{_setName} 세트 효과가 비활성화되었습니다!");
        }
        
        #endregion
        
        #region Override Methods
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // 방어구는 스택 불가능
            if (IsStackable)
            {
                Debug.LogWarning("방어구는 스택할 수 없습니다. IsStackable을 false로 설정합니다.");
            }
            
            // 방어력 최소값 보정
            if (_baseDefense < 0)
                _baseDefense = 0;
            
            // 마법 저항력 최소값 보정
            if (_magicResistance < 0)
                _magicResistance = 0;
            
            // 반사 데미지 설정 검증
            if (_hasReflectDamage && _reflectPercent <= 0)
            {
                _reflectPercent = 10f; // 기본값
            }
            else if (!_hasReflectDamage)
            {
                _reflectPercent = 0f;
            }
            
            // 가시 데미지 설정 검증
            if (_hasThorns && _thornsDamage <= 0)
            {
                _thornsDamage = 5; // 기본값
            }
            else if (!_hasThorns)
            {
                _thornsDamage = 0;
            }
        }
        
        public override string GetTooltipText()
        {
            string tooltip = base.GetTooltipText();
            
            tooltip += "\n<color=white>━━━━━━━━━━━━━━━━</color>\n";
            
            // 방어구 정보
            tooltip += $"Slot: <color=cyan>{_equipmentSlot}</color>\n";
            tooltip += $"Armor Type: {_armorType}\n\n";
            
            // 기본 스탯
            tooltip += "<color=yellow>Defense Stats:</color>\n";
            tooltip += $"  Defense: <color=green>+{_baseDefense}</color>\n";
            if (_magicResistance > 0)
                tooltip += $"  Magic Resistance: <color=blue>+{_magicResistance}</color>\n";
            
            // 회피/방어 확률
            if (_dodgeChance > 0 || _blockChance > 0 || _damageReduction > 0)
            {
                tooltip += "\n<color=cyan>Defensive Bonuses:</color>\n";
                if (_dodgeChance > 0)
                    tooltip += $"  Dodge Chance: {_dodgeChance:F1}%\n";
                if (_blockChance > 0)
                    tooltip += $"  Block Chance: {_blockChance:F1}%\n";
                if (_damageReduction > 0)
                    tooltip += $"  Damage Reduction: {_damageReduction:F1}%\n";
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
            
            // 세트 효과
            if (!string.IsNullOrEmpty(_setName) && _setPiecesRequired > 0)
            {
                tooltip += $"\n<color=gold>Set: {_setName} ({_setPiecesRequired} pieces)</color>\n";
                if (_setBonuses.Count > 0)
                {
                    tooltip += "<color=gray>Set Bonuses:</color>\n";
                    foreach (var bonus in _setBonuses)
                    {
                        string sign = bonus.value >= 0 ? "+" : "";
                        tooltip += $"  {sign}{bonus.value} {bonus.statType}\n";
                    }
                }
            }
            
            // 특수 효과
            if (_hasReflectDamage || _hasThorns)
            {
                tooltip += "\n<color=magenta>Special Effects:</color>\n";
                if (_hasReflectDamage)
                    tooltip += $"  • Reflects {_reflectPercent:F0}% damage\n";
                if (_hasThorns)
                    tooltip += $"  • Thorns: {_thornsDamage} damage\n";
            }
            
            return tooltip;
        }
        
        #endregion
        
        #region Editor Methods
        
#if UNITY_EDITOR
        [ContextMenu("Auto Balance Armor Stats")]
        private void AutoBalanceStats()
        {
            // 방어구 타입과 슬롯에 따른 자동 스탯 밸런싱
            float slotMultiplier = 1.0f;
            switch (_equipmentSlot)
            {
                case EquipmentSlot.Head:
                    slotMultiplier = 0.8f;
                    break;
                case EquipmentSlot.Body:
                    slotMultiplier = 1.5f;
                    break;
                case EquipmentSlot.Hands:
                    slotMultiplier = 0.6f;
                    break;
                case EquipmentSlot.Feet:
                    slotMultiplier = 0.7f;
                    break;
                case EquipmentSlot.Accessory1:
                case EquipmentSlot.Accessory2:
                    slotMultiplier = 0.3f;
                    break;
            }
            
            switch (_armorType)
            {
                case ArmorType.Cloth:
                    _baseDefense = Mathf.RoundToInt((2 + RequiredLevel * 1) * slotMultiplier);
                    _magicResistance = Mathf.RoundToInt((5 + RequiredLevel * 2) * slotMultiplier);
                    _dodgeChance = 5f;
                    break;
                    
                case ArmorType.Leather:
                    _baseDefense = Mathf.RoundToInt((5 + RequiredLevel * 2) * slotMultiplier);
                    _magicResistance = Mathf.RoundToInt((2 + RequiredLevel * 1) * slotMultiplier);
                    _dodgeChance = 10f;
                    break;
                    
                case ArmorType.Mail:
                    _baseDefense = Mathf.RoundToInt((10 + RequiredLevel * 3) * slotMultiplier);
                    _magicResistance = Mathf.RoundToInt((3 + RequiredLevel * 1) * slotMultiplier);
                    _blockChance = 5f;
                    break;
                    
                case ArmorType.Plate:
                    _baseDefense = Mathf.RoundToInt((15 + RequiredLevel * 4) * slotMultiplier);
                    _magicResistance = Mathf.RoundToInt((1 + RequiredLevel * 0.5f) * slotMultiplier);
                    _blockChance = 10f;
                    _damageReduction = 5f;
                    break;
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        
        #endregion
    }
}
