using System;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyPool pool;

    [Header("波次配置")]
    public WaveData waveData;

    void Start()
    {
        if (pool == null)
        {
            Debug.LogError("[Spawner] pool 没有指定，去 Inspector 里把 EnemyPool 拖进来");
            return;
        }

        if (waveData == null || waveData.waves == null || waveData.waves.Length == 0)
        {
            Debug.LogError("[Spawner] wave 没配，或表是空的");
            return;
        }
        Debug.Log($"[GC] 开局 {GC.GetTotalMemory(false) / 1024} KB");
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        for (int w = 0; w < waveData.waves.Length; w++)
        {
            if (GameManager.Instance != null && GameManager.Instance.isOver) yield break;
            WaveDef wave = waveData.waves[w];

            // 等场上敌人清空，再休整，再宣布
            yield return new WaitUntil(() => pool.ActiveCount == 0);
            yield return new WaitForSeconds(wave.startDelay);

            Debug.Log($"--- 第 {w + 1} 波 / 共 {waveData.waves.Length} 波 ---");
            if (GameManager.Instance != null) GameManager.Instance.SetWave(w + 1, waveData.waves.Length);

            foreach (WaveEntry entry in wave.entries)
            {
                WaitForSeconds wait = new WaitForSeconds(entry.interval);
                for (int i = 0; i < entry.count; i++)
                {
                    pool.Get(entry.enemy);
                    yield return wait;
                }
            }
        }
        Debug.Log($"[GC] 生成完 {GC.GetTotalMemory(false) / 1024} KB");

        yield return new WaitUntil(() => pool.ActiveCount == 0);
        if (GameManager.Instance != null) GameManager.Instance.Win();
    }
}
