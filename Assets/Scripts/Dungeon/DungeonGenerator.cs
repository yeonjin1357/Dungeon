using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon.Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        #region Fields
        [Header("Dungeon Settings")]
        [SerializeField] private int _dungeonWidth = 80;
        [SerializeField] private int _dungeonHeight = 80;
        [SerializeField] private int _minNodeSize = 10;
        [SerializeField] private int _maxSplitIterations = 5;
        
        [Header("Tilemap References")]
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private Tilemap _objectTilemap;
        
        [Header("Tile References")]
        [SerializeField] private TileBase _floorTile;
        [SerializeField] private TileBase _wallTile;
        [SerializeField] private TileBase _doorTile;
        [SerializeField] private TileBase _stairUpTile;
        [SerializeField] private TileBase _stairDownTile;
        
        private BSPNode _rootNode;
        private List<Room> _rooms = new List<Room>();
        private int _currentFloor = 1;
        private List<Vector2Int> _corridors = new List<Vector2Int>();
        
        private Vector2Int _playerStartPosition;
        private Vector2Int _exitPosition;
        
        public Vector2Int PlayerStartPosition => _playerStartPosition;
        public Vector2Int ExitPosition => _exitPosition;
        public List<Room> Rooms => _rooms;
        public int CurrentFloor => _currentFloor;
        #endregion
        
        #region Unity Lifecycle
        private void Start()
        {
            GenerateDungeon(1); // 기본적으로 1층부터 시작
        }
        #endregion
        
        #region Public Methods
        [ContextMenu("Generate New Dungeon")]
        public void GenerateDungeon()
        {
            GenerateDungeon(_currentFloor);
        }
        
        public void GenerateDungeon(int floorNumber)
        {
            _currentFloor = floorNumber;
            ClearDungeon();
            CreateBSPTree();
            CreateRooms();
            CreateCorridors();
            PlaceOnTilemap();
            PlaceSpecialTiles();
            
            // 적 스폰
            SpawnEnemies();
        }
        
        public void GenerateDungeonAsync(System.Action onComplete = null)
        {
            StartCoroutine(GenerateDungeonCoroutine(onComplete));
        }
        
        public bool IsWalkable(Vector2Int position)
        {
            Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);
            return _floorTilemap.HasTile(tilePosition) && !_wallTilemap.HasTile(tilePosition);
        }
        #endregion
        
        #region Private Methods
        private void ClearDungeon()
        {
            _rooms.Clear();
            _corridors.Clear();
            
            if (_floorTilemap != null)
                _floorTilemap.CompressBounds();
            
            if (_wallTilemap != null)
                _wallTilemap.CompressBounds();
            
            if (_objectTilemap != null)
                _objectTilemap.CompressBounds();
        }
        
        private void CreateBSPTree()
        {
            _rootNode = new BSPNode(0, 0, _dungeonWidth, _dungeonHeight);
            
            Queue<BSPNode> nodesToSplit = new Queue<BSPNode>();
            nodesToSplit.Enqueue(_rootNode);
            
            int iterations = 0;
            while (nodesToSplit.Count > 0 && iterations < _maxSplitIterations)
            {
                int nodesInThisIteration = nodesToSplit.Count;
                
                for (int i = 0; i < nodesInThisIteration; i++)
                {
                    BSPNode node = nodesToSplit.Dequeue();
                    
                    if (node.Split(_minNodeSize))
                    {
                        nodesToSplit.Enqueue(node.LeftChild);
                        nodesToSplit.Enqueue(node.RightChild);
                    }
                }
                
                iterations++;
            }
        }
        
        private void CreateRooms()
        {
            List<BSPNode> leaves = GetLeafNodes(_rootNode);
            
            for (int i = 0; i < leaves.Count; i++)
            {
                BSPNode leaf = leaves[i];
                leaf.CreateRoom();
                RectInt roomBounds = leaf.GetRoom();
                
                if (roomBounds.width > 0 && roomBounds.height > 0)
                {
                    // Room 객체 생성
                    RoomType type = RoomType.Normal;
                    
                    // 첫 번째 방은 시작 방
                    if (i == 0)
                        type = RoomType.Start;
                    // 마지막 방은 출구 방
                    else if (i == leaves.Count - 1)
                        type = RoomType.Exit;
                    // 10% 확률로 특별한 방
                    else if (Random.Range(0f, 1f) < 0.1f)
                    {
                        float rand = Random.Range(0f, 1f);
                        if (rand < 0.3f && _currentFloor > 1)
                            type = RoomType.Shop;
                        else if (rand < 0.6f)
                            type = RoomType.Treasure;
                        else
                            type = RoomType.Rest;
                    }
                    
                    Room room = new Room(roomBounds, type);
                    
                    // 바닥 타일 위치 저장
                    for (int x = roomBounds.xMin; x < roomBounds.xMax; x++)
                    {
                        for (int y = roomBounds.yMin; y < roomBounds.yMax; y++)
                        {
                            room.AddFloorTile(new Vector2Int(x, y));
                        }
                    }
                    
                    _rooms.Add(room);
                }
            }
        }
        
        private List<BSPNode> GetLeafNodes(BSPNode node)
        {
            List<BSPNode> leaves = new List<BSPNode>();
            
            if (node.IsLeaf)
            {
                leaves.Add(node);
            }
            else
            {
                if (node.LeftChild != null)
                    leaves.AddRange(GetLeafNodes(node.LeftChild));
                
                if (node.RightChild != null)
                    leaves.AddRange(GetLeafNodes(node.RightChild));
            }
            
            return leaves;
        }
        
        private void CreateCorridors()
        {
            CreateCorridorsFromNode(_rootNode);
        }
        
        private void CreateCorridorsFromNode(BSPNode node)
        {
            if (node.LeftChild == null || node.RightChild == null)
                return;
            
            RectInt leftRoom = node.LeftChild.GetRoom();
            RectInt rightRoom = node.RightChild.GetRoom();
            
            if (leftRoom.width > 0 && rightRoom.width > 0)
            {
                CreateCorridor(Vector2Int.RoundToInt(leftRoom.center), Vector2Int.RoundToInt(rightRoom.center));
            }
            
            CreateCorridorsFromNode(node.LeftChild);
            CreateCorridorsFromNode(node.RightChild);
        }
        
        private void CreateCorridor(Vector2Int start, Vector2Int end)
        {
            int x = start.x;
            int y = start.y;
            
            while (x != end.x)
            {
                _corridors.Add(new Vector2Int(x, y));
                x += (end.x > x) ? 1 : -1;
            }
            
            while (y != end.y)
            {
                _corridors.Add(new Vector2Int(x, y));
                y += (end.y > y) ? 1 : -1;
            }
            
            _corridors.Add(new Vector2Int(x, y));
        }
        
        private void PlaceOnTilemap()
        {
            // 전체를 벽으로 채우기
            for (int x = 0; x < _dungeonWidth; x++)
            {
                for (int y = 0; y < _dungeonHeight; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    _wallTilemap.SetTile(position, _wallTile);
                }
            }
            
            // 방 바닥 배치
            foreach (Room room in _rooms)
            {
                for (int x = room.bounds.xMin; x < room.bounds.xMax; x++)
                {
                    for (int y = room.bounds.yMin; y < room.bounds.yMax; y++)
                    {
                        Vector3Int position = new Vector3Int(x, y, 0);
                        _floorTilemap.SetTile(position, _floorTile);
                        _wallTilemap.SetTile(position, null);
                    }
                }
            }
            
            // 복도 배치
            foreach (Vector2Int corridor in _corridors)
            {
                Vector3Int position = new Vector3Int(corridor.x, corridor.y, 0);
                _floorTilemap.SetTile(position, _floorTile);
                _wallTilemap.SetTile(position, null);
                
                // 복도 주변 확장
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        
                        Vector3Int adjacentPos = new Vector3Int(corridor.x + dx, corridor.y + dy, 0);
                        if (!_floorTilemap.HasTile(adjacentPos))
                        {
                            _floorTilemap.SetTile(adjacentPos, _floorTile);
                            _wallTilemap.SetTile(adjacentPos, null);
                        }
                    }
                }
            }
            
            // 벽 정리
            for (int x = 0; x < _dungeonWidth; x++)
            {
                for (int y = 0; y < _dungeonHeight; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    
                    if (_floorTilemap.HasTile(position))
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                
                                Vector3Int adjacentPos = new Vector3Int(x + dx, y + dy, 0);
                                
                                if (adjacentPos.x < 0 || adjacentPos.x >= _dungeonWidth ||
                                    adjacentPos.y < 0 || adjacentPos.y >= _dungeonHeight)
                                    continue;
                                
                                if (!_floorTilemap.HasTile(adjacentPos) && _wallTilemap.HasTile(adjacentPos))
                                {
                                    _wallTilemap.SetTile(adjacentPos, _wallTile);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void PlaceSpecialTiles()
        {
            if (_rooms.Count < 2)
                return;
            
            // 시작 방 찾기
            Room startRoom = _rooms.Find(r => r.roomType == RoomType.Start) ?? _rooms[0];
            _playerStartPosition = startRoom.centerPosition;
            Vector3Int startPos = new Vector3Int(_playerStartPosition.x, _playerStartPosition.y, 0);
            
            if (_stairUpTile != null && _currentFloor > 1)
            {
                _objectTilemap.SetTile(startPos, _stairUpTile);
            }
            
            // 출구 방 찾기
            Room exitRoom = _rooms.Find(r => r.roomType == RoomType.Exit) ?? _rooms[_rooms.Count - 1];
            _exitPosition = exitRoom.centerPosition;
            Vector3Int exitPos = new Vector3Int(_exitPosition.x, _exitPosition.y, 0);
            
            if (_stairDownTile != null)
            {
                _objectTilemap.SetTile(exitPos, _stairDownTile);
            }
        }
        
        private void SpawnEnemies()
        {
            if (Enemies.EnemySpawnManager.Instance != null)
            {
                Enemies.EnemySpawnManager.Instance.SpawnEnemiesForDungeon(_rooms, _currentFloor);
            }
            else
            {
                Debug.LogWarning("EnemySpawnManager not found! Enemies will not spawn.");
            }
        }
        
        private IEnumerator GenerateDungeonCoroutine(System.Action onComplete)
        {
            ClearDungeon();
            yield return null;
            
            CreateBSPTree();
            yield return null;
            
            CreateRooms();
            yield return null;
            
            CreateCorridors();
            yield return null;
            
            PlaceOnTilemap();
            yield return null;
            
            PlaceSpecialTiles();
            yield return null;
            
            onComplete?.Invoke();
        }
        #endregion
        
        #region Gizmos
        private void OnDrawGizmos()
        {
            if (_rooms == null || _rooms.Count == 0)
                return;
            
            Gizmos.color = Color.green;
            foreach (Room room in _rooms)
            {
                Vector3 bottomLeft = new Vector3(room.bounds.xMin, room.bounds.yMin, 0);
                Vector3 bottomRight = new Vector3(room.bounds.xMax, room.bounds.yMin, 0);
                Vector3 topLeft = new Vector3(room.bounds.xMin, room.bounds.yMax, 0);
                Vector3 topRight = new Vector3(room.bounds.xMax, room.bounds.yMax, 0);
                
                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
            }
            
            Gizmos.color = Color.yellow;
            foreach (Vector2Int corridor in _corridors)
            {
                Vector3 pos = new Vector3(corridor.x + 0.5f, corridor.y + 0.5f, 0);
                Gizmos.DrawWireCube(pos, Vector3.one * 0.8f);
            }
            
            if (_playerStartPosition != Vector2Int.zero)
            {
                Gizmos.color = Color.blue;
                Vector3 startPos = new Vector3(_playerStartPosition.x + 0.5f, _playerStartPosition.y + 0.5f, 0);
                Gizmos.DrawWireSphere(startPos, 0.5f);
            }
            
            if (_exitPosition != Vector2Int.zero)
            {
                Gizmos.color = Color.red;
                Vector3 exitPos = new Vector3(_exitPosition.x + 0.5f, _exitPosition.y + 0.5f, 0);
                Gizmos.DrawWireSphere(exitPos, 0.5f);
            }
        }
        #endregion
    }
}