using UnityEngine;

public class EnemyHeavy : Enemy
{
    // 重甲敌人：先消耗护盾，护盾破掉后才开始扣本体血量。
    [Header("Enemy Details")]
    [SerializeField] private float currentShield = 50;
    [SerializeField] private float maxShield = 150;
    [SerializeField] private EnemyShield shieldObject;

    protected override void OnEnable()
    {
        base.OnEnable();

        currentShield = maxShield;
        EnableShieldIfNeeded();
    }

    private void EnableShieldIfNeeded()
    {
        if (shieldObject != null && currentShield > 0)
            shieldObject.gameObject.SetActive(true);
    }

    public override void TakeDamage(float damage)
    {
        // 护盾存在时，所有伤害都打在盾上，并触发护盾受击反馈。
        if (currentShield <= 0)
            base.TakeDamage(damage);
        else
        {
            currentShield -= damage;
            shieldObject.ActivateShieldImpact();

            if (currentShield <= 0)
                shieldObject.gameObject.SetActive(false);
        }
    }
}
