using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int kills;
    public int lives = 10;

    void Awake()
    {
        // 场景里如果多挂了一个，自毁，保证全局只有一个
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ===== 数据的唯一出口 =====
    // 想让游戏"知道击杀数变了"的所有逻辑，都写在这两个方法里。
    // 敌人只管调用，不关心后面要打日志还是刷新 UI。
    public void AddKill()
    {
        kills++;
        Report();
    }

    public void LoseLife()
    {
        lives--;
        Report();

        if (lives <= 0)
        {
            Debug.Log("Game Over");
        }
    }

    void Report()
    {
        Debug.Log($"kills={kills} lives={lives}");
    }
}
