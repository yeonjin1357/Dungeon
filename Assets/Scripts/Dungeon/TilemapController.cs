using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon.Dungeon
{
    public class TilemapController : MonoBehaviour
    {
        #region Fields
        [Header("Tilemap References")]
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private Tilemap _objectTilemap;
        
        [Header("Tile Assets")]
        [SerializeField] private DungeonTile _floorTile;
        [SerializeField] private DungeonTile _wallTile;
        [SerializeField] private DungeonTile _doorTile;
        [SerializeField] private DungeonTile _stairsUpTile;
        [SerializeField] private DungeonTile _stairsDownTile;
        
        private Dictionary<Vector3Int, TileType> _tileData;
        private Grid _grid;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _grid = GetComponent<Grid>();
            _tileData = new Dictionary<Vector3Int, TileType>();
            
            if (_grid == null)
            {
                Debug.LogError("Grid component not found on TilemapController!");
            }
            
            ValidateTilemapReferences();
        }
        #endregion

        #region Public Methods
        public void SetTile(Vector3Int position, TileType tileType)
        {
            DungeonTile tileToPlace = GetTileAsset(tileType);
            if (tileToPlace == null) return;
            
            Tilemap targetTilemap = GetTargetTilemap(tileType);
            targetTilemap.SetTile(position, tileToPlace);
            _tileData[position] = tileType;
        }
        
        public void SetTile(int x, int y, TileType tileType)
        {
            SetTile(new Vector3Int(x, y, 0), tileType);
        }
        
        public TileType GetTileType(Vector3Int position)
        {
            if (_tileData.TryGetValue(position, out TileType type))
            {
                return type;
            }
            return TileType.None;
        }
        
        public TileType GetTileType(int x, int y)
        {
            return GetTileType(new Vector3Int(x, y, 0));
        }
        
        public bool IsWalkable(Vector3Int position)
        {
            TileType type = GetTileType(position);
            switch (type)
            {
                case TileType.Floor:
                case TileType.Door:
                case TileType.StairsUp:
                case TileType.StairsDown:
                    return true;
                case TileType.Wall:
                case TileType.None:
                case TileType.Pit:
                    return false;
                default:
                    return true;
            }
        }
        
        public bool IsWalkable(int x, int y)
        {
            return IsWalkable(new Vector3Int(x, y, 0));
        }
        
        public void ClearTile(Vector3Int position)
        {
            _floorTilemap.SetTile(position, null);
            _wallTilemap.SetTile(position, null);
            _objectTilemap.SetTile(position, null);
            _tileData.Remove(position);
        }
        
        public void ClearAllTiles()
        {
            _floorTilemap.CompressBounds();
            _wallTilemap.CompressBounds();
            _objectTilemap.CompressBounds();
            
            BoundsInt bounds = _floorTilemap.cellBounds;
            BoundsInt wallBounds = _wallTilemap.cellBounds;
            BoundsInt objectBounds = _objectTilemap.cellBounds;
            
            bounds.min = Vector3Int.Min(bounds.min, Vector3Int.Min(wallBounds.min, objectBounds.min));
            bounds.max = Vector3Int.Max(bounds.max, Vector3Int.Max(wallBounds.max, objectBounds.max));
            
            _floorTilemap.FloodFill(bounds.min, null);
            _wallTilemap.FloodFill(bounds.min, null);
            _objectTilemap.FloodFill(bounds.min, null);
            
            _tileData.Clear();
        }
        
        public Vector3 GetWorldPosition(Vector3Int cellPosition)
        {
            return _grid.CellToWorld(cellPosition) + _grid.cellSize * 0.5f;
        }
        
        public Vector3Int GetCellPosition(Vector3 worldPosition)
        {
            return _grid.WorldToCell(worldPosition);
        }
        
        public void CreateRoom(int x, int y, int width, int height)
        {
            // Create floor
            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    SetTile(i, j, TileType.Floor);
                }
            }
            
            // Create walls
            for (int i = x - 1; i <= x + width; i++)
            {
                SetTile(i, y - 1, TileType.Wall);
                SetTile(i, y + height, TileType.Wall);
            }
            
            for (int j = y; j < y + height; j++)
            {
                SetTile(x - 1, j, TileType.Wall);
                SetTile(x + width, j, TileType.Wall);
            }
        }
        #endregion

        #region Private Methods
        private void ValidateTilemapReferences()
        {
            if (_floorTilemap == null)
                Debug.LogWarning("Floor Tilemap is not assigned!");
            if (_wallTilemap == null)
                Debug.LogWarning("Wall Tilemap is not assigned!");
            if (_objectTilemap == null)
                Debug.LogWarning("Object Tilemap is not assigned!");
        }
        
        private DungeonTile GetTileAsset(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Floor:
                    return _floorTile;
                case TileType.Wall:
                case TileType.SecretWall:
                    return _wallTile;
                case TileType.Door:
                    return _doorTile;
                case TileType.StairsUp:
                    return _stairsUpTile;
                case TileType.StairsDown:
                    return _stairsDownTile;
                default:
                    return null;
            }
        }
        
        private Tilemap GetTargetTilemap(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Floor:
                case TileType.Water:
                case TileType.Pit:
                    return _floorTilemap;
                case TileType.Wall:
                case TileType.SecretWall:
                    return _wallTilemap;
                case TileType.Door:
                case TileType.StairsUp:
                case TileType.StairsDown:
                case TileType.Chest:
                case TileType.Trap:
                    return _objectTilemap;
                default:
                    return _floorTilemap;
            }
        }
        #endregion
    }
}