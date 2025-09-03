using UnityEngine;
using Dungeon.Combat;

namespace Dungeon.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(GridPosition))]
    [RequireComponent(typeof(CombatStats))]
    public class PlayerTurnBased : MonoBehaviour, ITurnBased
    {
        #region Fields
        private PlayerController _controller;
        private PlayerMovement _movement;
        private GridPosition _gridPosition;
        private CombatStats _combatStats;
        
        private bool _isActive = true;
        private bool _hasMoved = false;
        private bool _hasActed = false;
        #endregion
        
        #region ITurnBased Implementation
        public bool IsActive => _isActive && _combatStats != null && !_combatStats.IsDead;
        
        public int TurnPriority => 100; // Player has highest priority
        
        public Vector2Int GridPosition => _gridPosition != null ? _gridPosition.CurrentPosition : Vector2Int.zero;
        
        public void ExecuteTurn()
        {
            // Player turn is handled by input
            // Just reset flags
            _hasMoved = false;
            _hasActed = false;
            
            Debug.Log("Player's turn!");
        }
        
        public void OnTurnStart()
        {
            _hasMoved = false;
            _hasActed = false;
            
            // Enable player input
            if (_movement != null)
            {
                _movement.enabled = true;
            }
        }
        
        public void OnTurnEnd()
        {
            // Disable player input until next turn
            if (_movement != null)
            {
                _movement.enabled = false;
            }
        }
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _movement = GetComponent<PlayerMovement>();
            _gridPosition = GetComponent<GridPosition>();
            _combatStats = GetComponent<CombatStats>();
            
            if (_combatStats == null)
            {
                _combatStats = gameObject.AddComponent<CombatStats>();
            }
        }
        
        private void Start()
        {
            // Register with turn manager
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.RegisterEntity(this);
            }
            
            // Subscribe to movement events
            if (_movement != null)
            {
                _movement.OnMoveComplete += OnPlayerMoved;
            }
            
            // Initialize combat stats from PlayerController
            if (_controller != null && _combatStats != null)
            {
                _combatStats.SetStats(
                    _controller.MaxHealth,
                    _controller.AttackPower,
                    _controller.Defense,
                    1 // Attack range
                );
            }
        }
        
        private void OnDestroy()
        {
            // Unregister from turn manager
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.UnregisterEntity(this);
            }
            
            // Unsubscribe from events
            if (_movement != null)
            {
                _movement.OnMoveComplete -= OnPlayerMoved;
            }
        }
        #endregion
        
        #region Public Methods
        public void EndPlayerTurn()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.EndCurrentTurn();
            }
        }
        
        public bool CanMove()
        {
            return !_hasMoved && IsActive;
        }
        
        public bool CanAct()
        {
            return !_hasActed && IsActive;
        }
        
        public void PerformAttack(GameObject target)
        {
            if (!CanAct())
            {
                Debug.Log("Player has already acted this turn!");
                return;
            }
            
            if (CombatSystem.Instance != null && CombatSystem.Instance.CanAttack(gameObject, target))
            {
                CombatSystem.Instance.PerformAttack(gameObject, target);
                _hasActed = true;
                
                // End turn after attacking
                EndPlayerTurn();
            }
        }
        #endregion
        
        #region Private Methods
        private void OnPlayerMoved()
        {
            _hasMoved = true;
            
            // Check if there's an enemy adjacent to attack
            CheckForAdjacentEnemies();
            
            // End turn after moving (if no combat)
            if (!_hasActed)
            {
                EndPlayerTurn();
            }
        }
        
        private void CheckForAdjacentEnemies()
        {
            // Check all 4 adjacent tiles for enemies
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                GridPosition + Vector2Int.up,
                GridPosition + Vector2Int.down,
                GridPosition + Vector2Int.left,
                GridPosition + Vector2Int.right
            };
            
            foreach (Vector2Int pos in adjacentPositions)
            {
                // Check for enemy at position
                Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(pos.x, pos.y), 0.1f);
                
                foreach (Collider2D col in colliders)
                {
                    if (col.CompareTag("Enemy"))
                    {
                        // Auto-attack adjacent enemy
                        PerformAttack(col.gameObject);
                        return;
                    }
                }
            }
        }
        #endregion
    }
}