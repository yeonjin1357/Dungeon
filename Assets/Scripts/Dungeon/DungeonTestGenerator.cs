using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon.Dungeon
{
    public class DungeonTestGenerator : MonoBehaviour
    {
        #region Fields
        [Header("Tilemap References")]
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;
        
        [Header("Test Tiles")]
        [SerializeField] private TileBase _testFloorTile;
        [SerializeField] private TileBase _testWallTile;
        
        [Header("Test Settings")]
        [SerializeField] private bool _generateOnStart = true;
        [SerializeField] private int _roomWidth = 10;
        [SerializeField] private int _roomHeight = 8;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (_generateOnStart)
            {
                GenerateTestRoom();
            }
        }
        #endregion

        #region Public Methods
        [ContextMenu("Generate Test Room")]
        public void GenerateTestRoom()
        {
            if (_floorTilemap == null || _wallTilemap == null)
            {
                Debug.LogError("Tilemaps not assigned!");
                return;
            }
            
            ClearTilemaps();
            
            // Create floor
            for (int x = 0; x < _roomWidth; x++)
            {
                for (int y = 0; y < _roomHeight; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    if (_testFloorTile != null)
                    {
                        _floorTilemap.SetTile(position, _testFloorTile);
                    }
                    else
                    {
                        // Use color tile as fallback
                        _floorTilemap.SetTile(position, CreateColorTile(Color.gray));
                    }
                }
            }
            
            // Create walls around the room
            for (int x = -1; x <= _roomWidth; x++)
            {
                SetWallTile(x, -1);
                SetWallTile(x, _roomHeight);
            }
            
            for (int y = 0; y < _roomHeight; y++)
            {
                SetWallTile(-1, y);
                SetWallTile(_roomWidth, y);
            }
            
            Debug.Log($"Test room generated: {_roomWidth}x{_roomHeight}");
        }
        
        [ContextMenu("Clear Tilemaps")]
        public void ClearTilemaps()
        {
            if (_floorTilemap != null)
                _floorTilemap.CompressBounds();
            if (_floorTilemap != null)
                _floorTilemap.CompressBounds();
            if (_wallTilemap != null)
                _wallTilemap.CompressBounds();
                
            BoundsInt bounds = new BoundsInt(-20, -20, 0, 40, 40, 1);
            
            if (_floorTilemap != null)
            {
                var positions = new Vector3Int[bounds.size.x * bounds.size.y * bounds.size.z];
                var tiles = new TileBase[positions.Length];
                int index = 0;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        for (int z = bounds.zMin; z < bounds.zMax; z++)
                        {
                            positions[index] = new Vector3Int(x, y, z);
                            tiles[index] = null;
                            index++;
                        }
                    }
                }
                _floorTilemap.SetTiles(positions, tiles);
            }
            
            if (_wallTilemap != null)
            {
                var positions = new Vector3Int[bounds.size.x * bounds.size.y * bounds.size.z];
                var tiles = new TileBase[positions.Length];
                int index = 0;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        for (int z = bounds.zMin; z < bounds.zMax; z++)
                        {
                            positions[index] = new Vector3Int(x, y, z);
                            tiles[index] = null;
                            index++;
                        }
                    }
                }
                _wallTilemap.SetTiles(positions, tiles);
            }
        }
        #endregion

        #region Private Methods
        private void SetWallTile(int x, int y)
        {
            Vector3Int position = new Vector3Int(x, y, 0);
            if (_testWallTile != null)
            {
                _wallTilemap.SetTile(position, _testWallTile);
            }
            else
            {
                // Use color tile as fallback
                _wallTilemap.SetTile(position, CreateColorTile(Color.black));
            }
        }
        
        private TileBase CreateColorTile(Color color)
        {
            // Create a simple colored tile for testing
            var tile = ScriptableObject.CreateInstance<Tile>();
            
            // Create a 1x1 white texture
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            
            // Create sprite from texture
            tile.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
            tile.color = Color.white;
            
            return tile;
        }
        #endregion
    }
}