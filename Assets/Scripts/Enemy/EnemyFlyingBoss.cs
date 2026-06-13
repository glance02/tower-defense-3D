using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyFlyingBoss : EnemyFlying
{
    // 飞行 Boss：预留了周期性生成小单位的逻辑，死亡时清理这些小单位。
    [SerializeField] private int amountToCreate = 150;
    [SerializeField] private float cooldown = .2f;
    [SerializeField] private GameObject bossUnitPrefab;

    private int unitsCreated;
    private float creationTimer;
    private EnemyFlyingBoss enemyFlyingBoss;
    private List<Enemy> createdEnemies = new();


    protected override void Update()
    {
        // CreateNewBossUnit 目前被注释掉，计时逻辑保留给之后启用召唤。
        base.Update();

        creationTimer -= Time.deltaTime;

        if (creationTimer < 0 && unitsCreated < amountToCreate)
        {
            creationTimer = cooldown;
            // CreateNewBossUnit();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        unitsCreated = 0;
    }

    private void CreateNewBossUnit()
    {
        // 从对象池生成 BossUnit，并让它落地后走向终点。
        unitsCreated++;
        GameObject newUnit = objectPool.Get(bossUnitPrefab, transform.position, Quaternion.identity);

        EnemyBossUnit bossUnit = newUnit.GetComponent<EnemyBossUnit>();
        bossUnit.SetupEnemy(GetFinalWayPoint(), this, enemyPortal);

        createdEnemies.Add(bossUnit);
    }

    private void EleminateAllUnits()
    {
        // Boss 死亡时一起击杀已生成单位，避免 Boss 死后小怪残留。
        foreach (var enemy in createdEnemies)
        {
            enemy.GetKilled();
        }
    }

    public override void GetKilled()
    {
        EleminateAllUnits();
        base.GetKilled();
    }
}
