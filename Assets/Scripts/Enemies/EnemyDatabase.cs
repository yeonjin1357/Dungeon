using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 적 데이터베이스 - 층별 몬스터 설정
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Dungeon/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        [Header("Floor Configurations")]
        [SerializeField] private List<FloorConfiguration> _floorConfigurations = new List<FloorConfiguration>();
        
        [Header("Boss Configurations")]
        [SerializeField] private List<BossConfiguration> _bossConfigurations = new List<BossConfiguration>();
        
        /// <summary>
        /// 특정 층의 적 설정 가져오기
        /// </summary>
        public FloorConfiguration GetFloorConfiguration(int floorNumber)
        {
            // 정확한 층 찾기
            FloorConfiguration exact = _floorConfigurations.FirstOrDefault(f => f.floorNumber == floorNumber);
            if (exact != null) return exact;
            
            // 없으면 가장 가까운 낮은 층 설정 사용
            FloorConfiguration closest = _floorConfigurations
                .Where(f => f.floorNumber < floorNumber)
                .OrderByDescending(f => f.floorNumber)
                .FirstOrDefault();
                
            if (closest != null) return closest;
            
            // 그것도 없으면 첫 번째 설정 반환
            return _floorConfigurations.FirstOrDefault();
        }
        
        /// <summary>
        /// 특정 층의 보스 가져오기
        /// </summary>
        public GameObject GetBossForFloor(int floorNumber)
        {
            BossConfiguration bossConfig = _bossConfigurations.FirstOrDefault(b => b.floorNumber == floorNumber);
            return bossConfig?.bossPrefab;
        }
        
        /// <summary>
        /// 층별 적 선택 (가중치 기반)
        /// </summary>
        public GameObject SelectEnemyForFloor(int floorNumber, bool allowElite = true, bool allowBoss = false)
        {
            FloorConfiguration config = GetFloorConfiguration(floorNumber);
            if (config == null || config.enemies.Count == 0)
                return null;
            
            // 필터링
            List<EnemyEntry> validEnemies = config.enemies.Where(e => 
                (allowElite || !e.isElite) && 
                (allowBoss || !e.isBoss)
            ).ToList();
            
            if (validEnemies.Count == 0)
                return null;
            
            // 가중치 기반 선택
            float totalWeight = validEnemies.Sum(e => e.GetAdjustedWeight(floorNumber));
            float randomValue = Random.Range(0f, totalWeight);
            
            float currentWeight = 0;
            foreach (var enemy in validEnemies)
            {
                currentWeight += enemy.GetAdjustedWeight(floorNumber);
                if (randomValue <= currentWeight)
                {
                    return enemy.enemyPrefab;
                }
            }
            
            // 안전장치
            return validEnemies[validEnemies.Count - 1].enemyPrefab;
        }
    }
    
    /// <summary>
    /// 층별 적 설정
    /// </summary>
    [System.Serializable]
    public class FloorConfiguration
    {
        [Header("Floor Info")]
        public int floorNumber = 1;
        public string floorName = "Sewers";
        public string description = "Dark and damp sewers filled with rats and slimes";
        
        [Header("Enemy Pool")]
        public List<EnemyEntry> enemies = new List<EnemyEntry>();
        
        [Header("Spawn Settings")]
        [Range(1, 10)]
        public int minEnemiesPerRoom = 1;
        [Range(1, 20)]
        public int maxEnemiesPerRoom = 4;
        
        [Header("Difficulty Modifiers")]
        [Range(0.5f, 3f)]
        public float healthMultiplier = 1f;
        [Range(0.5f, 3f)]
        public float damageMultiplier = 1f;
        [Range(0.5f, 2f)]
        public float speedMultiplier = 1f;
        
        [Header("Special Rules")]
        public bool allowEliteEnemies = false;
        public int maxElitesPerFloor = 0;
        public bool hasBossRoom = false;
    }
    
    /// <summary>
    /// 개별 적 엔트리
    /// </summary>
    [System.Serializable]
    public class EnemyEntry
    {
        [Header("Enemy Info")]
        public GameObject enemyPrefab;
        public string enemyName = "Enemy";
        
        [Header("Spawn Settings")]
        [Range(0f, 100f)]
        public float baseSpawnWeight = 50f;
        
        [Range(1, 100)]
        public int minFloor = 1;  // 최소 등장 층
        [Range(1, 100)]
        public int maxFloor = 100;  // 최대 등장 층
        
        [Header("Enemy Type")]
        public bool isElite = false;
        public bool isBoss = false;
        public bool isMiniBoss = false;
        
        [Header("Spawn Conditions")]
        public bool requiresDarkness = false;
        public bool requiresWater = false;
        public bool spawnInGroups = false;
        [Range(1, 5)]
        public int minGroupSize = 1;
        [Range(1, 10)]
        public int maxGroupSize = 1;
        
        /// <summary>
        /// 층에 따라 조정된 가중치 계산
        /// </summary>
        public float GetAdjustedWeight(int currentFloor)
        {
            // 층 범위 체크
            if (currentFloor < minFloor || currentFloor > maxFloor)
                return 0f;
            
            // 층이 올라갈수록 강한 몬스터의 가중치 증가
            float floorModifier = 1f;
            if (isElite)
            {
                // 엘리트는 높은 층에서 더 자주 나타남
                floorModifier = Mathf.Lerp(0.5f, 1.5f, (float)(currentFloor - minFloor) / (maxFloor - minFloor));
            }
            else
            {
                // 일반 몬스터는 적정 층에서 가장 자주 나타남
                int optimalFloor = (minFloor + maxFloor) / 2;
                float distance = Mathf.Abs(currentFloor - optimalFloor);
                floorModifier = 1f - (distance / (float)(maxFloor - minFloor)) * 0.5f;
            }
            
            return baseSpawnWeight * floorModifier;
        }
    }
    
    /// <summary>
    /// 보스 설정
    /// </summary>
    [System.Serializable]
    public class BossConfiguration
    {
        public int floorNumber = 5;
        public GameObject bossPrefab;
        public string bossName = "Floor Boss";
        public string bossDescription = "A powerful enemy guarding the floor";
        
        [Header("Boss Room Settings")]
        public bool requiresLargeRoom = true;
        public int minRoomSize = 10;
        public bool spawnMinions = false;
        public GameObject[] minionPrefabs;
    }
}