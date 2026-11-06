using UnityEngine;

/// <summary>
/// 一种敌人的数值配置。
/// 注意：它是个"资产文件"，不是场景里的物体，也不是组件，不能挂到物体上。
/// 在 Project 面板右键 Create → TowerDefense → Enemy Data 生成一份。
/// </summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "TowerDefense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string displayName = "小怪";
    public float maxHp = 100f;
    public float speed = 3f;
    public float turnSpeed = 360f;   // 度/秒
}
