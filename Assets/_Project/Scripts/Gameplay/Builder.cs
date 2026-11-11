using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 鼠标点地面建塔：预览 → 校验 → 扣款 → 实例化。
/// 预览和落地共用同一套判断（TryGetGroundPoint / IsSpotFree），
/// 否则两处规则迟早会不同步：预览绿、点下去被拒。
/// </summary>
public class Builder : MonoBehaviour
{
    public TurretData[] turretOptions;      // 按数字键 1、2 切换
    public LayerMask groundLayer;
    public LayerMask blockLayer;
    public float buildRadius = 0.8f;

    static readonly Color ValidColor = new Color(0.35f, 1f, 0.45f, 0.45f);
    static readonly Color InvalidColor = new Color(1f, 0.35f, 0.35f, 0.45f);

    Camera cam;
    int selected;

    GameObject ghost;               // 跟着鼠标走的半透明塔
    Renderer[] ghostRenderers;
    Material ghostMat;

    Transform rangeRing;            // 塔影脚下的射程圈
    Material ringMat;

    public int SelectedIndex => selected;      // 给 UI 读

    TurretData Current
    {
        get
        {
            if (turretOptions == null || turretOptions.Length == 0) return null;
            return turretOptions[Mathf.Clamp(selected, 0, turretOptions.Length - 1)];
        }
    }

    void Awake()
    {
        cam = Camera.main;      // 按标签查找有成本，缓存一次
    }

    void Start()
    {
        ghostMat = CreateGhostMaterial();
        BuildRangeRing();
        RebuildGhost();             // 它要用到 rangeRing，所以必须排在后面
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Select(1);

        if (GameManager.Instance == null || GameManager.Instance.isOver) { HidePreview(); return; }
        TurretData current = Current;
        if (current == null) { HidePreview(); return; }
        if (ghost == null) { HidePreview(); return; }         // 没有预制体：无预览，也建不了

        // 鼠标停在 UI 上时，既不预览也不落塔
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool onGround = false;
        Vector3 point = Vector3.zero;
        if (!overUI) onGround = TryGetGroundPoint(out point);

        // —— 预览：先摆好位置，再让它出现 ——
        if (onGround)
        {
            bool affordable = GameManager.Instance.gold >= current.cost;
            Color stateColor = (affordable && IsSpotFree(point)) ? ValidColor : InvalidColor;

            ghost.transform.position = point;
            ghostMat.color = stateColor;

            if (rangeRing != null)
            {
                rangeRing.position = point + Vector3.up * 0.02f;
                ringMat.color = new Color(stateColor.r, stateColor.g, stateColor.b, 0.15f);
            }
        }
        ShowGhost(onGround);
        ShowRing(onGround);

        // —— 点击才落塔 ——
        if (!onGround || !Input.GetMouseButtonDown(0)) return;
        TryBuild(current, point);
    }

    // ── 判断：只有这两个方法在回答"能不能建" ─────────────────────────

    bool TryGetGroundPoint(out Vector3 point)
    {
        point = Vector3.zero;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer)) return false;

        point = hit.point;
        return true;
    }

    bool IsSpotFree(Vector3 point)
    {
        Vector3 center = point + Vector3.up * 0.5f;
        return !Physics.CheckSphere(center, buildRadius, blockLayer);
    }

    // ── 落地 ────────────────────────────────────────────────────────

    void TryBuild(TurretData current, Vector3 point)
    {
        if (!IsSpotFree(point))
        {
            Debug.Log("[Builder] 这里被占了");
            return;
        }

        if (!GameManager.Instance.TrySpend(current.cost))
        {
            Debug.Log($"[Builder] 金币不足（需要 {current.cost}）");
            return;
        }

        Turret turret = Instantiate(current.prefab, point, Quaternion.identity)
                            .GetComponent<Turret>();
        turret.ApplyData(current);
    }

    public void Select(int index)
    {
        if (turretOptions == null || index < 0 || index >= turretOptions.Length) return;
        if (index == selected) return;          // 没变就不用重建

        selected = index;
        RebuildGhost();                         // 换了塔型，塔影也得跟着换
    }

    // ── 塔影 ────────────────────────────────────────────────────────

    Material CreateGhostMaterial()
    {
        // Sprites/Default：无光照 + 支持透明，正好当预览材质
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");   // 退路：不透明，但至少看得见
        if (shader == null)
        {
            Debug.LogError("[Builder] 找不到可用着色器，预览不可用");
            return null;
        }
        return new Material(shader);
    }

    void RebuildGhost()
    {
        if (ghost != null) Destroy(ghost);
        ghost = null;
        ghostRenderers = null;

        TurretData d = Current;
        if (d == null || d.prefab == null || ghostMat == null) return;

        ghost = Instantiate(d.prefab);
        ghost.name = "GhostPreview";

        // 只保留"看得见"，去掉"会动"和"会挡路"
        foreach (Collider c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (MonoBehaviour mb in ghost.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;

        ghostRenderers = ghost.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostRenderers[i].sharedMaterial = ghostMat;    // 共用一份，改颜色一次就全变
            ghostRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        ghostMat.color = ValidColor;
        ShowGhost(false);

        // 半径只在换塔型时变，所以缩放也只在这里设一次
        if (rangeRing != null)
        {
            float diameter = d.detectRange * 2f;    // 圆柱原语直径是 1，缩放值就等于直径
            rangeRing.localScale = new Vector3(diameter, 0.01f, diameter);
        }
    }

    // ── 射程圈 ──────────────────────────────────────────────────────

    void BuildRangeRing()
    {
        if (ghostMat == null) return;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "RangeRing";

        // 原语自带碰撞体。现在它不挡谁，但一个躺在地上的隐形碰撞体是定时炸弹
        Destroy(go.GetComponent<Collider>());

        ringMat = new Material(ghostMat);           // 复制一份：圈和塔影各调各的颜色
        Renderer r = go.GetComponent<Renderer>();
        r.sharedMaterial = ringMat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        rangeRing = go.transform;
        go.SetActive(false);
    }

    void ShowRing(bool visible)
    {
        if (rangeRing == null) return;
        if (rangeRing.gameObject.activeSelf != visible) rangeRing.gameObject.SetActive(visible);
    }

    void HidePreview()
    {
        ShowGhost(false);
        ShowRing(false);
    }

    void ShowGhost(bool visible)
    {
        if (ghost != null && ghost.activeSelf != visible) ghost.SetActive(visible);
    }
}
