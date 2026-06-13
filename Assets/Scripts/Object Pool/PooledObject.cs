using UnityEngine;

public class PooledObject : MonoBehaviour
{
    // 记录池化对象最初来自哪个 prefab，ObjectPoolManager 回收时用它找到正确对象池。
    public GameObject originalPrefab;
}
