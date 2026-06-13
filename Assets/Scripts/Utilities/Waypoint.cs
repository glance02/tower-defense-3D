using UnityEngine;

public class Waypoint : MonoBehaviour
{
    // 路点标记脚本：EnemyPortal 通过这个组件识别子物体中的路径点。
    // Only used for finding waypoint component

    void Awake()
    {
        // 路点只用于逻辑，不需要在游戏中显示。
        GetComponent<MeshRenderer>().enabled = false;
    }
}
