using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        Won,
        Lost
    }

    public int kills;
    public int lives = 10;
    public int currentWave;
    public int totalWaves;
    public int gold = 100;          // 初始资金
    public GameState state = GameState.Playing;
    public bool isOver => state != GameState.Playing;


    void OnEnable()
    {
        GameEvents.OnEnemyDied += HandleEnemyDied;
    }
    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= HandleEnemyDied;
    }
    void Update()
    {
        if (!isOver) return;
        if (!Input.GetKeyDown(KeyCode.R)) return;

        Time.timeScale = 1f;      // ← 必须先复位
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

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

    void HandleEnemyDied()
    {
        AddKill();
        AddGold(10);
    }

    public void AddKill()
    {
        kills++;
        Report();
    }

    public void LoseLife()
    {
        lives = Mathf.Max(0, lives - 1);
        Report();

        if (lives <= 0)
        {
            EndGame(GameState.Lost);
        }
    }

    void Report()
    {
        Debug.Log($"kills={kills} lives={lives} gold={gold}");
    }

    public void SetWave(int current, int total)
    {
        currentWave = current;
        totalWaves = total;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Report();
    }
    public bool TrySpend(int amount)
    {
        if (gold < amount) return false;   // 钱不够，什么都不改
        gold -= amount;
        Report();
        return true;
    }
    public void Win()
    {
        EndGame(GameState.Won);
    }
    void EndGame(GameState result)
    {
        if (isOver) return;
        state = result;
        Time.timeScale = 0f;
        Debug.Log(result);
    }

}
