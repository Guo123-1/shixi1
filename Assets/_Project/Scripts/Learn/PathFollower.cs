using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 3f;
    public float turnSpeed = 360f;   // 度/秒

    int index;        // ← 核心：当前目标点编号
    bool arrived;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || arrived) return;

        Transform target = waypoints[index];

        // 1. 朝当前目标点挪一小步（就一步）
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);

        // 2. 平滑转向
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;   // 抹掉高度差，不然敌人会低头/仰头
        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
        }

        // 3. 到了就换下一个目标，而不是一口气走完
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            index++;
            if (index >= waypoints.Length)
            {
                arrived = true;
                Debug.Log("reach base");
                Invoke(nameof(ReturnToPool), 1f);   // 原来是 Destroy，现在改成归还
            }
        }
    }

    // 归还给池子：状态清零 + 隐藏自己。
    // 隐藏之后 EnemyPool.Get() 就能重新取到它。
    void ReturnToPool()
    {
        index = 0;
        arrived = false;
        gameObject.SetActive(false);
    }
}
