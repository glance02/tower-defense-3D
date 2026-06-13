using System.Collections.Generic;
using UnityEngine;

public class TowerHammer : Tower
{
    // 锤塔：范围内有敌人时周期性砸地，对所有范围内敌人施加减速。
    [Header("Slow Modifiers")]
    [Range(0, 1)]
    [SerializeField] private float slowMultipler = .4f;
    [SerializeField] private float slowDuration;

    private HammerVisual hammerVisual;

    protected override void Awake()
    {
        base.Awake();

        hammerVisual = GetComponent<HammerVisual>();
    }

    protected override void FixedUpdate()
    {
        // 锤塔不需要锁定单个目标，只检查范围内是否至少有一个敌人。
        if (isTowerActive == false)
            return;

        if (CanAttack())
            Attack();
    }

    protected override void Attack()
    {
        // 攻击动画和减速效果同步触发。
        base.Attack();
        hammerVisual.PlayAttackAnimation();

        foreach (var enemy in FindValidTargets())
        {
            enemy.SlowEnemy(slowMultipler, slowDuration);
        }
    }

    protected override bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCoolDown && AtLeastOneEnemyInRadius();
    }

    private List<Enemy> FindValidTargets()
    {
        // 收集当前半径内所有敌人，锤塔是 AoE 减速。
        List<Enemy> targets = new();
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, attackRange, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();

            if (newEnemy != null)
                targets.Add(newEnemy);
        }

        return targets;
    }

    private bool AtLeastOneEnemyInRadius()
    {
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, attackRange, whatIsEnemy);
        return enemyColliders.Length > 0;
    }
}
