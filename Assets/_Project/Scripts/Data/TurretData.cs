using UnityEngine;

[CreateAssetMenu(menuName = "TD/Turret Data", fileName = "Turret_")]
public class TurretData : ScriptableObject
{
    public string displayName = "炮台";
    public int cost = 50;

    [Header("战斗")]
    public float damage = 25f;
    public float fireInterval = 0.5f;
    public float detectRange = 10f;
    public float aimTolerance = 10f;
    public float turnSpeed = 180f;      // 度/秒

    [Header("表现")]
    public GameObject prefab;
    public GameObject bulletPrefab;
}

