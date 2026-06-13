using UnityEngine;

public class SpiderLegRef : MonoBehaviour
{
    // 蜘蛛腿落点参考：从参考物向下射线，找到地面接触点。
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float contactPointRadius = 0.5f;

    // Create leg contact point using Raycast 
    public Vector3 ContactPoint()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, whatIsGround))
            return hitInfo.point;

        return transform.position;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, ContactPoint());
        Gizmos.DrawSphere(ContactPoint(), contactPointRadius);
    }
}
