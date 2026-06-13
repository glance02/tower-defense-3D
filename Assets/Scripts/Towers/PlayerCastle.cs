using UnityEngine;

public class PlayerCastle : MonoBehaviour
{
    // 玩家城堡终点：敌人进入触发器时移除敌人并扣生命。
    private GameManager gameManager;

    void OnTriggerEnter(Collider other)
    {
        // 只有 Enemy 标签会被视为成功漏怪。
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.RemoveEnemy();

            if (gameManager == null)
                gameManager = FindFirstObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.UpdateHP(-1);
            }
        }
    }
}
