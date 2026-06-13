using UnityEngine;
using UnityEngine.AI;

public class ProjectileDrone : MonoBehaviour
{
    // 蜘蛛无人机投射物：启用后用 NavMeshAgent 追最近敌人，靠近后自爆。
    [SerializeField] private float damage;
    [SerializeField] private float damageRadius;
    [SerializeField] private float detonateDistance;
    [SerializeField] private GameObject explosionVFX;
    [Space]

    [SerializeField] private float enemyCheckRadius = 10;
    [SerializeField] private float targetUpdateInterval = .5f;
    [SerializeField] private LayerMask whatIsEnemy;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private ObjectPoolManager objectPool;
    private TrailRenderer trail;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        objectPool = ObjectPoolManager.instance;
        trail = GetComponent<TrailRenderer>();

        InvokeRepeating(nameof(UpdateClosestTarget), .1f, targetUpdateInterval);
    }

    void Update()
    {
        // agent 还没放到 NavMesh 上时不能设置目的地。
        if (currentTarget == null || agent.enabled == false || agent.isOnNavMesh == false)
            return;

        agent.SetDestination(currentTarget.position);

        if (Vector3.Distance(transform.position, currentTarget.position) < detonateDistance)
            Explode();
    }

    public void Explode()
    {
        // 自爆造成范围伤害，播放特效，然后回收自身。
        DamageEnemies();
        objectPool.Get(explosionVFX, transform.position + new Vector3(0, 0.4f, 0), Quaternion.identity);
        objectPool.Remove(gameObject);
    }

    public void DamageEnemies()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, damageRadius, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            IDamagable damagableIn = enemy.GetComponent<IDamagable>();

            if (damagableIn != null)
                damagableIn.TakeDamage(damage);
        }
    }

    public void SetupDrone(float towerDamage)
    {
        // 从塔上释放无人机时调用：清拖尾、设置伤害、脱离父物体并启用寻路。
        trail.Clear();
        damage = towerDamage;
        agent.enabled = true;
        transform.parent = null;
    }

    private void UpdateClosestTarget()
    {
        // 定时更新目标，避免每帧做 OverlapSphere。
        currentTarget = FindClosestEnemy();
    }

    private Transform FindClosestEnemy()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, enemyCheckRadius, whatIsEnemy);

        Transform nearestEnemy = null;
        float shortestDistance = float.MaxValue;

        foreach (Collider enemy in enemiesAround)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < shortestDistance)
            {
                nearestEnemy = enemy.transform;
                shortestDistance = distance;
            }
        }

        return nearestEnemy;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius); 
    }
}
