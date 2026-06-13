using UnityEngine;

public class ProjectileHarpoon : MonoBehaviour
{
    // 鱼叉投射物：飞向指定敌人，命中后挂到敌人身上并回调塔开始持续伤害。
    private bool isAttached;
    private float speed;
    private Enemy enemy;
    private TowerHarpoon tower;

    [SerializeField] private Transform connectionPoint;

    void Update()
    {
        // 已经命中后位置跟随父物体，不再主动移动。
        if (enemy == null || isAttached)
            return;

        MoveTowardsEnemy();

        if (Vector3.Distance(transform.position, enemy.transform.position) < .4f)
            AttachToEnemy();
    }

    public void SetupProjectile(Enemy newEnemy, float newSpeed, TowerHarpoon newTower)
    {
        // TowerHarpoon 发射时注入目标、速度和回调引用。
        enemy = newEnemy;
        speed = newSpeed;
        tower = newTower;
    }

    private void AttachToEnemy()
    {
        // 命中后设置父物体，让鱼叉跟随敌人移动。
        isAttached = true;
        transform.parent = enemy.transform;
        tower.ActivateAttack();
    }

    private void MoveTowardsEnemy()
    {
        transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
        transform.forward = enemy.transform.position - transform.position;
    }

    public Transform GetConnectionPoint()
    {
        if (connectionPoint == null)
            return transform;

        return connectionPoint;
    }
}
