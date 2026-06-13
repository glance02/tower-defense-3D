using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TowerDrone : Tower
{
    // 无人机塔：轮流释放挂在塔上的多个无人机，自爆后再重新装填。
    [Header("Tower Drone Details")]
    [SerializeField] private float attackTimeMultiplier = .4f; // The percentage of the time used for attacking (40%)
    [SerializeField] private float reloadTimeMultiplier = .6f; // The percentage of the time used for reloading (60%)
    [SerializeField] private GameObject dronePrefab;
    [Space]

    [SerializeField] private Transform[] webSet;
    [SerializeField] private Transform[] attachPointSet;
    [SerializeField] private Transform[] attachPointRefSet;

    private int droneIndex;
    private Vector3 dronePointOffset = new(0, -.18f, 0);
    private GameObject[] activeDrones;

    protected override void Start()
    {
        // 开局先在每个挂点生成待发射无人机。
        base.Start();
        InitializeSpiders();
        reloadTimeMultiplier = 1 - attackTimeMultiplier;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateAttachPointsPosition();
    }

    // For testing without enemy
    // protected override bool CanAttack()
    // {
    //     return Time.time > lastTimeAttacked + attackCoolDown;
    // }

    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackCo());
    }

    private void UpdateAttachPointsPosition()
    {
        // 挂点参考物可能随模型动画移动，这里每帧同步真实挂点位置。
        for (int i = 0; i < attachPointSet.Length; i++)
        {
            attachPointSet[i].position = attachPointRefSet[i].position;
        }
    }
    
    private void InitializeSpiders()
    {
        // 预先从对象池取出无人机并挂到塔上，表现为待发射状态。
        activeDrones = new GameObject[attachPointSet.Length];

        for (int i = 0; i < activeDrones.Length; i++)
        {
            GameObject newDrone = objectPool.Get(dronePrefab, attachPointSet[i].position + dronePointOffset, Quaternion.identity, attachPointSet[i]);
            newDrone.SetActive(true);
            activeDrones[i] = newDrone;
        }
    }

    private IEnumerator AttackCo()
    {
        // 每次攻击只发射一个无人机，按数组下标轮流循环。
        Transform currentWeb = webSet[droneIndex];
        Transform currentAttachPoint = attachPointSet[droneIndex];

        // The cooldown is split into 4 parts (for 4 drones)
        // Each drone only use 1/4th of the time for attacking and reloading
        // So if attackCoolDown is 4, then each drone with take up one second
        float attackTime = (attackCoolDown / 4) * attackTimeMultiplier;
        float reloadTime = (attackCoolDown / 4) * reloadTimeMultiplier;

        // Attacking phase
        yield return ChangeScaleCo(currentWeb, 1, attackTime);
        activeDrones[droneIndex].GetComponent<ProjectileDrone>().SetupDrone(towerDamage);

        // Reloading phase
        yield return ChangeScaleCo(currentWeb, .1f, reloadTime);
        activeDrones[droneIndex] = objectPool.Get(dronePrefab, currentAttachPoint.position + dronePointOffset, Quaternion.identity, currentAttachPoint);
        activeDrones[droneIndex].SetActive(true);

        // Wraps around to 0 if it reaches the end
        droneIndex = (droneIndex + 1) % attachPointSet.Length;
    }

    public IEnumerator ChangeScaleCo(Transform obj, float newScale, float duration = .25f)
    {
        // 用缩放模拟蛛丝/挂载装置展开和收回。
        float time = 0;

        Vector3 initialScale = obj.localScale;
        Vector3 targetScale = new(1, newScale, 1);

        while (time < duration)
        {
            obj.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        obj.localScale = targetScale;
    }

}
