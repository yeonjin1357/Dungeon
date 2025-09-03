using UnityEngine;

namespace Dungeon.Dungeon
{
    public class BSPNode
    {
        #region Fields
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        public BSPNode LeftChild { get; private set; }
        public BSPNode RightChild { get; private set; }
        
        public RectInt Room { get; private set; }
        public Vector2Int RoomCenter => Vector2Int.RoundToInt(Room.center);
        
        private const int MIN_NODE_SIZE = 10;
        private const int MIN_ROOM_SIZE = 5;
        private const int MAX_ROOM_SIZE = 15;
        #endregion
        
        #region Constructor
        public BSPNode(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        #endregion
        
        #region Public Methods
        public bool IsLeaf => LeftChild == null && RightChild == null;
        
        public bool Split(int minNodeSize = MIN_NODE_SIZE)
        {
            if (LeftChild != null || RightChild != null)
                return false;
            
            bool splitHorizontal = Random.Range(0f, 1f) > 0.5f;
            
            if (Width > Height && Width / (float)Height >= 1.25f)
                splitHorizontal = false;
            else if (Height > Width && Height / (float)Width >= 1.25f)
                splitHorizontal = true;
            
            int maxSize = (splitHorizontal ? Height : Width) - minNodeSize;
            
            if (maxSize <= minNodeSize)
                return false;
            
            int splitPosition = Random.Range(minNodeSize, maxSize);
            
            if (splitHorizontal)
            {
                LeftChild = new BSPNode(X, Y, Width, splitPosition);
                RightChild = new BSPNode(X, Y + splitPosition, Width, Height - splitPosition);
            }
            else
            {
                LeftChild = new BSPNode(X, Y, splitPosition, Height);
                RightChild = new BSPNode(X + splitPosition, Y, Width - splitPosition, Height);
            }
            
            return true;
        }
        
        public void CreateRoom()
        {
            if (LeftChild != null || RightChild != null)
                return;
            
            int roomWidth = Random.Range(MIN_ROOM_SIZE, Mathf.Min(Width - 2, MAX_ROOM_SIZE));
            int roomHeight = Random.Range(MIN_ROOM_SIZE, Mathf.Min(Height - 2, MAX_ROOM_SIZE));
            
            int roomX = X + Random.Range(1, Width - roomWidth - 1);
            int roomY = Y + Random.Range(1, Height - roomHeight - 1);
            
            Room = new RectInt(roomX, roomY, roomWidth, roomHeight);
        }
        
        public RectInt GetRoom()
        {
            if (Room.width > 0 && Room.height > 0)
                return Room;
            
            RectInt leftRoom = new RectInt();
            RectInt rightRoom = new RectInt();
            
            if (LeftChild != null)
                leftRoom = LeftChild.GetRoom();
            
            if (RightChild != null)
                rightRoom = RightChild.GetRoom();
            
            if (leftRoom.width == 0 && leftRoom.height == 0)
                return rightRoom;
            else if (rightRoom.width == 0 && rightRoom.height == 0)
                return leftRoom;
            else if (Random.Range(0f, 1f) > 0.5f)
                return leftRoom;
            else
                return rightRoom;
        }
        #endregion
    }
}