using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon.Dungeon
{
    public class BasicTilemapTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private Color _floorColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _wallColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        private void Start()
        {
            Debug.Log("BasicTilemapTest starting...");
            CreateTestRoom();
        }
        
        private void CreateTestRoom()
        {
            // Find Grid
            GameObject gridObject = GameObject.Find("Grid");
            if (gridObject == null)
            {
                Debug.LogError("Grid not found!");
                return;
            }
            
            // Find tilemaps
            Transform floorTransform = gridObject.transform.Find("Floor");
            Transform wallsTransform = gridObject.transform.Find("Walls");
            
            if (floorTransform == null || wallsTransform == null)
            {
                Debug.LogError("Floor or Walls tilemap not found!");
                return;
            }
            
            Tilemap floorTilemap = floorTransform.GetComponent<Tilemap>();
            Tilemap wallsTilemap = wallsTransform.GetComponent<Tilemap>();
            
            // Create simple tiles
            Tile floorTile = CreateColorTile(_floorColor);
            Tile wallTile = CreateColorTile(_wallColor);
            wallTile.colliderType = Tile.ColliderType.Sprite;
            
            // Generate a simple room
            int roomWidth = 10;
            int roomHeight = 8;
            
            // Create floor
            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    floorTilemap.SetTile(pos, floorTile);
                }
            }
            
            // Create walls
            for (int x = -1; x <= roomWidth; x++)
            {
                wallsTilemap.SetTile(new Vector3Int(x, -1, 0), wallTile);
                wallsTilemap.SetTile(new Vector3Int(x, roomHeight, 0), wallTile);
            }
            
            for (int y = 0; y < roomHeight; y++)
            {
                wallsTilemap.SetTile(new Vector3Int(-1, y, 0), wallTile);
                wallsTilemap.SetTile(new Vector3Int(roomWidth, y, 0), wallTile);
            }
            
            Debug.Log($"Test room created: {roomWidth}x{roomHeight}");
        }
        
        private Tile CreateColorTile(Color color)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            
            // Create texture
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            
            // Create sprite
            tile.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
            
            return tile;
        }
    }
}