using UnityEngine;


public class LifecycleDemo : MonoBehaviour
{
    int updateCount;   // Update 被调用的次数
    int fixedCount;    // FixedUpdate 被调用的次数

    void Awake()
    {
        // 对象一被创建就调用，即使脚本是 disabled 也会调用。
        // 用来做「自身」的初始化，比如缓存组件引用。
        Debug.Log($"[{name}] 1.Awake　→ 自身初始化（缓存组件）");
    }

    void OnEnable()
    {
        // 每次被启用时调用，第一次在 Awake 之后。
        Debug.Log($"[{name}] 2.OnEnable　→ 每次启用都会调用");
    }

    void Start()
    {
        // 第一帧 Update 之前调用，只调用一次。
        // 脚本 disabled 时不会调用。用来做「依赖别人已就绪」的初始化。
        Debug.Log($"[{name}] 3.Start　→ 依赖其他对象已就绪的初始化");
    }

    void FixedUpdate()
    {
        // 固定间隔（默认 0.02 秒 = 50 次/秒），物理相关逻辑写这里。
        fixedCount++;
    }

    void Update()
    {
        // 每帧一次，帧率不固定。
        updateCount++;

        if (updateCount <= 3)   // 只打印前 3 帧，否则刷屏
        {
            Debug.Log($"[{name}] 4.Update　第 {updateCount} 帧");
        }
    }

    void LateUpdate()
    {
        // 每帧 Update 之后调用，相机跟随写这里。
        if (updateCount <= 3)
        {
            Debug.Log($"[{name}] 5.LateUpdate　第 {updateCount} 帧（Update 之后）");
        }
    }

    void OnDisable()
    {
        // 被禁用 / SetActive(false) / 销毁前 都会调用。
        Debug.Log($"[{name}] OnDisable　→ 停止，此时 Update 共执行 {updateCount} 次");
    }

    void OnDestroy()
    {
        // 销毁时调用。以后写事件系统，一定要在这里反注册，
        // 否则会出现「对象已销毁但事件还在调用」的空引用。
        Debug.Log($"[{name}] OnDestroy　→ 清理，反注册事件");
    }
}
