using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    // 所有防御塔的基类：负责索敌、转向、攻击节奏，具体攻击方式由子类实现。
    public bool isAttackForward;
    public Enemy currentEnemy;

    [Header("Tower Details")]
    [SerializeField] protected int towerDamage = 10;
    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected float attackCoolDown = 1f;
    [SerializeField] protected float rotationSpeed = 10f;
    [Space]

    [SerializeField] protected Transform towerHead;
    [SerializeField] protected Transform towerBody;
    [SerializeField] protected Transform gunPoint;
    [Space]

    [SerializeField] protected EnemyType enemyPriorityType;
    [SerializeField] protected LayerMask whatIsEnemy;
    [SerializeField] protected LayerMask whatIsTargetable;
    [Space]

    [Header("SFX Details")]
    [SerializeField] protected AudioSource attackSFX;

    protected bool isTowerActive = true;
    protected float lastTimeAttacked;
    protected Coroutine deactiveTowerCo;
    protected ObjectPoolManager objectPool;
    protected Collider[] enemyHitColliders = new Collider[50];

    private GameObject currentEMPFX;

    protected virtual void Awake()
    {
        // For override
    }

    protected virtual void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void FixedUpdate()
    {
        // 使用 FixedUpdate 做范围检测，减少每帧检测带来的波动。
        CheckForEnemies();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void DeactivateTower(float duration, GameObject empFX)
    {
        // 蜘蛛 Boss 的 EMP 会临时禁用塔，并显示 EMP 特效。
        if (deactiveTowerCo != null)
            StopCoroutine(deactiveTowerCo);

        if (currentEMPFX != null)
            objectPool.Remove(currentEMPFX);

        currentEMPFX = objectPool.Get(empFX, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        deactiveTowerCo = StartCoroutine(DeactivateTowerCo(duration));
    }

    protected void CheckForEnemies()
    {
        // 塔的通用战斗流程：丢失目标、重新索敌、转向、尝试攻击。
        if (isTowerActive == false)
            return;

        LoseTargetIfNeeded();
        UpdateTargetIfNeeded();
        HandleRotation();

        if (CanAttack())
            AttemptToAttack();
    }

    private void UpdateTargetIfNeeded()
    {
        if (currentEnemy == null)
        {
            currentEnemy = FindEnemiesWithinRange();
            return;
        }
    }

    protected virtual void HandleRotation()
    {
        RotateTowardsEnemy();
        RotateBodyTowardsEnemy();
    }

    protected virtual void RotateTowardsEnemy()
    {
        // 默认让塔头朝向敌人中心点；某些塔会重写此方法做特殊瞄准。
        if (currentEnemy == null || towerHead == null)
            return;

        // Calculate the vector direction from the tower head to the current enemy's position
        Vector3 directionToTarget = DirectionToEnemyFrom(towerHead);

        // Create a new rotation that look in the direction of the target
        Quaternion newRotation = Quaternion.LookRotation(directionToTarget);

        // Calculate the in-between rotation from the current to the target orientation
        // Then convert it to Euler angles (rotations in degrees around each axis)
        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, newRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        // Convert the Euler angle rotation to Quaternion and apply it to the tower head rotation 
        towerHead.rotation = Quaternion.Euler(rotation);
    }

    protected virtual void RotateBodyTowardsEnemy()
    {
        // 塔身只绕水平面旋转，避免模型上下倾斜。
        if (towerBody == null|| currentEnemy == null)
            return;

        Vector3 directionToEnemy = DirectionToEnemyFrom(towerBody);
        directionToEnemy.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);
        towerBody.rotation = Quaternion.Slerp(towerBody.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    // Tower can still target disabled enemy from object pool
    // Make a check for inactive current enemy so it can switch to other enemy
    protected void AttemptToAttack()
    {
        // 对象池里的敌人可能已被回收但引用还在，这里先做一次有效性检查。
        if (currentEnemy != null && currentEnemy.gameObject.activeSelf == false)
        {
            currentEnemy = null;
            return;
        }

        Attack();
    }

    // Returns the transform of the closest enemy within the tower's attack radius.
    // Uses Physics.OverlapSphereNonAlloc to detect nearby enemies and FindTheClosestEnemy()
    // to determine which one is closest to the finish line.
    protected virtual Enemy FindEnemiesWithinRange()
    {
        // 先找优先类型敌人；没有优先目标时，再从所有敌人里选离终点最近的。
        List<Enemy> allEnemy = new();
        List<Enemy> priorityEnemies = new();

        // Check for all enemies within attack radius using layer mask, and store them in pre-allocated array
        int enemycount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, enemyHitColliders, whatIsEnemy);

        for (int i = 0; i < enemycount; i++)
        {
            if (!enemyHitColliders[i].TryGetComponent<Enemy>(out var enemy))
                continue;

            if (enemy.GetEnemyType() == enemyPriorityType)
                priorityEnemies.Add(enemy);
            else
                allEnemy.Add(enemy);
        }

        if (priorityEnemies.Count > 0)
            return GetTheClosestEnemy(priorityEnemies);

        if (allEnemy.Count > 0)
            return GetTheClosestEnemy(allEnemy);

        return null;
    }

    private Enemy GetTheClosestEnemy(List<Enemy> enemyList)
    {
        // “最近”不是离塔最近，而是离玩家城堡/终点最近，威胁更高。
        Enemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemyList)
        {
            float remainDistance = enemy.CalculateDistanceToGoal();

            // Update if the enemy distance is closer than previous closest distance
            if (remainDistance < closestDistance)
            {
                closestDistance = remainDistance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private IEnumerator DeactivateTowerCo(float duration)
    {
        // 恢复后重置攻击计时，给塔头一点时间重新对准目标。
        isTowerActive = false;
        yield return new WaitForSeconds(duration);
        isTowerActive = true;

        // Prevent the tower from attacking immediately when enable
        // Let the tower has time to adjust their head position before attacking
        lastTimeAttacked = Time.time;
        objectPool.Remove(currentEMPFX);
    }

    protected virtual void LoseTargetIfNeeded()
    {
        // 目标离开攻击范围时释放引用，下次检测会重新找目标。
        if (currentEnemy == null)
            return;

        if (Vector3.Distance(currentEnemy.transform.position, transform.position) > attackRange)
            currentEnemy = null;
    }

    public float GetAttackRange() => attackRange;

    public float GetAttackCooldown() => attackCoolDown;

    protected virtual void Attack() => lastTimeAttacked = Time.time;

    protected virtual bool CanAttack() => Time.time > lastTimeAttacked + attackCoolDown && currentEnemy != null;

    protected virtual Vector3 DirectionToEnemyFrom(Transform startPoint) => (currentEnemy.GetCenterPoint() - startPoint.position).normalized;

}
