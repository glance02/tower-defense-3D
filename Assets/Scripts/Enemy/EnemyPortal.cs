using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPortal : MonoBehaviour
{
    // 敌人传送门：保存路点，按冷却从本波列表里随机取敌人并生成。
    [SerializeField] private float spawnCoolDown;
    [SerializeField] private WaveManager myWaveManager;
    [SerializeField] private List<Waypoint> waypointList;
    [Space]

    [SerializeField] private ParticleSystem flyPortalFX;

    private float spawnTimer;
    private Coroutine flyPortalFXCo;
    private ObjectPoolManager objectPool;
    private GameManager myGameManager;
    private List<GameObject> enemiesToCreate = new();
    private List<GameObject> activeEnemies = new();

    public Vector3[] currentWaypoints { get; private set; }

    void Awake()
    {
        CollectWaypoints();

        if (myGameManager == null)
            myGameManager = FindFirstObjectByType<GameManager>();

        if (myWaveManager == null)
            myWaveManager = FindFirstObjectByType<WaveManager>();
    }

    void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    void Update()
    {
        // 只要待生成列表还有敌人，就按 spawnCoolDown 一个个放出来。
        if (CanMakeNewEnemy())
            SpawnEnemy();
    }

    private void PlaceEnemyAtFlyPortalIfNeeded(GameObject newEnemy, EnemyType enemyType)
    {
        // 飞行单位从专门的飞行入口出现，普通地面单位从当前传送门出现。
        if (enemyType != EnemyType.Flying)
            return;

        if (flyPortalFXCo != null)
            StopCoroutine(flyPortalFXCo);

        flyPortalFXCo = StartCoroutine(PlayFlyPortalFXCo());
        newEnemy.transform.position = flyPortalFX.transform.position;
    }

    private bool CanMakeNewEnemy()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0 && enemiesToCreate.Count > 0)
        {
            spawnTimer = spawnCoolDown;
            return true;
        }

        return false;
    }

    private GameObject GetRandomEnemy()
    {
        // 同一波内随机生成不同类型敌人，避免固定顺序太机械。
        int randomIndex = Random.Range(0, enemiesToCreate.Count);
        GameObject choosenEnemy = enemiesToCreate[randomIndex];
        enemiesToCreate.Remove(choosenEnemy);
        return choosenEnemy;
    }

    private void SpawnEnemy()
    {
        // 从对象池取敌人，而不是直接 Instantiate，减少运行时 GC 和卡顿。
        GameObject randomEnemy = GetRandomEnemy();
        GameObject newEnemy = objectPool.Get(randomEnemy, transform.position, Quaternion.identity);

        Enemy enemy = newEnemy.GetComponent<Enemy>();
        enemy.SetupEnemyWaypoint(this);
        
        PlaceEnemyAtFlyPortalIfNeeded(newEnemy, enemy.GetEnemyType());

        newEnemy.SetActive(true);
        activeEnemies.Add(newEnemy);
    }

    private void CollectWaypoints()
    {
        // 传送门子物体上的 Waypoint 决定敌人的行进路线。
        // Make a new waypoint list each time
        waypointList = new();

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<Waypoint>(out var waypoint))
                waypointList.Add(waypoint);
        }

        currentWaypoints = new Vector3[waypointList.Count];

        // Collect all waypoints only once instead of on each enemy
        for (int i = 0; i < currentWaypoints.Length; i++)
        {
            currentWaypoints[i] = waypointList[i].transform.position;
        }
    }

    public void RemoveActiveEnemy(GameObject enemyToRemove)
    {
        // 敌人死亡或进城堡都会从活动列表移除，并通知波次系统检查是否结束。
        activeEnemies.Remove(enemyToRemove);
        myGameManager.IncreaseKilledEnemy();
        myWaveManager.DecreaseEnemyAmount();
        myWaveManager.HandleWaveCompletion(activeEnemies.Count);
    }

    private IEnumerator PlayFlyPortalFXCo()
    {
        flyPortalFX.Play();

        yield return new WaitForSeconds(2);

        flyPortalFX.Stop();
    }

    public void AssignWaveManager(WaveManager waveManager) => myWaveManager = waveManager;

    public List<GameObject> GetActiveEnemies() => activeEnemies;

    public void AddEnemy(GameObject enemy) => enemiesToCreate.Add(enemy);

}
