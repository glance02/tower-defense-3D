using System.Collections.Generic;
using UnityEngine;

public class TowerFan : Tower
{
    // 风扇塔/探测塔：不造成伤害，周期性让范围内敌人无法隐身。
    [Header("Fan Details")]
    [SerializeField] private float revealFrequency = .1f;
    [SerializeField] private float revealDuration = 1f;

    private List<Enemy> enemiesToReveal = new();


    protected override void Awake()
    {
        base.Awake();

        InvokeRepeating(nameof(RevealEnemies), .1f, revealFrequency);
    }

    private void RevealEnemies()
    {
        // 范围触发器维护 enemiesToReveal，这里定时给它们刷新反隐持续时间。
        foreach (Enemy enemy in enemiesToReveal)
        {
            if (enemy.isActiveAndEnabled)
                enemy.DisableHide(revealDuration);
        }
    }

    void OnValidate()
    {
        // Inspector 中调整 attackRange 时，编辑器里同步显示前方射程线。
        ForwardAttackDisplay display = GetComponent<ForwardAttackDisplay>();

        if (display != null)
            display.CreateLines(true, attackRange);
    }

    public void AddEnemyToReveal(Enemy enemy) => enemiesToReveal.Add(enemy);

    public void RemoveEnemyToReveal(Enemy enemy) => enemiesToReveal.Remove(enemy);
}
