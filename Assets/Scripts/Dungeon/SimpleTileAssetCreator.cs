using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dungeon.Dungeon
{
    public class SimpleTileAssetCreator : MonoBehaviour
    {
        #if UNITY_EDITOR
        [MenuItem("Tools/Create Simple Tile Assets")]
        public static void CreateTileAssets()
        {
            // Create tiles folder if it doesn't exist
            string tilePath = "Assets/Tiles";
            if (!AssetDatabase.IsValidFolder(tilePath))
            {
                AssetDatabase.CreateFolder("Assets", "Tiles");
            }
            
            // Create Floor Tile
            CreateColorTile("FloorTile", new Color(0.6f, 0.5f, 0.4f), tilePath);
            
            // Create Wall Tile
            CreateColorTile("WallTile", new Color(0.3f, 0.3f, 0.3f), tilePath);
            
            // Create Door Tile
            CreateColorTile("DoorTile", new Color(0.5f, 0.3f, 0.1f), tilePath);
            
            // Create Stairs Up Tile
            CreateColorTile("StairsUpTile", new Color(0.2f, 0.8f, 0.2f), tilePath);
            
            // Create Stairs Down Tile
            CreateColorTile("StairsDownTile", new Color(0.8f, 0.2f, 0.2f), tilePath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Simple tile assets created successfully in Assets/Tiles!");
        }
        
        private static void CreateColorTile(string name, Color color, string path)
        {
            // Create texture
            Texture2D texture = new Texture2D(32, 32);
            texture.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Save texture as asset
            string texturePath = $"{path}/{name}_Texture.png";
            byte[] pngData = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + texturePath.Replace("Assets", ""), pngData);
            AssetDatabase.ImportAsset(texturePath);
            
            // Load the imported texture
            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            
            // Create sprite from texture
            string spritePath = texturePath;
            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
            }
            
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            
            // Create Tile asset
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.white;
            
            string tilePath = $"{path}/{name}.asset";
            AssetDatabase.CreateAsset(tile, tilePath);
        }
        #endif
    }
}