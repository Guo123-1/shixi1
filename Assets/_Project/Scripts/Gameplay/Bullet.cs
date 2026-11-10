using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Transform target;
    public float speed = 15f;
    public float lifeTime = 3f;
    public float hitRadius = 0.3f;
    public float damage = 25f;

    void Start()
    {
        Destroy(gameObject, lifeTime);   // 兜底：3 秒后无论打没打中都消失
    }

    void Update()
    {
        // 目标没了（被销毁 / 回池隐藏）→ 自己也消失
        if (target == null || !target.gameObject.activeSelf)
        {
            Destroy(gameObject);
            return;
        }

        // 朝目标飞
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);

        //命中判定：够近就算打中
        if (Vector3.Distance(transform.position, target.position) < hitRadius)
        {
            Hit();
            Destroy(gameObject);
        }
    }

    void Hit()
    {
        // 子弹不知道目标具体是什么类，只关心"它能不能挨打"
        if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
        }
    }
    public void Init(Transform target, float damage)
    {
        this.target = target;
        this.damage = damage;
    }

}

