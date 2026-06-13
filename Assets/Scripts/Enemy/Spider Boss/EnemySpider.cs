using UnityEngine;

public class EnemySpider : Enemy
{
    // 蜘蛛 Boss：移动时沿路点前进，并定期对附近防御塔释放 EMP。
    [Header("EMP Attack Details")]
    [SerializeField] private float towerCheckRadius = 5;
    [SerializeField] private float empCooldown = 8;
    [SerializeField] private float empFXDuration = 3;
    [SerializeField] private GameObject empPrefab;
    [SerializeField] private LayerMask whatIsTower;
    private float empAttackTimer;

    private EnemySpiderVisual spiderVisual;

    protected override void Awake()
    {
        base.Awake();
        spiderVisual = GetComponent<EnemySpiderVisual>();
    }

    protected override void Start()
    {
        base.Start();

        empAttackTimer = empCooldown;
        spiderVisual.BrieflySpeedUpLegs();
    }

    protected override void Update()
    {
        base.Update();

        empAttackTimer -= Time.deltaTime;

        if (empAttackTimer < 0)
            AttemptToEMP();
    }

    protected override bool ShouldChangeWaypoint()
    {
        if (nextWaypointIndex >= enemyWaypoints.Length)
            return false;

        if (agent.remainingDistance <= 0.2f)
            return true;

        return false;
    }

    protected override void ChangeWayPoint()
    {
        spiderVisual.BrieflySpeedUpLegs();
        base.ChangeWayPoint();
    }

    private void AttemptToEMP()
    {
        // 随机选取范围内一座塔作为 EMP 目标。
        Transform target = FindRandomTower();

        if (target == null)
            return;

        empAttackTimer = empCooldown;

        GameObject newEMP = objectPool.Get(empPrefab, transform.position + new Vector3(0, 0.15f, 0), Quaternion.identity);
        newEMP.GetComponent<EnemySpiderEMP>().SetupEMP(empFXDuration, target.position);
    }

    private Transform FindRandomTower()
    {
        Collider[] towers = Physics.OverlapSphere(transform.position, towerCheckRadius, whatIsTower);

        if (towers.Length > 0)
            return towers[Random.Range(0, towers.Length)].transform.root;

        return null;
    }
}
