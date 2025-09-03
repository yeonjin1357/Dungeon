using UnityEngine;

namespace Dungeon.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(GridPosition))]
    [RequireComponent(typeof(PlayerAnimator))]
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        [Header("Player Stats")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth;
        [SerializeField] private int _attackPower = 10;
        [SerializeField] private int _defense = 5;
        
        [Header("Visual")]
        [SerializeField] private Color _playerColor = Color.blue;
        
        private PlayerMovement _movement;
        private GridPosition _gridPosition;
        private SpriteRenderer _spriteRenderer;
        private PlayerAnimator _animator;
        private PlayerInputHandler _inputHandler;
        #endregion

        #region Properties
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public int AttackPower => _attackPower;
        public int Defense => _defense;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _gridPosition = GetComponent<GridPosition>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<PlayerAnimator>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<PlayerAnimator>();
            }
            
            if (_inputHandler == null)
            {
                _inputHandler = gameObject.AddComponent<PlayerInputHandler>();
            }
            
            InitializePlayer();
            SetupInputCallbacks();
        }

        private void Start()
        {
            // Set initial position to center of the room
            _gridPosition.SetGridPosition(5, 4);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - _defense);
            _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);
            
            Debug.Log($"Player took {actualDamage} damage. Health: {_currentHealth}/{_maxHealth}");
            
            _animator.PlayHurtAnimation();
            
            if (_currentHealth <= 0)
            {
                OnDeath();
            }
        }
        
        public void Heal(int amount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            Debug.Log($"Player healed for {amount}. Health: {_currentHealth}/{_maxHealth}");
        }
        #endregion

        #region Private Methods
        private void InitializePlayer()
        {
            _currentHealth = _maxHealth;
            
            // Create a simple colored sprite for the player
            CreatePlayerSprite();
        }
        
        private void CreatePlayerSprite()
        {
            // Create a simple square sprite for testing
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            
            // Create a simple character shape
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    // Create a simple circle shape
                    float centerX = 16f;
                    float centerY = 16f;
                    float distance = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2));
                    
                    if (distance < 14)
                    {
                        pixels[y * 32 + x] = _playerColor;
                    }
                    else
                    {
                        pixels[y * 32 + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            
            Sprite playerSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
            _spriteRenderer.sprite = playerSprite;
            _spriteRenderer.sortingLayerName = "Default";
            _spriteRenderer.sortingOrder = 10; // Above tiles
        }
        
        private void OnDeath()
        {
            Debug.Log("Player died!");
            _animator.PlayDeathAnimation();
            // TODO: Implement death logic (game over screen, respawn, etc.)
        }
        
        private void SetupInputCallbacks()
        {
            if (_movement != null)
            {
                _movement.OnMoveStart += OnMoveStart;
                _movement.OnMoveComplete += OnMoveComplete;
            }
        }
        
        private void OnDestroy()
        {
            if (_movement != null)
            {
                _movement.OnMoveStart -= OnMoveStart;
                _movement.OnMoveComplete -= OnMoveComplete;
            }
        }
        
        private void OnMoveStart(Vector2Int direction)
        {
            _animator.PlayMoveAnimation(direction);
        }
        
        private void OnMoveComplete()
        {
            _animator.PlayIdleAnimation();
        }
        #endregion
    }
}