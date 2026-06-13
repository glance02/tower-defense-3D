using UnityEngine;

public class TowerCannon : Tower
{
    // 炮塔：发射带抛物线的炮弹，优先攻击敌人最密集的位置。
    [Header("Cannon Details")]
    [SerializeField] private float timeToTarget = 1.5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private ParticleSystem attackVFX;

    protected override void Attack()
    {
        // 计算初速度后把炮弹交给 ProjectileCannon，炮弹自己负责飞行和爆炸伤害。
        base.Attack();

        Vector3 velocity = CalculateLaunchVelocity();
        attackVFX.Play();
        
        GameObject projectile = objectPool.Get(projectilePrefab, gunPoint.position, Quaternion.identity);
        ProjectileCannon projectileComp = projectile.GetComponent<ProjectileCannon>();

        projectileComp.SetupProjectile(velocity, towerDamage, objectPool);
    }

    protected override void HandleRotation()
    {
        if (currentEnemy == null)
            return;

        RotateBodyTowardsEnemy();
        FaceLaunchDirection();
    }

    // Override find enemies logic to only target enemy that has the most enemies around it
    protected override Enemy FindEnemiesWithinRange()
    {
        // 炮塔适合打群体，所以目标选择逻辑不是最近终点，而是周围敌人最多的敌人。
        int foundColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, enemyHitColliders, whatIsEnemy);

        Enemy bestTarget = null;
        int maxNearbyEnemies = 0;

        for (int i = 0; i < foundColliders; i++)
        {
            Transform enemyTransform = enemyHitColliders[i].transform;
            int enemiesAmount = EnemiesAroundEnemy(enemyTransform);

            if (enemiesAmount > maxNearbyEnemies)
            {
                maxNearbyEnemies = enemiesAmount;
                bestTarget = enemyTransform.GetComponent<Enemy>();
            }
        }

        return bestTarget;
    }

    // Return the collider of enemies around targeted enemy
    private int EnemiesAroundEnemy(Transform enemyToCheck)
    {
        return Physics.OverlapSphereNonAlloc(enemyToCheck.position, 1, enemyHitColliders, whatIsEnemy);
    }

    // Rotate tower head towards the calculated launch velocity
    private void FaceLaunchDirection()
    {
        Vector3 attackDirection = CalculateLaunchVelocity();
        Quaternion lookRotation = Quaternion.LookRotation(attackDirection);

        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        towerHead.rotation = Quaternion.Euler(rotation.x, towerHead.eulerAngles.y, 0);
    }

    // Calculate projectile launch velocity
    private Vector3 CalculateLaunchVelocity()
    {
        // 通过指定飞行时间反推初速度，使炮弹能按弧线落到当前敌人位置。
        Vector3 direction = currentEnemy.GetCenterPoint() - gunPoint.position;
        Vector3 directionXZ = new(direction.x, 0, direction.z);

        // Get the speed needed on x and z axis to reach the target in time
        Vector3 velocityXZ = directionXZ / timeToTarget;

        // Calculate the upward velocity so the projectile can reach its target height in time until gravity pull it down 
        float yVelocity = (direction.y - Physics.gravity.y * Mathf.Pow(timeToTarget, 2) / 2) / timeToTarget;
        Vector3 launchVelocity = velocityXZ + (Vector3.up * yVelocity);
        
        return launchVelocity;
    }
}
