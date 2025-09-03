using UnityEngine;
using System.Collections.Generic;

namespace Dungeon.Dungeon
{
    /// <summary>
    /// 던전 방 정보
    /// </summary>
    [System.Serializable]
    public class Room
    {
        public RectInt bounds;
        public RoomType roomType;
        public List<Vector2Int> floorTiles;
        public List<Vector2Int> doorPositions;
        public Vector2Int centerPosition;
        public bool isVisited;
        public int roomId;
        
        public Room(RectInt bounds, RoomType type = RoomType.Normal)
        {
            this.bounds = bounds;
            this.roomType = type;
            this.floorTiles = new List<Vector2Int>();
            this.doorPositions = new List<Vector2Int>();
            this.centerPosition = new Vector2Int(
                bounds.x + bounds.width / 2,
                bounds.y + bounds.height / 2
            );
            this.isVisited = false;
            this.roomId = GetHashCode();
        }
        
        /// <summary>
        /// 방에 바닥 타일 추가
        /// </summary>
        public void AddFloorTile(Vector2Int position)
        {
            if (!floorTiles.Contains(position))
            {
                floorTiles.Add(position);
            }
        }
        
        /// <summary>
        /// 방에 문 위치 추가
        /// </summary>
        public void AddDoor(Vector2Int position)
        {
            if (!doorPositions.Contains(position))
            {
                doorPositions.Add(position);
            }
        }
        
        /// <summary>
        /// 특정 위치가 방 안에 있는지 확인
        /// </summary>
        public bool ContainsPosition(Vector2Int position)
        {
            return position.x >= bounds.x && 
                   position.x < bounds.x + bounds.width &&
                   position.y >= bounds.y && 
                   position.y < bounds.y + bounds.height;
        }
        
        /// <summary>
        /// 방의 크기 계산
        /// </summary>
        public int GetArea()
        {
            return bounds.width * bounds.height;
        }
        
        /// <summary>
        /// 방의 둘레 계산
        /// </summary>
        public int GetPerimeter()
        {
            return 2 * (bounds.width + bounds.height);
        }
    }
    
    /// <summary>
    /// 방 타입
    /// </summary>
    public enum RoomType
    {
        Normal,     // 일반 방
        Start,      // 시작 방
        Exit,       // 출구 방
        Boss,       // 보스 방
        Shop,       // 상점
        Treasure,   // 보물 방
        Secret,     // 비밀 방
        Rest        // 휴식 방
    }
}