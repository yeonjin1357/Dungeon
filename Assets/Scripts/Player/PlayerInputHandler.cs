using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dungeon.Dungeon;

namespace Dungeon.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInputHandler : MonoBehaviour
    {
        #region Fields
        [Header("Touch Settings")]
        [SerializeField] private float _tapThreshold = 0.3f; // Max time for tap
        [SerializeField] private float _swipeThreshold = 50f; // Min distance for swipe
        [SerializeField] private float _holdThreshold = 0.5f; // Min time for hold
        
        [Header("Input Buffer")]
        [SerializeField] private int _maxBufferSize = 3;
        private Queue<InputCommand> _inputBuffer;
        
        [Header("References")]
        private PlayerMovement _playerMovement;
        private UnityEngine.Camera _mainCamera;
        private Grid _grid;
        
        // Touch tracking
        private Vector2 _touchStartPosition;
        private float _touchStartTime;
        private bool _isTouching;
        private bool _isHolding;
        private Coroutine _holdCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _mainCamera = UnityEngine.Camera.main;
            _inputBuffer = new Queue<InputCommand>();
            
            // Find grid
            GameObject gridObject = GameObject.Find("Grid");
            if (gridObject != null)
            {
                _grid = gridObject.GetComponent<Grid>();
            }
        }

        private void Update()
        {
            HandleInput();
            ProcessInputBuffer();
        }
        #endregion

        #region Input Handling
        private void HandleInput()
        {
            // Handle mouse input (for editor testing)
            if (Application.isEditor || !Application.isMobilePlatform)
            {
                HandleMouseInput();
            }
            // Handle touch input (for mobile)
            else if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnTouchStart(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                OnTouchMove(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnTouchEnd(Input.mousePosition);
            }
        }

        private void HandleTouchInput()
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchStart(touch.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnTouchMove(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnTouchEnd(touch.position);
                    break;
            }
        }

        private void OnTouchStart(Vector2 position)
        {
            _touchStartPosition = position;
            _touchStartTime = Time.time;
            _isTouching = true;
            _isHolding = false;
            
            // Start hold detection
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
            }
            _holdCoroutine = StartCoroutine(DetectHold());
        }

        private void OnTouchMove(Vector2 position)
        {
            if (!_isTouching) return;
            
            // Cancel hold if moved too far
            float distance = Vector2.Distance(_touchStartPosition, position);
            if (distance > _swipeThreshold * 0.5f && _holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
                _isHolding = false;
            }
        }

        private void OnTouchEnd(Vector2 position)
        {
            if (!_isTouching) return;
            
            _isTouching = false;
            
            // Stop hold detection
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }
            
            float touchDuration = Time.time - _touchStartTime;
            float swipeDistance = Vector2.Distance(_touchStartPosition, position);
            
            if (_isHolding)
            {
                // Already handled by hold
                _isHolding = false;
            }
            else if (swipeDistance >= _swipeThreshold)
            {
                // Swipe detected
                Vector2 swipeDirection = (position - _touchStartPosition).normalized;
                OnSwipe(swipeDirection);
            }
            else if (touchDuration < _tapThreshold)
            {
                // Tap detected
                OnTap(position);
            }
        }

        private IEnumerator DetectHold()
        {
            yield return new WaitForSeconds(_holdThreshold);
            
            if (_isTouching)
            {
                _isHolding = true;
                OnHold(_touchStartPosition);
            }
        }
        #endregion

        #region Input Commands
        private void OnTap(Vector2 screenPosition)
        {
            // Convert screen position to world position
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPosition);
            worldPos.z = 0;
            
            // Convert to grid position
            Vector2Int gridPos = GetGridPosition(worldPos);
            
            // Calculate direction from player to tap position
            Vector2Int playerPos = _playerMovement.CurrentGridPosition;
            Vector2Int direction = gridPos - playerPos;
            
            // If tapped on adjacent tile, move there
            if (Mathf.Abs(direction.x) <= 1 && Mathf.Abs(direction.y) <= 1)
            {
                if (direction != Vector2Int.zero)
                {
                    AddInputCommand(new InputCommand
                    {
                        Type = CommandType.Move,
                        Direction = direction,
                        Position = gridPos
                    });
                }
            }
            // If tapped further away, pathfind (simplified: move in general direction)
            else if (direction != Vector2Int.zero)
            {
                // Normalize to cardinal direction
                Vector2Int moveDir = Vector2Int.zero;
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    moveDir.x = direction.x > 0 ? 1 : -1;
                }
                else
                {
                    moveDir.y = direction.y > 0 ? 1 : -1;
                }
                
                AddInputCommand(new InputCommand
                {
                    Type = CommandType.Move,
                    Direction = moveDir,
                    Position = gridPos
                });
            }
            
            Debug.Log($"Tap at grid position: {gridPos}");
        }

        private void OnHold(Vector2 screenPosition)
        {
            // Convert to world position
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPosition);
            worldPos.z = 0;
            
            // Convert to grid position
            Vector2Int gridPos = GetGridPosition(worldPos);
            
            // Examine/interact with tile
            AddInputCommand(new InputCommand
            {
                Type = CommandType.Examine,
                Position = gridPos
            });
            
            Debug.Log($"Hold at grid position: {gridPos} - Examining");
        }

        private void OnSwipe(Vector2 direction)
        {
            // Convert swipe to cardinal direction
            Vector2Int moveDir = Vector2Int.zero;
            
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                moveDir.x = direction.x > 0 ? 1 : -1;
            }
            else
            {
                moveDir.y = direction.y > 0 ? 1 : -1;
            }
            
            AddInputCommand(new InputCommand
            {
                Type = CommandType.Move,
                Direction = moveDir
            });
            
            Debug.Log($"Swipe in direction: {moveDir}");
        }
        #endregion

        #region Input Buffer
        private void AddInputCommand(InputCommand command)
        {
            if (_inputBuffer.Count >= _maxBufferSize)
            {
                _inputBuffer.Dequeue(); // Remove oldest
            }
            
            _inputBuffer.Enqueue(command);
        }

        private void ProcessInputBuffer()
        {
            if (_inputBuffer.Count == 0) return;
            if (_playerMovement.IsMoving) return;
            
            InputCommand command = _inputBuffer.Dequeue();
            ExecuteCommand(command);
        }

        private void ExecuteCommand(InputCommand command)
        {
            switch (command.Type)
            {
                case CommandType.Move:
                    _playerMovement.TryMove(command.Direction);
                    break;
                case CommandType.Examine:
                    ExamineTile(command.Position);
                    break;
            }
        }
        #endregion

        #region Helper Methods
        private Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            if (_grid != null)
            {
                Vector3Int cellPos = _grid.WorldToCell(worldPosition);
                return new Vector2Int(cellPos.x, cellPos.y);
            }
            
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x),
                Mathf.RoundToInt(worldPosition.y)
            );
        }

        private void ExamineTile(Vector2Int position)
        {
            // Check what's at this position
            GameObject gridObject = GameObject.Find("Grid");
            if (gridObject != null)
            {
                TilemapController tilemapController = gridObject.GetComponent<TilemapController>();
                if (tilemapController != null)
                {
                    TileType tileType = tilemapController.GetTileType(position.x, position.y);
                    Debug.Log($"Examining tile at {position}: {tileType}");
                }
            }
        }
        #endregion

        #region Inner Classes
        private enum CommandType
        {
            Move,
            Examine
        }

        private struct InputCommand
        {
            public CommandType Type;
            public Vector2Int Direction;
            public Vector2Int Position;
        }
        #endregion
    }
}
