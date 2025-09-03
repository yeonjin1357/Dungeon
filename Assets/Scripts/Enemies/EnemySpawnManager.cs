using System.Collections.Generic;
using UnityEngine;
using Dungeon.Dungeon;
using System.Linq;
using Dungeon.Player;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 던전 생성 시 적을 자동으로 스폰하는 매니저
    /// </summary>
    public class EnemySpawnManager : MonoBehaviour
    {
        #region Singleton
        private static EnemySpawnManager _instance;
        public static EnemySpawnManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EnemySpawnManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EnemySpawnManager");
                        _instance = go.AddComponent<EnemySpawnManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion
        
        #region Fields
        [Header("Spawn Settings")]
        [SerializeField] private int _minEnemiesPerRoom = 1;
        [SerializeField] private int _maxEnemiesPerRoom = 4;
        [SerializeField] private float _spawnDelayBetweenEnemies = 0.1f;
        
        [Header("Floor Configuration")]
        [SerializeField] private List<FloorEnemyPool> _floorEnemyPools = new List<FloorEnemyPool>();
        
        [Header("Special Room Rules")]
        [SerializeField] private bool _spawnInBossRoom = false;
        [SerializeField] private bool _spawnInShopRoom = false;
        [SerializeField] private bool _spawnInStartRoom = false;
        
        private int _currentFloor = 1;
        private List<GameObject> _spawnedEnemies = new List<GameObject>();
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// 던전 생성 완료 후 적 스폰
        /// </summary>
        public void SpawnEnemiesForDungeon(List<Room> rooms, int floorNumber)
        {
            _currentFloor = floorNumber;
            ClearAllEnemies();
            
            FloorEnemyPool currentPool = GetFloorEnemyPool(floorNumber);
            if (currentPool == null)
            {
                Debug.LogWarning($"No enemy pool configured for floor {floorNumber}");
                return;
            }
            
            foreach (Room room in rooms)
            {
                if (ShouldSpawnInRoom(room))
                {
                    SpawnEnemiesInRoom(room, currentPool);
                }
            }
        }
        
        /// <summary>
        /// 특정 방에 적 스폰
        /// </summary>
        public void SpawnEnemiesInRoom(Room room, FloorEnemyPool enemyPool = null)
        {
            if (enemyPool == null)
            {
                enemyPool = GetFloorEnemyPool(_currentFloor);
            }
            
            if (enemyPool == null || enemyPool.enemyPrefabs.Count == 0)
            {
                Debug.LogWarning("No enemies available to spawn");
                return;
            }
            
            int enemyCount = Random.Range(_minEnemiesPerRoom, _maxEnemiesPerRoom + 1);
            List<Vector2Int> availablePositions = GetAvailableSpawnPositions(room);
            
            // 방 크기에 따라 적 수 조정
            enemyCount = Mathf.Min(enemyCount, availablePositions.Count);
            enemyCount = Mathf.Min(enemyCount, Mathf.Max(1, room.bounds.width * room.bounds.height / 20));
            
            for (int i = 0; i < enemyCount; i++)
            {
                if (availablePositions.Count == 0) break;
                
                // 가중치 기반 랜덤 선택
                GameObject enemyPrefab = SelectEnemyByWeight(enemyPool);
                if (enemyPrefab == null) continue;
                
                // 랜덤 위치 선택
                int posIndex = Random.Range(0, availablePositions.Count);
                Vector2Int spawnPos = availablePositions[posIndex];
                availablePositions.RemoveAt(posIndex);
                
                // 적 생성
                SpawnEnemy(enemyPrefab, spawnPos);
            }
        }
        
        /// <summary>
        /// 모든 적 제거
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (GameObject enemy in _spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            _spawnedEnemies.Clear();
        }
        
        /// <summary>
        /// 현재 층 설정
        /// </summary>
        public void SetCurrentFloor(int floor)
        {
            _currentFloor = floor;
        }
        #endregion
        
        #region Private Methods
        private FloorEnemyPool GetFloorEnemyPool(int floor)
        {
            // 정확한 층 찾기
            FloorEnemyPool exactMatch = _floorEnemyPools.FirstOrDefault(p => p.floorNumber == floor);
            if (exactMatch != null) return exactMatch;
            
            // 없으면 가장 가까운 낮은 층 찾기
            FloorEnemyPool closestLower = _floorEnemyPools
                .Where(p => p.floorNumber < floor)
                .OrderByDescending(p => p.floorNumber)
                .FirstOrDefault();
                
            if (closestLower != null) return closestLower;
            
            // 그것도 없으면 첫 번째 풀 반환
            return _floorEnemyPools.FirstOrDefault();
        }
        
        private bool ShouldSpawnInRoom(Room room)
        {
            // 시작 방 체크
            if (room.roomType == RoomType.Start && !_spawnInStartRoom)
                return false;
            
            // 보스 방 체크
            if (room.roomType == RoomType.Boss && !_spawnInBossRoom)
                return false;
            
            // 상점 체크
            if (room.roomType == RoomType.Shop && !_spawnInShopRoom)
                return false;
            
            // 너무 작은 방 체크 (3x3 이하)
            if (room.bounds.width <= 3 || room.bounds.height <= 3)
                return false;
            
            return true;
        }
        
        private List<Vector2Int> GetAvailableSpawnPositions(Room room)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            
            // 방의 중앙 영역에만 스폰 (벽에서 1칸 떨어진 곳)
            for (int x = room.bounds.x + 1; x < room.bounds.x + room.bounds.width - 1; x++)
            {
                for (int y = room.bounds.y + 1; y < room.bounds.y + room.bounds.height - 1; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    // 플레이어 시작 위치 근처는 제외
                    if (Vector2Int.Distance(pos, Vector2Int.zero) < 3f)
                        continue;
                    
                    // 출구 근처는 제외
                    // TODO: 출구 위치 체크
                    
                    positions.Add(pos);
                }
            }
            
            return positions;
        }
        
        private GameObject SelectEnemyByWeight(FloorEnemyPool pool)
        {
            if (pool.enemyPrefabs == null || pool.enemyPrefabs.Count == 0)
                return null;
            
            // 가중치 총합 계산
            float totalWeight = pool.enemyPrefabs.Sum(e => e.spawnWeight);
            float randomValue = Random.Range(0f, totalWeight);
            
            float currentWeight = 0;
            foreach (var enemyConfig in pool.enemyPrefabs)
            {
                currentWeight += enemyConfig.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return enemyConfig.enemyPrefab;
                }
            }
            
            // 안전장치: 마지막 적 반환
            return pool.enemyPrefabs[pool.enemyPrefabs.Count - 1].enemyPrefab;
        }
        
        private void SpawnEnemy(GameObject prefab, Vector2Int position)
        {
            if (prefab == null) return;
            
            Vector3 worldPos = new Vector3(position.x, position.y, 0);
            GameObject enemy = Instantiate(prefab, worldPos, Quaternion.identity);
            
            // GridPosition 설정
            GridPosition gridPos = enemy.GetComponent<GridPosition>();
            if (gridPos != null)
            {
                gridPos.SetGridPosition(position);
            }
            
            _spawnedEnemies.Add(enemy);
            
            Debug.Log($"Spawned {enemy.name} at {position}");
        }
        #endregion
    }
    
    /// <summary>
    /// 층별 적 풀 설정
    /// </summary>
    [System.Serializable]
    public class FloorEnemyPool
    {
        public int floorNumber = 1;
        public string floorName = "Floor 1";
        public List<EnemySpawnConfig> enemyPrefabs = new List<EnemySpawnConfig>();
        
        [Header("Difficulty Settings")]
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public int maxEliteCount = 0;  // 정예 몬스터 최대 수
    }
    
    /// <summary>
    /// 적 스폰 설정
    /// </summary>
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public GameObject enemyPrefab;
        [Range(0f, 100f)]
        public float spawnWeight = 50f;  // 스폰 가중치 (높을수록 자주 나옴)
        public bool isElite = false;     // 정예 몬스터 여부
        public bool isBoss = false;      // 보스 여부
    }
}