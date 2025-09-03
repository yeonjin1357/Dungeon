using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon.Dungeon
{
    [CreateAssetMenu(fileName = "ColorTile", menuName = "Tiles/ColorTile")]
    public class ColorTile : TileBase
    {
        [SerializeField] private Color _color = Color.white;
        
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = CreateSprite(_color);
            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = Tile.ColliderType.Sprite;
        }
        
        private Sprite CreateSprite(Color color)
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
        }
    }
}