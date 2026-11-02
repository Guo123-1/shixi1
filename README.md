# TowerDefense

> 个人练习项目 · 目标不是做一款完整的游戏，而是把 **帧驱动逻辑 / 对象池 / 接口解耦** 这三件事写扎实，作为投 Unity 客户端实习的作品集。
> 引擎版本：Unity 2021.3.45f2c1 · 模板：3D（Built-in）· 全部使用自带几何体占位，不引入外部美术

---

## 一句话玩法

怪物沿固定路径走向基地，炮台自动锁定射程内最近的敌人并开火，击杀计分，漏怪扣基地血量。

## 当前进度

| 模块 | 状态 | 说明 |
|---|---|---|
| 敌人路径行走 | ✅ | 路径点数组 + 索引推进 + 平滑转向 |
| 波次生成 | ✅ | 协程按间隔生成，数量与间隔可配 |
| 敌人对象池 | ✅ | 开局预热，运行期零 `Instantiate` |
| 炮台索敌 | ✅ | `OverlapSphere` + LayerMask 过滤，转向瞄准 |
| 开火与子弹 | ✅ | 角度容差 + 冷却时间，子弹目标注入 |
| 伤害与死亡 | ✅ | `IDamageable` 接口，死亡回池复用 |
| 战况统计 | ✅ | 单例 `GameManager` 记录击杀 / 基地血量 |
| HUD | ✅ | TMP 显示击杀数与剩余生命，按变化刷新 |
| 炮台建造（花费金币） | ⬜ | 下一步 |
| 关卡波次配置 | ⬜ | 计划用 ScriptableObject 数据驱动 |
| 事件系统 | ⬜ | 替代当前直接调用，进一步解耦 UI |
| 存档 / 胜负结算 | ⬜ | — |

## 脚本结构

```
Assets/_Project/Scripts/
  Gameplay/
    PathFollower.cs    敌人：沿路径点行走、转向、受击、回池
    EnemySpawner.cs    波次：按间隔从池中取出敌人
    EnemyPool.cs       对象池：预热、取出、归还
    Turret.cs          炮台：索敌、转向、开火
    Bullet.cs          子弹：追踪目标、距离判定命中、伤害
    IDamageable.cs     接口：可受伤对象的统一约定
    GameManager.cs     全局状态：击杀数、基地生命（单例）
  UI/
    HUD.cs             读取 GameManager，按值变化刷新文本
```

## 几个值得说明的设计

**1. 移动全部是帧驱动，不是「一次走完」**

每个 `Update` 只朝当前目标挪一小步（`MoveTowards(pos, target, speed * Time.deltaTime)`），
走到阈值内才把 `index` 推到下一个点。跨帧的状态（`index`、`arrived`、`hp`）一律放在**实例字段**里，
不能放局部变量——局部变量每帧重建，记不住上一帧走到哪。

**2. 对象池用 `SetActive(false)` 表示归还**

池子不维护「空闲列表」，取的时候扫一遍 `activeSelf == false` 的成员即可。
好处是不需要额外的回调与状态同步，坏处是遍历成本 O(n)——在 200 个量级下完全可接受。

池子大小的估算方式：

```
峰值 = 单个敌人占用时长 ÷ 生成间隔
     = (路径长度 ÷ 速度 + 死亡延迟) ÷ interval
     = (18.6 ÷ 3 + 1) ÷ 0.05 ≈ 144
```

实测峰值 139（拐弯时速度不满值），取 200 留余量。**池子够不够，看「临时新建」警告次数是否为 0**，
这是确定性指标；`GC.GetTotalMemory` 在编辑器下波动极大，不能作为判据。

**3. 子弹通过参数拿目标，不用单例**

`Turret.Fire(Transform target)` 生成子弹后直接 `bullet.target = target`。
炮台会有多座，用单例存「当前锁定目标」会被后创建的炮台互相覆盖——单炮台时正常，加第二座就出错。
判据：**全局唯一**（`GameManager`）才配单例，有明确调用关系的（炮台 → 子弹）一律参数传递。

**4. 伤害走接口，子弹不认识具体类型**

```csharp
if (target.TryGetComponent<IDamageable>(out var d))
    d.TakeDamage(damage);
```

子弹只依赖「能受伤」这份约定，以后加可破坏路障、Boss、护盾塔都不需要改子弹代码。

**5. HUD 先比较再赋值**

```csharp
if (gm.kills != lastKills) { lastKills = gm.kills; killText.text = ...; }
```

`text` 赋值会触发字符串分配 + Canvas 重建。每帧无脑赋值在 2 万帧/秒下等于每秒 2 万次重建。

**6. 状态清零放在 `OnEnable`，不放在回池方法里**

不管敌人是走到终点、被打死、还是被清场逻辑隐藏，重新出场时一定走 `OnEnable`。
初始化写在「上场那一刻」，比写在「下场那一刻」少一条漏路径。

## 如何运行

1. Unity Hub 用 **2021.3.45f2c1** 打开本目录（更高版本会提示升级，可拒绝）
2. 打开 `Assets/Scenes/SampleScene.unity`
3. 直接 Play

场景中的关键物体：

| 物体 | 作用 |
|---|---|
| `EnemyPool` | 挂着 `EnemyPool`，需配 `Enemy Prefab`、`Waypoints`(5 个)、`Prewarm Count` |
| `EnemySpawner` | 挂着 `EnemySpawner`，需配 `Pool`、`Interval`、`Count` |
| `GameManager` | 挂着 `GameManager`，基地生命默认 10 |
| `Cube` ~ `Cube (4)` | 5 个路径点 |
| `Canvas` | 挂着 `HUD`，含 `KillsText` / `LivesText` |

> 注意：**预制体不能保存对场景物体的引用**。`PathFollower.waypoints` 必须在运行时由 `EnemyPool` 赋值，
> 直接在预制体上拖场景路径点会变成 `None`。

## 目录约定

```
Assets/
  _Project/              # 自己写的一切都放这里，方便和外部资源区分
    Scripts/
      Gameplay/          # 敌人、炮台、子弹、波次、对象池、全局状态
      UI/                # 界面脚本
    Prefabs/
    ScriptableObjects/   # 计划用于波次与数值配置
    Scenes/
    Art/
  TextMesh Pro/          # TMP Essential Resources（UI 文字依赖）
  ThirdParty/            # 外部插件、免费美术资源，一律隔离
```

## 编码约定

- 类名/方法名 `PascalCase`，私有字段 `_camelCase`（当前部分脚本仍为 camelCase，待统一）
- 一个脚本一个职责，超过 200 行就该拆
- 不在 `Update` 里 `GetComponent` / `Camera.main`，一律提前缓存
- `Update` 开头先想「什么情况下这一帧什么都不用做」，用 guard clause 提前 `return`，不写嵌套 `if`
- `Invoke` / `CancelInvoke` 的方法名一律用 `nameof(...)`，避免改名后静默失效
- 提交信息格式：`feat: 敌人路径移动` / `fix: 炮台冷却不生效` / `refactor: 抽出对象池`

## 已知待办

- `Turret` 每帧调用 `Physics.OverlapSphere` 会分配数组，敌人变多后改用 `OverlapSphereNonAlloc` 或降低索敌频率
- `Bullet` 用距离判定命中，速度很高时会穿透，届时改射线检测
- 私有字段命名尚未统一为 `_camelCase`
