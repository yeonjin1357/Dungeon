using UnityEngine;

namespace Dungeon.Player
{
    public class GridPosition : MonoBehaviour
    {
        #region Fields
        [Header("Grid Settings")]
        [SerializeField] private Vector2Int _currentGridPosition;
        [SerializeField] private float _cellSize = 1f;
        
        private Grid _grid;
        #endregion

        #region Properties
        public Vector2Int CurrentPosition => _currentGridPosition;
        public float CellSize => _cellSize;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            GameObject gridObject = GameObject.Find("Grid");
            if (gridObject != null)
            {
                _grid = gridObject.GetComponent<Grid>();
                if (_grid != null)
                {
                    _cellSize = _grid.cellSize.x;
                }
            }
            
            InitializePosition();
        }
        #endregion

        #region Public Methods
        public void SetGridPosition(Vector2Int newPosition)
        {
            _currentGridPosition = newPosition;
            UpdateWorldPosition();
        }
        
        public void SetGridPosition(int x, int y)
        {
            SetGridPosition(new Vector2Int(x, y));
        }
        
        public Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            if (_grid != null)
            {
                return _grid.CellToWorld(new Vector3Int(gridPosition.x, gridPosition.y, 0)) + _grid.cellSize * 0.5f;
            }
            return new Vector3(gridPosition.x * _cellSize, gridPosition.y * _cellSize, 0);
        }
        
        public Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            if (_grid != null)
            {
                Vector3Int cellPos = _grid.WorldToCell(worldPosition);
                return new Vector2Int(cellPos.x, cellPos.y);
            }
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x / _cellSize),
                Mathf.RoundToInt(worldPosition.y / _cellSize)
            );
        }
        
        public void MoveBy(Vector2Int offset)
        {
            SetGridPosition(_currentGridPosition + offset);
        }
        #endregion

        #region Private Methods
        private void InitializePosition()
        {
            _currentGridPosition = GetGridPosition(transform.position);
            UpdateWorldPosition();
        }
        
        private void UpdateWorldPosition()
        {
            Vector3 targetPosition = GetWorldPosition(_currentGridPosition);
            targetPosition.z = transform.position.z;
            transform.position = targetPosition;
        }
        #endregion
    }
}