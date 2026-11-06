using System;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyPool pool;

    [Header("敌人种类：偶数只出 normalData，奇数只出 eliteData")]
    public EnemyData normalData;
    public EnemyData eliteData;

    public float interval = 1f;
    public int count = 5;

    void Start()
    {
        if (pool == null)
        {
            Debug.LogError("[Spawner] pool 没有指定，去 Inspector 里把 EnemyPool 拖进来");
            return;
        }
        if (normalData == null || eliteData == null)
        {
            Debug.LogError("[Spawner] normalData / eliteData 有空槽，去 Inspector 里拖两个资产进来");
            return;
        }

        Debug.Log($"[GC] 开局 {GC.GetTotalMemory(false) / 1024} KB");
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        // 只造一个等待对象，循环里反复用。在循环里 new 就是每一轮产生一份垃圾。
        WaitForSeconds wait = new WaitForSeconds(interval);

        for (int i = 0; i < count; i++)
        {
            // 偶数只出小怪、奇数只出精英。用循环变量 i 判断，不再额外维护计数器。
            EnemyData data = (i % 2 == 0) ? normalData : eliteData;
            pool.Get(data);

            yield return wait;
        }

        Debug.Log($"[GC] 生成完 {GC.GetTotalMemory(false) / 1024} KB");
    }
}
