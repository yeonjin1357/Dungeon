namespace Dungeon.Enemies
{
    /// <summary>
    /// 적의 상태 열거형
    /// </summary>
    public enum EnemyState
    {
        Idle,       // 대기 상태
        Patrol,     // 순찰 상태
        Chase,      // 추적 상태
        Attack,     // 공격 상태
        Flee,       // 도주 상태
        Dead        // 죽음 상태
    }
}