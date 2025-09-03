using UnityEngine;
using System;
using Dungeon.Items.Interfaces;

namespace Dungeon.Items
{
    /// <summary>
    /// 아이템 기본 데이터를 저장하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Dungeon/Items/Basic Item", order = 0)]
    public class ItemData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _itemId = "";
        [SerializeField] private string _itemName = "New Item";
        [SerializeField] [TextArea(3, 5)] private string _description = "Item description";
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _worldPrefab; // 월드에 드롭될 때 사용할 프리팹
        
        [Header("아이템 속성")]
        [SerializeField] private ItemType _itemType = ItemType.Misc;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;
        [SerializeField] private bool _isStackable = false;
        [SerializeField] private int _maxStackSize = 1;
        
        [Header("거래 정보")]
        [SerializeField] private int _buyPrice = 10;
        [SerializeField] private int _sellPrice = 5;
        [SerializeField] private bool _isSellable = true;
        [SerializeField] private bool _isDroppable = true;
        
        [Header("사용 제한")]
        [SerializeField] private int _requiredLevel = 1;
        [SerializeField] private string _requiredClass = ""; // 빈 문자열이면 모든 클래스 사용 가능
        
        #region Properties
        
        public string ItemId => _itemId;
        public string ItemName => _itemName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameObject WorldPrefab => _worldPrefab;
        
        public ItemType ItemType => _itemType;
        public ItemRarity Rarity => _rarity;
        public bool IsStackable => _isStackable;
        public int MaxStackSize => _maxStackSize;
        
        public int BuyPrice => _buyPrice;
        public int SellPrice => _sellPrice;
        public bool IsSellable => _isSellable;
        public bool IsDroppable => _isDroppable;
        
        public int RequiredLevel => _requiredLevel;
        public string RequiredClass => _requiredClass;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void OnValidate()
        {
            // ID가 비어있으면 자동 생성
            if (string.IsNullOrEmpty(_itemId))
            {
                _itemId = Guid.NewGuid().ToString();
            }
            
            // 스택 불가능한 아이템은 최대 스택 1로 고정
            if (!_isStackable)
            {
                _maxStackSize = 1;
            }
            else if (_maxStackSize < 1)
            {
                _maxStackSize = 1;
            }
            
            // 판매 가격은 구매 가격을 초과할 수 없음
            if (_sellPrice > _buyPrice)
            {
                _sellPrice = _buyPrice / 2;
            }
            
            // 필요 레벨 최소값 보정
            if (_requiredLevel < 1)
            {
                _requiredLevel = 1;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 아이템 사용 가능 여부 확인
        /// </summary>
        public virtual bool CanUseItem(GameObject user)
        {
            // 기본적으로 레벨 체크만 수행
            // 실제 구현은 상속받은 클래스에서
            return true;
        }
        
        /// <summary>
        /// 아이템 툴팁 텍스트 생성
        /// </summary>
        public virtual string GetTooltipText()
        {
            string tooltip = $"<color=white>{_itemName}</color>\n";
            
            // 희귀도에 따른 색상
            string rarityColor = GetRarityColorHex();
            tooltip += $"<color={rarityColor}>{_rarity}</color>\n\n";
            
            // 설명
            tooltip += $"{_description}\n\n";
            
            // 타입
            tooltip += $"Type: {_itemType}\n";
            
            // 필요 레벨
            if (_requiredLevel > 1)
            {
                tooltip += $"Required Level: {_requiredLevel}\n";
            }
            
            // 필요 클래스
            if (!string.IsNullOrEmpty(_requiredClass))
            {
                tooltip += $"Required Class: {_requiredClass}\n";
            }
            
            // 가격 정보
            if (_isSellable)
            {
                tooltip += $"\n<color=yellow>Sell Price: {_sellPrice} Gold</color>";
            }
            
            return tooltip;
        }
        
        /// <summary>
        /// 희귀도에 따른 색상 코드 반환
        /// </summary>
        protected string GetRarityColorHex()
        {
            switch (_rarity)
            {
                case ItemRarity.Common:
                    return "#FFFFFF"; // 흰색
                case ItemRarity.Uncommon:
                    return "#00FF00"; // 초록색
                case ItemRarity.Rare:
                    return "#0080FF"; // 파란색
                case ItemRarity.Epic:
                    return "#A020F0"; // 보라색
                case ItemRarity.Legendary:
                    return "#FFA500"; // 주황색
                default:
                    return "#FFFFFF";
            }
        }
        
        /// <summary>
        /// 아이템 복제 (인스턴스 생성)
        /// </summary>
        public virtual ItemData Clone()
        {
            return Instantiate(this);
        }
        
        #endregion
        
        #region Editor Methods
        
#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 아이템 ID 재생성
        /// </summary>
        [ContextMenu("Generate New Item ID")]
        private void GenerateNewItemId()
        {
            _itemId = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        /// <summary>
        /// 에디터에서 가격 자동 설정
        /// </summary>
        [ContextMenu("Auto Calculate Prices")]
        private void AutoCalculatePrices()
        {
            // 희귀도에 따른 기본 가격 설정
            int basePrice = 10;
            switch (_rarity)
            {
                case ItemRarity.Common:
                    basePrice = 10;
                    break;
                case ItemRarity.Uncommon:
                    basePrice = 50;
                    break;
                case ItemRarity.Rare:
                    basePrice = 200;
                    break;
                case ItemRarity.Epic:
                    basePrice = 1000;
                    break;
                case ItemRarity.Legendary:
                    basePrice = 5000;
                    break;
            }
            
            _buyPrice = basePrice * _requiredLevel;
            _sellPrice = _buyPrice / 2;
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        
        #endregion
    }
}