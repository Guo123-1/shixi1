using UnityEngine;

// 实验用：按 P 暂停/恢复，观察"游戏时间"和"真实时间"的差别
public class TimeProbe : MonoBehaviour
{
    float nextLog;

    void Update()
    {
        // 按 P 切换暂停
        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
            Debug.Log($"--- timeScale 切换为 {Time.timeScale} ---");
        }

        // 每隔 0.5 真实秒打印一次（用 unscaledTime 计时，暂停时也照常打印）
        if (Time.unscaledTime >= nextLog)
        {
            nextLog = Time.unscaledTime + 0.5f;
            Debug.Log($"游戏时间 {Time.time:F2}s   真实时间 {Time.realtimeSinceStartup:F2}s   deltaTime {Time.deltaTime:F5}");
        }
    }
}
