using UnityEngine;

public class EnemyBossUnit : Enemy
{
    // 飞行 Boss 生成的小单位：先从空中落下，落地后再启用 NavMeshAgent 前往终点。
    private Vector3 savedDestination;
    private Vector3 lastKnownBossPosition;
    private EnemyFlyingBoss myBoss;

    protected override void Update()
    {
        base.Update();

        if (myBoss != null)
            lastKnownBossPosition = myBoss.transform.position;
    }

    public void SetupEnemy(Vector3 destination, EnemyFlyingBoss myNewBoss, EnemyPortal myNewPortal)
    {
        // Boss 生成单位后调用，手动加入传送门活动列表，保证波次计数正确。
        ResetEnemy();
        ResetMovement();

        myBoss = myNewBoss;
        enemyPortal = myNewPortal;
        enemyPortal.GetActiveEnemies().Add(gameObject);
        savedDestination = destination;

        InvokeRepeating(nameof(SnapToBossIfNeeded), .1f, .5f);
    }

    private void ResetMovement()
    {
        // 初始阶段让单位受重力下落，暂时禁用寻路。
        rb.useGravity = true;
        rb.isKinematic = false;
        agent.enabled = false;
    }

    // Disable gravity and enable kinematic once spawned unit touch the ground, then set its destination
    // The gravity is enabled and kinematic is disabled by default to let the unit fall to the ground
    void OnCollisionEnter(Collision collision)
    {
        // 碰到地面后切换成 NavMesh 移动模式。
        if (collision.collider.CompareTag("Enemy"))
            return;

        if (Vector3.Distance(transform.position, lastKnownBossPosition) > 2.5f)
        {
            if (myBoss != null)
                transform.position = lastKnownBossPosition + new Vector3(0, -1, 0);
        }

        rb.useGravity = false;
        rb.isKinematic = true;

        agent.enabled = true;
        agent.SetDestination(savedDestination);
    }

    private void SnapToBossIfNeeded()
    {
        // 如果单位落到 NavMesh 外太远，就拉回 Boss 附近重新下落。
        if (agent.enabled && agent.isOnNavMesh == false)
        {
            if (Vector3.Distance(transform.position, lastKnownBossPosition) > 3f)
            {
                transform.position = lastKnownBossPosition + new Vector3(0, -1, 0);
                ResetMovement();
            }
        }
    }

    public override float CalculateDistanceToGoal()
    {
        return Vector3.Distance(transform.position, GetFinalWayPoint());
    }
}
