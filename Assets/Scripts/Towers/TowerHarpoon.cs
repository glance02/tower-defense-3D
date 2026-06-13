using System.Collections;
using UnityEngine;

public class TowerHarpoon : Tower
{
    // 鱼叉塔：锁住飞行敌人，造成一次伤害、减速，并持续掉血。
    [Header("Harpoon Details")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileTransform;
    private ProjectileHarpoon currentProjectile;

    [Header("Damage Details")]
    [SerializeField] private float damageOvertime = 10;
    [SerializeField] private float overtimeEffectDuration = 10;
    [Range(0f, 1f)]
    [SerializeField] private float slowEffect = .7f;

    private bool busyWithAttack;
    private bool reachTarget;
    private Coroutine damageOvertimeCo;
    private HarpoonVisual visuals;

    protected override void Awake()
    {
        base.Awake();

        currentProjectile = GetComponentInChildren<ProjectileHarpoon>();
        visuals = GetComponent<HarpoonVisual>();
    }

    protected override void Attack()
    {
        // 鱼叉发射后会进入 busy 状态，直到命中、持续伤害结束或丢失目标才重置。
        base.Attack();

        if (Physics.Raycast(gunPoint.position, gunPoint.forward, out RaycastHit hitInfo, Mathf.Infinity, whatIsTargetable))
        {
            // Making sure current enemy is the same as the Raycast hit target
            currentEnemy = hitInfo.collider.GetComponent<Enemy>();

            busyWithAttack = true;
            currentProjectile.SetupProjectile(currentEnemy, projectileSpeed, this);
            visuals.EnableChainVisual(true, currentProjectile.GetConnectionPoint());

            Invoke(nameof(ResetAttackIfMissed), 1);
        }
    }

    public void ResetAttack()
    {
        // 飞行敌人死亡、鱼叉未命中或持续伤害结束时都会回到待机状态。
        if (damageOvertimeCo != null)
            StopCoroutine(damageOvertimeCo);

        currentEnemy = null;
        lastTimeAttacked = Time.time;
        busyWithAttack = false;
        reachTarget = false;
        visuals.EnableChainVisual(false);
        CreateNewProjectile();
    }

    private void ResetAttackIfMissed()
    {
        if (reachTarget == true)
            return;

        Destroy(currentProjectile.gameObject);
        ResetAttack();
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && busyWithAttack == false;
    }

    // Won't let go until attack is over
    protected override void LoseTargetIfNeeded()
    {
        if (busyWithAttack == false)
            base.LoseTargetIfNeeded();
    }

    private void CreateNewProjectile()
    {
        GameObject newProjectile = objectPool.Get(projectilePrefab, projectileTransform.position, projectileTransform.rotation, towerHead);
        currentProjectile = newProjectile.GetComponent<ProjectileHarpoon>();
    }

    public void ActivateAttack()
    {
        // ProjectileHarpoon 命中敌人后回调这里，真正开始减速和持续伤害。
        reachTarget = true;
        currentEnemy.GetComponent<EnemyFlying>().AddObservingTower(this);
        currentEnemy.SlowEnemy(slowEffect, overtimeEffectDuration);
        visuals.CreateElectrifyVFX(currentEnemy.transform);

        IDamagable damagable = currentEnemy.GetComponent<IDamagable>();
        damagable?.TakeDamage(towerDamage);

        damageOvertimeCo = StartCoroutine(DamageOvertimeCo(damagable));
    }

    private IEnumerator DamageOvertimeCo(IDamagable damagable)
    {
        // 把总持续伤害拆成多次 tick，方便形成电击持续伤害的感觉。
        float time = 0;

        // How often the damage is tick
        float damageFrequency = overtimeEffectDuration / damageOvertime;
        float damagePertick = damageOvertime / (overtimeEffectDuration / damageFrequency);

        while (time < overtimeEffectDuration)
        {
            damagable?.TakeDamage(damagePertick);
            yield return new WaitForSeconds(damageFrequency);
            time += damageFrequency;
        }

        ResetAttack();
    }
}
