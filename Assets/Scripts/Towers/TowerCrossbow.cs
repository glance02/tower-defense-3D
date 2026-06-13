using UnityEngine;

public class TowerCrossbow : Tower
{
    // 弩塔：射线即时命中单体敌人，并播放光束、命中特效和装填视觉。
    private CrossbowVisual visual;

    protected override void Awake()
    {
        base.Awake();

        visual = GetComponent<CrossbowVisual>();
    }

    protected override void Attack()
    {
        // 命中逻辑和视觉都在同一次攻击中完成。
        base.Attack();

        Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

        if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity, whatIsTargetable))
        {
            towerHead.forward = directionToEnemy;

            // Search for IDamagable interface from the hit enemy
            IDamagable damagableIn = hitInfo.transform.GetComponent<IDamagable>();
            damagableIn.TakeDamage(towerDamage);

            visual.CreateOnHitVFX(hitInfo.point);
            visual.PlayAttackVFX(gunPoint.position, hitInfo.point);
            visual.PlayReloadVFX(attackCoolDown);

            AudioManager.instance?.PlaySFX(attackSFX, true);
        }
    }
}
