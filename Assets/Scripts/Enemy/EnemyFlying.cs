using System.Collections.Generic;
using UnityEngine;

public class EnemyFlying : Enemy
{
    // 飞行敌人：不按中间路点走，直接飞向最终目标；死亡时通知鱼叉塔重置。
    private List<TowerHarpoon> observingTowers = new();

    protected override void Start()
    {
        // 飞行单位直接设置终点，不使用 Enemy.Update 中逐路点切换的完整路径。
        base.Start();

        agent.SetDestination(GetFinalWayPoint());
    }

    public override float CalculateDistanceToGoal()
    {
        return Vector3.Distance(transform.position, GetFinalWayPoint());
    }

    public override void RemoveEnemy()
    {
        // 如果被鱼叉锁住，死亡/回收前先让相关塔退出 busy 状态。
        foreach (var tower in observingTowers)
            tower.ResetAttack();

        foreach (var harpoon in GetComponentsInChildren<ProjectileHarpoon>())
            objectPool.Remove(harpoon.gameObject);

        base.RemoveEnemy();
    }

    public void AddObservingTower(TowerHarpoon newTower) => observingTowers.Add(newTower);
}
