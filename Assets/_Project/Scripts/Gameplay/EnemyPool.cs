using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人对象池。
/// 开局一次性创建 prewarmCount 个敌人并隐藏，之后靠 Get(data) 取用。
/// "归还"由敌人自己 SetActive(false) 完成，池子通过 activeSelf 判断谁在待命。
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public PathFollower enemyPrefab;
    public Transform[] waypoints;

    [Tooltip("开局预先创建多少个，按同时在场敌人的峰值来估")]
    public int prewarmCount = 50;

    readonly List<PathFollower> pool = new List<PathFollower>();

    void Awake()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[Pool] enemyPrefab 没有指定，池子建不起来");
            return;
        }

        for (int i = 0; i < prewarmCount; i++)
        {
            PathFollower e = Instantiate(enemyPrefab, transform);
            e.gameObject.SetActive(false);
            pool.Add(e);
        }

        Debug.Log($"[Pool] 预热完成，共 {pool.Count} 个，内存 {System.GC.GetTotalMemory(false) / 1024} KB");
    }

    /// <summary>
    /// 场上还活着的敌人数量。
    /// 池子用 activeSelf 判断闲忙，所以"没被隐藏的"就是在场上的。
    /// </summary>
    public int ActiveCount
    {
        get
        {
            int n = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].gameObject.activeSelf) n++;
            }
            return n;
        }
    }

    /// <summary>
    /// 从池里取一个闲置的敌人，装上指定数值，放到起点并激活。
    /// 数值走参数而不是字段：漏传会编译不过，而不是运行时静默用错。
    /// </summary>
    public PathFollower Get(EnemyData data)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            PathFollower e = pool[i];
            if (e.gameObject.activeSelf) continue;   // 正在场上跑，跳过

            Activate(e, data);
            return e;
        }

        // 池子不够用：临时扩建。出现这条日志说明 prewarmCount 估小了
        Debug.LogWarning("[Pool] 池子不够用，临时新建一个");
        PathFollower extra = Instantiate(enemyPrefab, transform);
        pool.Add(extra);
        Activate(extra, data);
        return extra;
    }

    void Activate(PathFollower e, EnemyData data)
    {
        e.ApplyData(data);            // 先装数值，再激活（OnEnable 里要用 maxHp）
        e.waypoints = waypoints;
        if (waypoints != null && waypoints.Length > 0)
        {
            e.transform.position = waypoints[0].position;
        }
        e.gameObject.SetActive(true);
    }
}
