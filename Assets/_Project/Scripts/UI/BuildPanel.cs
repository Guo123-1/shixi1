using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanel : MonoBehaviour
{
    public Builder builder;          // Inspector 拖
    public Button[] buttons;

    int lastSelected = -1;

    void Start()
    {
        for (int i = 0; i < buttons.Length && i < builder.turretOptions.Length; i++)
        {
            TurretData d = builder.turretOptions[i];
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text =
                $"{d.displayName}\n{d.cost} 金";

            int index = i;                                    // ← 这一行不能省
            buttons[i].onClick.AddListener(() => builder.Select(index));
        }
    }

    void Update()
    {
        if (builder.SelectedIndex == lastSelected) return;
        lastSelected = builder.SelectedIndex;

        for (int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = (i != lastSelected);
    }
}

