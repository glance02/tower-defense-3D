using UnityEngine;

public class EnemyStealthHideArea : MonoBehaviour
{
    // 隐身敌人的范围触发器：维护可以被它一起隐藏的附近敌人列表。
    private EnemyStealth enemyStealth;

    private void Awake()
    {
        enemyStealth = GetComponentInParent<EnemyStealth>();
    }

    void OnTriggerEnter(Collider other)
    {
        AddEnemyToHideList(other, true);
    }

    void OnTriggerExit(Collider other)
    {
        AddEnemyToHideList(other, false);
    }

    private void AddEnemyToHideList(Collider enemyCollider, bool isEnemyAdd)
    {
        // 不把其他隐身敌人加入列表，避免互相递归隐藏造成混乱。
        Enemy newEnemy = enemyCollider.GetComponent<Enemy>();

        if (newEnemy == null)
            return;

        if (newEnemy.GetEnemyType() == EnemyType.Stealth)
            return;

        if (isEnemyAdd)
            enemyStealth.GetEnemiesToHide().Add(newEnemy);
        else
            enemyStealth.GetEnemiesToHide().Remove(newEnemy);
    }
}
