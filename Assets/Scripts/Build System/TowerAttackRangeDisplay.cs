using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TowerAttackRangeDisplay : MonoBehaviour
{
    // 用 LineRenderer 画出圆形攻击范围，主要给建造预览使用。
    [SerializeField] private float range;
    [SerializeField] private float lineWidth = .1f;
    private int segments = 50; // Amount of dots used to create a circle

    private LineRenderer lineRenderer;

    void Awake()
    {
        // 初始化圆环线段数量、宽度、材质和阴影设置。
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1; // Add extra point to close the circle
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = FindFirstObjectByType<BuildManager>().GetAttackRangeMaterial();
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void CreateCircle(bool isCircleShow, float range = 0)
    {
        // 显示时重新按当前塔范围计算圆环点位。
        lineRenderer.enabled = isCircleShow;

        if (isCircleShow == false)
            return;

        float angle = 0;
        Vector3 center = transform.position;

        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * range;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * range;

            lineRenderer.SetPosition(i, new Vector3(x + center.x, center.y, z + center.z));
            angle += 360 / segments;
        }

        lineRenderer.SetPosition(segments, lineRenderer.GetPosition(0));
    }
}
