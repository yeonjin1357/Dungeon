using UnityEngine;

namespace Dungeon.Combat
{
    public interface ITurnBased
    {
        bool IsActive { get; }
        int TurnPriority { get; }
        Vector2Int GridPosition { get; }
        
        void ExecuteTurn();
        void OnTurnStart();
        void OnTurnEnd();
    }
}