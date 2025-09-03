using System.Collections;
using UnityEngine;
using Dungeon.Dungeon;

namespace Dungeon.Player
{
    [RequireComponent(typeof(GridPosition))]
    public class PlayerMovement : MonoBehaviour
    {
        #region Events
        public delegate void MoveStartDelegate(Vector2Int direction);
        public event MoveStartDelegate OnMoveStart;
        
        public delegate void MoveCompleteDelegate();
        public event MoveCompleteDelegate OnMoveComplete;
        #endregion
        
        #region Fields
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private bool _isMoving = false;
        [SerializeField] private bool _allowDiagonalMovement = false;
        
        [Header("References")]
        private GridPosition _gridPosition;
        private TilemapController _tilemapController;
        
        // Input directions
        private Vector2Int[] _cardinalDirections = new Vector2Int[]
        {
            Vector2Int.up,    // W
            Vector2Int.down,  // S
            Vector2Int.left,  // A
            Vector2Int.right  // D
        };
        #endregion

        #region Properties
        public bool IsMoving => _isMoving;
        public Vector2Int CurrentGridPosition => _gridPosition.CurrentPosition;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _gridPosition = GetComponent<GridPosition>();
            
            // Find TilemapController
            GameObject gridObject = GameObject.Find("Grid");
            if (gridObject != null)
            {
                _tilemapController = gridObject.GetComponent<TilemapController>();
            }
        }

        private void Update()
        {
            if (!_isMoving)
            {
                HandleKeyboardInput();
            }
        }
        #endregion

        #region Public Methods
        public bool TryMove(Vector2Int direction)
        {
            if (_isMoving) return false;
            
            Vector2Int targetPosition = _gridPosition.CurrentPosition + direction;
            
            if (IsValidMove(targetPosition))
            {
                StartCoroutine(MoveToPosition(targetPosition));
                return true;
            }
            
            return false;
        }
        
        public bool TryMoveToPosition(Vector2Int targetPosition)
        {
            if (_isMoving) return false;
            
            // Calculate direction
            Vector2Int direction = targetPosition - _gridPosition.CurrentPosition;
            
            // Check if it's a single step move
            if (Mathf.Abs(direction.x) <= 1 && Mathf.Abs(direction.y) <= 1)
            {
                // Check diagonal movement
                if (!_allowDiagonalMovement && direction.x != 0 && direction.y != 0)
                {
                    return false;
                }
                
                if (IsValidMove(targetPosition))
                {
                    StartCoroutine(MoveToPosition(targetPosition));
                    return true;
                }
            }
            
            return false;
        }
        
        public bool IsValidMove(Vector2Int targetPosition)
        {
            // Check with tilemap controller if available
            if (_tilemapController != null)
            {
                return _tilemapController.IsWalkable(targetPosition.x, targetPosition.y);
            }
            
            // Default: allow movement if no tilemap controller
            return true;
        }
        #endregion

        #region Private Methods
        private void HandleKeyboardInput()
        {
            Vector2Int moveDirection = Vector2Int.zero;
            
            // Get input
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                moveDirection = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                moveDirection = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                moveDirection = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                moveDirection = Vector2Int.right;
            
            // Handle diagonal movement if allowed
            if (_allowDiagonalMovement)
            {
                if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
                    moveDirection = new Vector2Int(1, 1);
                else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
                    moveDirection = new Vector2Int(-1, 1);
                else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
                    moveDirection = new Vector2Int(1, -1);
                else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
                    moveDirection = new Vector2Int(-1, -1);
            }
            
            if (moveDirection != Vector2Int.zero)
            {
                TryMove(moveDirection);
            }
        }
        
        private IEnumerator MoveToPosition(Vector2Int targetPosition)
        {
            _isMoving = true;
            
            // Calculate direction and trigger move start event
            Vector2Int direction = targetPosition - _gridPosition.CurrentPosition;
            OnMoveStart?.Invoke(direction);
            
            Vector3 startPos = transform.position;
            Vector3 endPos = _gridPosition.GetWorldPosition(targetPosition);
            endPos.z = transform.position.z;
            
            float journey = 0f;
            float duration = 1f / _moveSpeed;
            
            while (journey <= duration)
            {
                journey += Time.deltaTime;
                float percent = Mathf.Clamp01(journey / duration);
                
                // Use smooth step for better animation
                float smoothPercent = percent * percent * (3f - 2f * percent);
                transform.position = Vector3.Lerp(startPos, endPos, smoothPercent);
                
                yield return null;
            }
            
            // Ensure final position is exact
            transform.position = endPos;
            _gridPosition.SetGridPosition(targetPosition);
            
            _isMoving = false;
            OnMoveCompleted();
        }
        
        private void OnMoveCompleted()
        {
            // Trigger turn-based system events here
            Debug.Log($"Player moved to {_gridPosition.CurrentPosition}");
            
            // Trigger move complete event
            OnMoveComplete?.Invoke();
            
            // Check for interactions at new position
            CheckForInteractions();
        }
        
        private void CheckForInteractions()
        {
            // Check for items, stairs, doors, etc.
            if (_tilemapController != null)
            {
                TileType tileType = _tilemapController.GetTileType(_gridPosition.CurrentPosition.x, _gridPosition.CurrentPosition.y);
                
                switch (tileType)
                {
                    case TileType.StairsDown:
                        Debug.Log("Found stairs down!");
                        break;
                    case TileType.StairsUp:
                        Debug.Log("Found stairs up!");
                        break;
                    case TileType.Door:
                        Debug.Log("At a door");
                        break;
                }
            }
        }
        #endregion
    }
}