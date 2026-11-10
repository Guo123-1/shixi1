using UnityEngine;

// 冒号后面：基类在前（只能有一个），接口在后（可以有多个）
public class PathFollower : MonoBehaviour, IDamageable
{
    public Transform[] waypoints;
    public float speed = 3f;
    public float turnSpeed = 360f;   // 度/秒
    public float maxHp = 100f;

    // 数值来源。注意它不由你自己在 Inspector 里拖，
    // 而是池子取出敌人时统一注入 —— 因为同一个预制体要能变成不同的怪。
    public EnemyData data;

    [SerializeField] float hp;       // 加 SerializeField 是为了运行时能在 Inspector 里看它掉血

    int index;        // ← 核心：当前目标点编号
    bool arrived;
    bool isDead;

    // 每次从池里被取出、物体重新激活时执行。
    // 所有"跨局状态"都在这里清零，比在归还时清更安全。
    void OnEnable()
    {
        CancelInvoke();     // 清掉上一局遗留的延时调用
        index = 0;
        arrived = false;
        isDead = false;

        hp = maxHp;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (arrived || isDead) return;

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
                if (GameManager.Instance != null) GameManager.Instance.LoseLife();
                arrived = true;
                Debug.Log("reach base");
                Invoke(nameof(ReturnToPool), 1f);
            }
        }
    }

    /// <summary>
    /// 把一份数值配置装到自己身上。必须在 SetActive(true) 之前调用，
    /// 因为 OnEnable 会执行 hp = maxHp，晚一步就用到旧血量了。
    /// </summary>
    public void ApplyData(EnemyData d)
    {
        if (d == null)
        {
            // 以前这里是静默 return，看不出问题。改成警告：漏配数据时要能看见。
            Debug.LogWarning($"[{name}] ApplyData 收到 null，沿用预制体上的数值", this);
            return;
        }

        data = d;
        maxHp = d.maxHp;
        speed = d.speed;
        turnSpeed = d.turnSpeed;
    }

    // ===== 接口要求的方法。子弹不认识 PathFollower，只认这个方法名 =====
    public void TakeDamage(float amount)
    {
        if (isDead) return;          // 已经死了，别重复扣血、重复回池

        hp -= amount;
        Debug.Log($"{name} hp = {hp}");

        if (hp <= 0f)
        {
            isDead = true;
            GameEvents.RaiseEnemyDied();
            Debug.Log("enemy died, hp=0");
            ReturnToPool();          // 死亡立即回池，不用等 1 秒
        }
    }

    // 归还给池子：只负责隐藏，状态清零交给 OnEnable
    void ReturnToPool()
    {
        CancelInvoke();
        gameObject.SetActive(false);
    }
}
