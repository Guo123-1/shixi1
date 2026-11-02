using System;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyPool pool;
    public float interval = 1f;
    public int count = 5;

    void Start()
    {
        if (pool == null)
        {
            Debug.LogError("[Spawner] pool 没有指定，去 Inspector 里把 EnemyPool 拖进来");
            return;
        }

        Debug.Log($"[GC] 开局 {GC.GetTotalMemory(false) / 1024} KB");
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        for (int i = 0; i < count; i++)
        {
            pool.Get();          // ← 原来是 Instantiate，现在改成从池里取
            yield return new WaitForSeconds(interval);
        }

        Debug.Log($"[GC] 生成完 {GC.GetTotalMemory(false) / 1024} KB");
    }
}
