using System;

// 谁也不认识的广播站：只负责转发，不负责处理。
// static = 不需要挂到物体上，全场景共用一个。
public static class GameEvents
{
    public static event Action OnEnemyDied;

    public static void RaiseEnemyDied()
    {
        OnEnemyDied?.Invoke();
    }
}
