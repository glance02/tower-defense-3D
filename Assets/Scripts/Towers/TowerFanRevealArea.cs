using UnityEngine;

public class TowerFanRevealArea : MonoBehaviour
{
    // 风扇塔的触发器区域：进入列表的敌人会被 TowerFan 周期性反隐。
    private TowerFan towerFan;

    void Awake()
    {
        towerFan = GetComponentInParent<TowerFan>();
    }

    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
            towerFan.AddEnemyToReveal(enemy);
    }

    void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
            towerFan.RemoveEnemyToReveal(enemy);
    }
}
