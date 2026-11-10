using TMPro;   // TextMeshProUGUI 在这个命名空间里
using UnityEngine;

/// <summary>
/// 屏幕左上角的战况显示。
/// 每帧检查数字有没有变，变了才去改文本 —— 每帧无脑赋值会产生垃圾。
/// </summary>
public class HUD : MonoBehaviour
{
    public TextMeshProUGUI killText;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI resultText;   // 场景里先做成隐藏状态


    int lastKills = -1;    // 故意初始成 -1：保证第一帧一定会刷一次
    int lastLives = -1;
    int lastWave = -1;
    int lastGold = -1;
    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.kills != lastKills)
        {
            lastKills = gm.kills;
            killText.text = $"击杀：{gm.kills}";
        }

        if (gm.lives != lastLives)
        {
            lastLives = gm.lives;
            lifeText.text = $"生命：{gm.lives}";
        }
        if (gm.currentWave != lastWave)
        {
            lastWave = gm.currentWave;
            waveText.text = $"波次：{gm.currentWave}/{gm.totalWaves}";
        }
        if (gm.gold != lastGold)
        {
            lastGold = gm.gold;
            goldText.text = $"金币：{gm.gold}";
        }
        if (gm.isOver && !resultText.gameObject.activeSelf)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = gm.state == GameManager.GameState.Won ? "胜利" : "失败";
        }

    }
}
