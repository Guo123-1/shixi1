using UnityEngine;
using TMPro;   // TextMeshProUGUI 在这个命名空间里

/// <summary>
/// 屏幕左上角的战况显示。
/// 每帧检查数字有没有变，变了才去改文本 —— 每帧无脑赋值会产生垃圾。
/// </summary>
public class HUD : MonoBehaviour
{
    public TextMeshProUGUI killText;
    public TextMeshProUGUI lifeText;

    int lastKills = -1;    // 故意初始成 -1：保证第一帧一定会刷一次
    int lastLives = -1;

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.kills != lastKills)
        {
            lastKills = gm.kills;
            killText.text = $"Kills: {gm.kills}";
        }

        if (gm.lives != lastLives)
        {
            lastLives = gm.lives;
            lifeText.text = $"Lives: {gm.lives}";
        }
    }
}
