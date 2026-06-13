using UnityEngine;

public class EnemySpiderEMP : MonoBehaviour
{
    // 蜘蛛 Boss 发射的 EMP：移动到目标塔附近，触发范围内塔的临时失效。
    [SerializeField] private float empRadius = 2;
    [SerializeField] private float empEffectDuration = 5;
    [SerializeField] private float empSpeed = 1;
    [SerializeField] private GameObject empFX;

    private bool shouldShrink;
    private float shrinkSpeed = 3;
    private Vector3 destination;
    private ObjectPoolManager objectPool;

    void Awake()
    {
        objectPool = ObjectPoolManager.instance;
    }

    void Update()
    {
        // 到达目标点后开始缩小并回收。
        MoveTowerTarget();

        if (shouldShrink)
            Shrink();
    }

    private void Shrink()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (transform.localScale.x <= 0.1f)
            objectPool.Remove(gameObject);
    }

    private void MoveTowerTarget()
    {
        // EMP 不追踪移动目标，只飞向释放时记录的目标位置。
        transform.position = Vector3.MoveTowards(transform.position, destination, empSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < .1f)
            DeactivateEMP();
    }

    public void SetupEMP(float duration, Vector3 target)
    {
        // EnemySpider 生成 EMP 后注入持续时间和目标位置。
        empEffectDuration = duration;
        destination = target;
    }

    void OnTriggerEnter(Collider other)
    {
        // 命中塔的碰撞体时，让塔进入 DeactivateTower 状态。
        Tower tower = other.GetComponent<Tower>();

        if (tower != null)
            tower.DeactivateTower(empEffectDuration, empFX);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, empRadius);
    }

    private void DeactivateEMP() => shouldShrink = true;
}
