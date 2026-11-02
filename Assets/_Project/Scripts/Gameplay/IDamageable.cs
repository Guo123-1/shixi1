/// <summary>
/// 一份"能力清单"：谁签了它，谁就能被子弹这类攻击物伤害。
/// 注意：这里是 interface，不是 class，也不继承 MonoBehaviour。
/// 接口不是组件，不能拖到物体上，只能被类实现。
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
}
