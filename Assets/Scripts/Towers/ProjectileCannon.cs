using UnityEngine;

public class ProjectileCannon : MonoBehaviour
{
    // 炮弹投射物：按初速度飞行，碰撞时造成范围伤害并播放爆炸特效。
    [SerializeField] private float damageRadius;
    [SerializeField] private GameObject VFXExplosion;
    [SerializeField] private LayerMask whatIsEnemy;

    private float projectileDamage;
    private Rigidbody rb;
    private ObjectPoolManager objPool;
    private TrailRenderer trail;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }

    public void SetupProjectile(Vector3 newVelocity, float towerDamage, ObjectPoolManager newObjPool)
    {
        // 从对象池取出后重置拖尾、速度和伤害。
        trail.Clear();
        rb.linearVelocity = newVelocity;
        projectileDamage = towerDamage;
        objPool = newObjPool;
    }

    public void DamageEnemies()
    {
        // 爆炸点周围所有实现 IDamagable 的敌人都会受到伤害。
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, damageRadius, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            IDamagable damagableIn = enemy.GetComponent<IDamagable>();

            if (damagableIn != null)
                damagableIn.TakeDamage(projectileDamage);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 炮弹命中任意触发器后爆炸并回收到对象池。
        DamageEnemies();

        objPool.Get(VFXExplosion, transform.position, Quaternion.identity);
        objPool.Remove(gameObject);
    }
}
