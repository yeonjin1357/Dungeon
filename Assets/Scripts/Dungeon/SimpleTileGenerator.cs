using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon.Dungeon
{
    [RequireComponent(typeof(Grid))]
    public class SimpleTileGenerator : MonoBehaviour
    {
        #region Fields
        private Tilemap _floorTilemap;
        private Tilemap _wallTilemap;
        private Tile _floorTile;
        private Tile _wallTile;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeTilemaps();
            CreateTestTiles();
        }

        private void Start()
        {
            GenerateSimpleRoom();
        }
        #endregion

        #region Private Methods
        private void InitializeTilemaps()
        {
            // Find tilemaps by name
            Transform floorTransform = transform.Find("Floor");
            Transform wallsTransform = transform.Find("Walls");
            
            if (floorTransform != null)
                _floorTilemap = floorTransform.GetComponent<Tilemap>();
            else
                Debug.LogError("Floor tilemap not found!");
                
            if (wallsTransform != null)
                _wallTilemap = wallsTransform.GetComponent<Tilemap>();
            else
                Debug.LogError("Walls tilemap not found!");
        }
        
        private void CreateTestTiles()
        {
            // Create floor tile (gray)
            _floorTile = ScriptableObject.CreateInstance<Tile>();
            Texture2D floorTexture = new Texture2D(32, 32);
            Color[] floorPixels = new Color[32 * 32];
            for (int i = 0; i < floorPixels.Length; i++)
            {
                floorPixels[i] = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            floorTexture.SetPixels(floorPixels);
            floorTexture.filterMode = FilterMode.Point;
            floorTexture.Apply();
            _floorTile.sprite = Sprite.Create(floorTexture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
            
            // Create wall tile (dark)
            _wallTile = ScriptableObject.CreateInstance<Tile>();
            Texture2D wallTexture = new Texture2D(32, 32);
            Color[] wallPixels = new Color[32 * 32];
            for (int i = 0; i < wallPixels.Length; i++)
            {
                wallPixels[i] = new Color(0.2f, 0.2f, 0.2f, 1f);
            }
            wallTexture.SetPixels(wallPixels);
            wallTexture.filterMode = FilterMode.Point;
            wallTexture.Apply();
            _wallTile.sprite = Sprite.Create(wallTexture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
            _wallTile.colliderType = Tile.ColliderType.Sprite;
        }
        
        private void GenerateSimpleRoom()
        {
            if (_floorTilemap == null || _wallTilemap == null)
            {
                Debug.LogError("Tilemaps not initialized!");
                return;
            }
            
            int roomWidth = 10;
            int roomHeight = 8;
            int startX = -roomWidth / 2;
            int startY = -roomHeight / 2;
            
            // Clear existing tiles
            _floorTilemap.CompressBounds();
            _wallTilemap.CompressBounds();
            
            // Generate floor
            for (int x = startX; x < startX + roomWidth; x++)
            {
                for (int y = startY; y < startY + roomHeight; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    _floorTilemap.SetTile(position, _floorTile);
                }
            }
            
            // Generate walls
            for (int x = startX - 1; x <= startX + roomWidth; x++)
            {
                _wallTilemap.SetTile(new Vector3Int(x, startY - 1, 0), _wallTile);
                _wallTilemap.SetTile(new Vector3Int(x, startY + roomHeight, 0), _wallTile);
            }
            
            for (int y = startY; y < startY + roomHeight; y++)
            {
                _wallTilemap.SetTile(new Vector3Int(startX - 1, y, 0), _wallTile);
                _wallTilemap.SetTile(new Vector3Int(startX + roomWidth, y, 0), _wallTile);
            }
            
            Debug.Log($"Simple room generated at ({startX}, {startY}) with size {roomWidth}x{roomHeight}");
        }
        #endregion
    }
}