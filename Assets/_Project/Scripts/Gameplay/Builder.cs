using UnityEngine;
using UnityEngine.EventSystems;

public class Builder : MonoBehaviour
{
    public TurretData[] turretOptions;      // 按数字键 1、2 切换
    public LayerMask groundLayer;
    public LayerMask blockLayer;
    public float buildRadius = 0.8f;

    int selected;
    public int SelectedIndex => selected;      // 给 UI 读
    TurretData Current
    {
        get
        {
            if (turretOptions == null || turretOptions.Length == 0) return null;
            return turretOptions[Mathf.Clamp(selected, 0, turretOptions.Length - 1)];
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Select(1);

        if (GameManager.Instance == null || GameManager.Instance.isOver) return;

        TurretData current = Current;
        if (current == null) return;

        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer)) return;

        Vector3 center = hit.point + Vector3.up * 0.5f;
        if (Physics.CheckSphere(center, buildRadius, blockLayer))
        {
            Debug.Log("[Builder] 这里被占了");
            return;
        }

        if (!GameManager.Instance.TrySpend(current.cost))
        {
            Debug.Log($"[Builder] 金币不足（需要 {current.cost}）");
            return;
        }

        Turret turret = Instantiate(current.prefab, hit.point, Quaternion.identity)
                            .GetComponent<Turret>();
        turret.ApplyData(current);
    }

    public void Select(int index)
    {
        if (turretOptions == null || index < 0 || index >= turretOptions.Length) return;
        selected = index;
    }
}




