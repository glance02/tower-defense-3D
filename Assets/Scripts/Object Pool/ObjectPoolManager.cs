using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    // 统一对象池：复用敌人、子弹和特效，减少频繁 Instantiate/Destroy 造成的性能开销。
    public static ObjectPoolManager instance;

    [Header("Object Pool Details")]
    [SerializeField] private int defaultPoolSize = 50;
    [SerializeField] private int maxPoolSize = 500;
    [SerializeField] private GameObject[] enemyPools;
    [SerializeField] private GameObject[] projectilePools;
    [SerializeField] private GameObject[] vfxPools;

    private Dictionary<GameObject, ObjectPool<GameObject>> poolDictionary;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializePools();
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion? rotation = null, Transform parent = null)
    {
        // 外部统一用 prefab 作为 key 取对象；没有预建池时会动态创建一个。
        if (poolDictionary.ContainsKey(prefab) == false)
        {
            Debug.LogWarning("No pool was found for game object " + prefab.name + ". Creating new pool!");
            CreateNewPool(prefab);
        }

        GameObject objectToGet = poolDictionary[prefab].Get();
        objectToGet.transform.position = position;
        objectToGet.transform.rotation =  rotation ?? Quaternion.identity;
        objectToGet.transform.parent = parent;

        // Unity was checking NavMeshAgent as soon as the pooled object was activated
        // during preload, before it had valid poition. Disabling agent upfront avoided
        // that premature validation
        if (objectToGet.TryGetComponent<NavMeshAgent>(out var agent))
            agent.enabled = false;
        else
            objectToGet.SetActive(true);

        return objectToGet;
    }

    public void Remove(GameObject objectToRemove)
    {
        // PooledObject 记录了对象来自哪个 prefab，用它找到对应池并释放回去。
        GameObject originalPrefab = objectToRemove.GetComponent<PooledObject>()?.originalPrefab;

        if (originalPrefab == null)
        {
            Debug.LogWarning("You do not have object pool for this game object. Game object will be destroyed!");
            Destroy(objectToRemove);
            return;
        }

        poolDictionary[originalPrefab].Release(objectToRemove);
    }

    private void InitializePools()
    {
        // 启动时把 Inspector 中配置的敌人、子弹、特效 prefab 都建好池。
        poolDictionary = new Dictionary<GameObject, ObjectPool<GameObject>>();

        foreach(GameObject prefab in enemyPools)
            CreateNewPool(prefab);

        foreach (GameObject prefab in projectilePools)
            CreateNewPool(prefab);

        foreach (GameObject prefab in vfxPools)
            CreateNewPool(prefab);
    }

    private void CreateNewPool(GameObject prefab)
    {
        // UnityEngine.Pool.ObjectPool 会负责创建、取出、释放和超量销毁。
        var pool = new ObjectPool<GameObject>
            (
                createFunc: () => NewPoolObject(prefab),
                actionOnRelease: obj =>
                {
                    obj.SetActive(false);
                    obj.transform.parent = transform;
                },
                actionOnDestroy: obj => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: defaultPoolSize,
                maxSize: maxPoolSize
            );

        poolDictionary.Add(prefab, pool);
        StartCoroutine(PreloadPoolCo(pool, defaultPoolSize));
    }

    private IEnumerator PreloadPoolCo(ObjectPool<GameObject> poolToPreload, int count)
    {
        // 分帧预热对象池，避免一帧内创建大量对象导致明显卡顿。
        List<GameObject> preloadedObjects = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = poolToPreload.Get();
            preloadedObjects.Add(obj);
            obj.SetActive(false);
            yield return null;
        }

        foreach (GameObject obj in preloadedObjects)
            poolToPreload.Release(obj);
    }

    private GameObject NewPoolObject(GameObject prefab)
    {
        // 给每个池化对象挂上来源 prefab 标记，释放时才能回到正确的池。
        if (prefab.TryGetComponent<NavMeshAgent>(out var agent))
            agent.enabled = false;

        GameObject newObject = Instantiate(prefab);
        newObject.AddComponent<PooledObject>().originalPrefab = prefab;

        return newObject;
    }
}
