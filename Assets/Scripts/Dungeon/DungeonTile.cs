using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonTile", menuName = "Dungeon/Tile")]
    public class DungeonTile : TileBase
    {
        #region Fields
        [Header("Tile Properties")]
        [SerializeField] private TileType _tileType;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private bool _isWalkable = true;
        [SerializeField] private bool _blocksSight = false;
        [SerializeField] private bool _isInteractable = false;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _tintColor = Color.white;
        #endregion

        #region Properties
        public TileType Type => _tileType;
        public bool IsWalkable => _isWalkable;
        public bool BlocksSight => _blocksSight;
        public bool IsInteractable => _isInteractable;
        #endregion

        #region TileBase Overrides
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = _sprite;
            tileData.color = _tintColor;
            tileData.transform = Matrix4x4.identity;
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = _isWalkable ? Tile.ColliderType.None : Tile.ColliderType.Sprite;
        }

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            return base.StartUp(position, tilemap, go);
        }
        #endregion
    }
}