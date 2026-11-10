using UnityEngine;

public class Turret : MonoBehaviour
{
    public TurretData data;
    public Transform firePoint;
    public LayerMask enemyLayer;

    float nextFireTime;                 // ← 跨帧的东西放字段

    public void ApplyData(TurretData d)
    {
        if (d == null)
        {
            Debug.LogWarning($"[{name}] ApplyData 收到 null", this);
            return;
        }
        data = d;
    }

    void Update()
    {
        if (data == null) return;              // 没配数据就什么都不做

        Transform enemy = FindNearestEnemy();
        if (enemy == null) return;

        Vector3 direction = enemy.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(direction),
            data.turnSpeed * Time.deltaTime);      // ← 原来是 Slerp + 硬编码 5f

        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > data.aimTolerance) return;
        if (Time.time < nextFireTime) return;

        Fire(enemy);
        nextFireTime = Time.time + data.fireInterval;
    }


    void Fire(Transform target)
    {
        GameObject obj = Instantiate(data.bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = obj.GetComponent<Bullet>();
        if (bullet != null) bullet.Init(target, data.damage);
    }

    Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, data.detectRange, enemyLayer);
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
