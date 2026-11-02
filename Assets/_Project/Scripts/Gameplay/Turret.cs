using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float detectRange = 10f;
    public LayerMask enemyLayer;
    public float aimTolerance = 10f;    // 容差角度：差多少度以内算对准
    public float fireInterval = 0.5f;   // 冷却秒数

    float nextFireTime;                 // ← 跨帧的东西放字段

    void Update()
    {
        Transform enemy = FindNearestEnemy();
        if (enemy == null) return;          // 没敌人 → 这一帧什么都不做

        Vector3 direction = enemy.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;   // 目标和自己重合，转向无意义

        // 1. 平滑转向
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 5f);

        // 2. 对准了才开火
        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > aimTolerance) return;    // 没对准
        if (Time.time < nextFireTime) return; // 还在冷却

        Fire(enemy);
        nextFireTime = Time.time + fireInterval;
    }

    void Fire(Transform target)
    {
        if(target == null) return;
        GameObject bulletObj=Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.target = target;
        }

    }

    Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, enemyLayer);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }
        return nearest;
    }
}
