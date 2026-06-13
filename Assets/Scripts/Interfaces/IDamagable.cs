using UnityEngine;

public interface IDamagable
{
    // 统一受伤接口：塔和投射物不需要知道目标具体是哪种 Enemy，只要能 TakeDamage。
    void TakeDamage(float damage);
}
