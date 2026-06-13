using UnityEngine;

public class TowerMinigun : Tower
{
    // 机枪塔：用射线确认命中，再生成可视化子弹飞向命中点。
    [Header("Minigun Details")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private GameObject projectilePrefab;
    private MinigunVisual minigunVisual;
    [Space]

    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private Transform[] gunPointSet;
    private int gunPointIndex;

    protected override void Awake()
    {
        base.Awake();
        minigunVisual = GetComponent<MinigunVisual>();
    }

    protected override void Attack()
    {
        // 多个枪口轮流开火，制造连续扫射的视觉效果。
        gunPoint = gunPointSet[gunPointIndex];
        Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

        if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity, whatIsTargetable))
        {
            IDamagable damagable = hitInfo.transform.GetComponent<IDamagable>();

            if (damagable == null)
                return;

            GameObject newProjectile = objectPool.Get(projectilePrefab, gunPoint.position, Quaternion.identity);
            newProjectile.GetComponent<ProjectileMinigun>().SetupProjectile(hitInfo.point, damagable, towerDamage, projectileSpeed, objectPool);

            minigunVisual.ReCoilGun(gunPoint);

            // Set attack on cooldown
            base.Attack();

            // Increase the index and wrap back to 0 using modulo
            gunPointIndex = (gunPointIndex + 1) % gunPointSet.Length;
        }
    }

    protected override void RotateTowardsEnemy()
    {
        // 机枪模型有偏移量，所以瞄准时额外减去 rotationOffset。
        if (currentEnemy == null)
            return;

        Vector3 directionToEnemy = currentEnemy.GetCenterPoint() - rotationOffset - towerHead.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);

        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;
        towerHead.rotation = Quaternion.Euler(rotation);
    }
}
