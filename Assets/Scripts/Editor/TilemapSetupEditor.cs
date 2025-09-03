using UnityEngine;
using UnityEditor;
using Dungeon.Dungeon;

public class TilemapSetupEditor : Editor
{
    [MenuItem("Dungeon/Setup Tilemap System")]
    public static void SetupTilemapSystem()
    {
        GameObject gridObject = GameObject.Find("Grid");
        if (gridObject == null)
        {
            Debug.LogError("Grid GameObject not found!");
            return;
        }
        
        // Add SimpleTileGenerator component
        SimpleTileGenerator generator = gridObject.GetComponent<SimpleTileGenerator>();
        if (generator == null)
        {
            generator = gridObject.AddComponent<SimpleTileGenerator>();
            Debug.Log("SimpleTileGenerator component added to Grid");
        }
        else
        {
            Debug.Log("SimpleTileGenerator component already exists on Grid");
        }
        
        EditorUtility.SetDirty(gridObject);
    }
}