using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon.Dungeon
{
    public class DungeonSetup : MonoBehaviour
    {
        public static void SetupDungeonGenerator()
        {
            GameObject grid = GameObject.Find("Grid");
            if (grid == null)
            {
                Debug.LogError("Grid GameObject not found!");
                return;
            }
            
            DungeonGenerator generator = grid.GetComponent<DungeonGenerator>();
            if (generator == null)
            {
                generator = grid.AddComponent<DungeonGenerator>();
            }
            
            // Find and assign Tilemaps
            Transform floorTransform = grid.transform.Find("Floor");
            Transform wallsTransform = grid.transform.Find("Walls");
            Transform objectsTransform = grid.transform.Find("Objects");
            
            if (floorTransform != null)
            {
                Tilemap floorTilemap = floorTransform.GetComponent<Tilemap>();
                if (floorTilemap == null)
                {
                    floorTilemap = floorTransform.gameObject.AddComponent<Tilemap>();
                    floorTransform.gameObject.AddComponent<TilemapRenderer>();
                }
                generator.GetType().GetField("_floorTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(generator, floorTilemap);
            }
            
            if (wallsTransform != null)
            {
                Tilemap wallsTilemap = wallsTransform.GetComponent<Tilemap>();
                if (wallsTilemap == null)
                {
                    wallsTilemap = wallsTransform.gameObject.AddComponent<Tilemap>();
                    wallsTransform.gameObject.AddComponent<TilemapRenderer>();
                }
                generator.GetType().GetField("_wallTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(generator, wallsTilemap);
            }
            
            if (objectsTransform != null)
            {
                Tilemap objectsTilemap = objectsTransform.GetComponent<Tilemap>();
                if (objectsTilemap == null)
                {
                    objectsTilemap = objectsTransform.gameObject.AddComponent<Tilemap>();
                    objectsTransform.gameObject.AddComponent<TilemapRenderer>();
                }
                generator.GetType().GetField("_objectTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(generator, objectsTilemap);
            }
            
            // Create simple tiles programmatically
            CreateAndAssignTiles(generator);
            
            Debug.Log("DungeonGenerator setup completed!");
        }
        
        private static void CreateAndAssignTiles(DungeonGenerator generator)
        {
            // Create floor tile
            ColorTile floorTile = ScriptableObject.CreateInstance<ColorTile>();
            floorTile.GetType().GetField("_color", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(floorTile, new Color(0.6f, 0.5f, 0.4f));
            
            // Create wall tile
            ColorTile wallTile = ScriptableObject.CreateInstance<ColorTile>();
            wallTile.GetType().GetField("_color", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(wallTile, new Color(0.3f, 0.3f, 0.3f));
            
            // Create door tile
            ColorTile doorTile = ScriptableObject.CreateInstance<ColorTile>();
            doorTile.GetType().GetField("_color", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(doorTile, new Color(0.5f, 0.3f, 0.1f));
            
            // Create stairs up tile
            ColorTile stairsUpTile = ScriptableObject.CreateInstance<ColorTile>();
            stairsUpTile.GetType().GetField("_color", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(stairsUpTile, new Color(0.2f, 0.8f, 0.2f));
            
            // Create stairs down tile
            ColorTile stairsDownTile = ScriptableObject.CreateInstance<ColorTile>();
            stairsDownTile.GetType().GetField("_color", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(stairsDownTile, new Color(0.8f, 0.2f, 0.2f));
            
            // Assign tiles to generator
            generator.GetType().GetField("_floorTile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(generator, floorTile);
            generator.GetType().GetField("_wallTile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(generator, wallTile);
            generator.GetType().GetField("_doorTile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(generator, doorTile);
            generator.GetType().GetField("_stairUpTile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(generator, stairsUpTile);
            generator.GetType().GetField("_stairDownTile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(generator, stairsDownTile);
        }
        
        private void Start()
        {
            SetupDungeonGenerator();
        }
    }
}