using System;
using UnityEngine;

/// <summary>
/// 一整套关卡的全部波次。一个资产 = 一关。
/// 三层结构：WaveData（关卡） → WaveDef（一波） → WaveEntry（一波里的一段）
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "TowerDefense/Wave Data")]
public class WaveData : ScriptableObject
{
    public WaveDef[] waves;
}

/// <summary>
/// 一波敌人。
/// startDelay = 这一波开始前等多少秒（从上一波出完算起）。
/// </summary>
[Serializable]
public class WaveDef
{
    public float startDelay = 3f;
    public WaveEntry[] entries;
}

/// <summary>
/// 一波里的一段：出什么怪、出几只、隔多久。
/// 这一段和你上一版完全一样，原样搬了进来。
/// </summary>
[Serializable]
public class WaveEntry
{
    public EnemyData enemy;
    public int count = 3;
    public float interval = 1f;
}
