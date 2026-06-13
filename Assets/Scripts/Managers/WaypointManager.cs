using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    // 简单路点容器：如果某个系统需要 Transform 数组，可以从这里统一读取。
    [SerializeField] private Transform[] waypoints;

    public Transform[] GetWaypoints() => waypoints;
}
