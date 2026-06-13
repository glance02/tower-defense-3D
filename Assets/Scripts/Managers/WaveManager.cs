using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[System.Serializable]
public class WaveDetails
{
    public int basicEnemyCount;
    public int fastEnemyCount;
    public int swarmEnemyCount;
    public int heavyEnemyCount;
    public int stealthEnemyCount;
    public int flyingEnemyCount;
    public int flyingBossEnemyCount;
    public int spiderBossEnemyCount;
    public GridBuilder waveGrid;
}

public class WaveManager : MonoBehaviour
{
    // 波次管理器：根据每波配置准备敌人，控制下一波按钮，并在地图变化后重建 NavMesh。
    public EnemyPortal enemyPortal { get; private set; }

    [SerializeField] private GridBuilder currentGrid;
    [SerializeField] private NavMeshSurface flyingNavSurface;
    [SerializeField] private NavMeshSurface droneNavSurface;
    [SerializeField] private MeshCollider[] flyingNavColliders;
    [Space]

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemyPrefab;
    [SerializeField] private GameObject fastEnemyPrefab;
    [SerializeField] private GameObject swarmEnemyPrefab;
    [SerializeField] private GameObject heavyEnemyPrefab;
    [SerializeField] private GameObject stealthEnemyPrefab;
    [SerializeField] private GameObject flyingEnemyPrefab;
    [SerializeField] private GameObject flyingBossEnemyPrefab;
    [SerializeField] private GameObject spiderBossEnemyPrefab;

    [Header("Wave Settings")]
    [SerializeField] private int waveIndex;
    [SerializeField] private WaveDetails[] levelWaves;

    private bool isGameBegun;
    private bool isMakingNextWave;
    private bool isNextWaveButtonEnabled;
    private UIInGame uiInGame;
    private GameManager gameManager;
    private List<GameObject> enemyList;

    void Awake()
    {
        enemyPortal = FindFirstObjectByType<EnemyPortal>();
        gameManager = FindFirstObjectByType<GameManager>();
        uiInGame = FindFirstObjectByType<UIInGame>(FindObjectsInactive.Include);

        flyingNavColliders = GetComponentsInChildren<MeshCollider>();
    }

    public void ActivateWaveManager()
    {
        // GameManager 准备好关卡后，才允许玩家启动第一波。
        isGameBegun = true;
        uiInGame = gameManager.uiInGame;
        EnableNextWaveUI(true);
    }

    private void UpdateNavMeshes()
    {
        // 开新波前重建寻路数据，保证动态地形/建造后的路径是最新的。
        foreach (var collider in flyingNavColliders)
        {
            collider.enabled = true;
        }

        flyingNavSurface.BuildNavMesh();

        foreach (var collider in flyingNavColliders)
        {
            collider.enabled = false;
        }

        currentGrid.UpdateNewNavMesh();
        droneNavSurface.BuildNavMesh();
    }

    public void UpdateDroneNavMesh()
    {
        droneNavSurface.BuildNavMesh();
    }

    public void HandleWaveCompletion(int activeEnemyCount)
    {
        // EnemyPortal 每移除一个敌人都会回调这里；只有场上敌人清零才进入下一波判断。
        // Stop next wave when WaveManager is disabled
        if (isGameBegun == false)
            return;

        if (isMakingNextWave == true)
            return;

        if (activeEnemyCount > 0)
            return;

        isMakingNextWave = true;
        waveIndex++;

        if (HasNoMoreWave())
        {
            gameManager.CompleteLevel();
            return;
        }

        EnableNextWaveUI(true);
    }

    public void StartNewWave()
    {
        // 下一波按钮调用这里：先更新寻路，再把本波敌人交给传送门逐个生成。
        UpdateNavMeshes();
        GiveEnemiesToPortals();
        EnableNextWaveUI(false);
        isMakingNextWave = false;
    }

    public void EnableNextWaveUI(bool isEnable)
    {
        if (isNextWaveButtonEnabled == isEnable)
            return;

        isNextWaveButtonEnabled = isEnable; // To keep track of toggle status
        uiInGame.ToggleNextWaveButton(isEnable);
    }

    private void GiveEnemiesToPortals()
    {
        // WaveManager 不直接生成敌人，只把“要生成的敌人列表”交给 EnemyPortal。
        enemyList = GetNewEnemies();

        if (enemyList == null)
        {
            Debug.Log("No more wave to setup");
            return;
        }

        for (int i = 0; i < enemyList.Count; i++)
        {
            GameObject enemyToAdd = enemyList[i];
            enemyPortal.AddEnemy(enemyToAdd);
        }

        uiInGame.UpdateEnemyCountText(enemyList.Count);
    }

    public void DecreaseEnemyAmount()
    {
        // UI 上显示的是本波剩余敌人数量。
        int remainingEnemies = enemyList.Count - gameManager.GetKilledEnemies();
        uiInGame.UpdateEnemyCountText(remainingEnemies);
    }

    private List<GameObject> GetNewEnemies()
    {
        // 根据 Inspector 里配置的数量，把不同敌人 prefab 展平成一个生成列表。
        // Check if there are still more waves available
        if (waveIndex >= levelWaves.Length)
            return null;

        List<GameObject> enemyList = new();

        for (int i = 0; i < levelWaves[waveIndex].basicEnemyCount; i++)
            enemyList.Add(basicEnemyPrefab);

        for (int i = 0; i < levelWaves[waveIndex].fastEnemyCount; i++)
            enemyList.Add(fastEnemyPrefab);

        for (int i = 0; i < levelWaves[waveIndex].swarmEnemyCount; i++)
            enemyList.Add(swarmEnemyPrefab);

        for (int i = 0; i < levelWaves[waveIndex].heavyEnemyCount; i++)
            enemyList.Add(heavyEnemyPrefab);

        for (int i = 0; i < levelWaves[waveIndex].stealthEnemyCount; i++)
            enemyList.Add(stealthEnemyPrefab);

        for (int i = 0; i < levelWaves[waveIndex].flyingEnemyCount; i++)
            enemyList.Add(flyingEnemyPrefab);

        for (int i = 0; i < levelWaves[waveIndex].flyingBossEnemyCount; i++)
            enemyList.Add(flyingBossEnemyPrefab);

        for (int i = 0; i < levelWaves[waveIndex].spiderBossEnemyCount; i++)
            enemyList.Add(spiderBossEnemyPrefab);

        return enemyList;
    }

    public WaveDetails[] GetLevelWaves() => levelWaves;

    public int GetEnemyCount() => enemyList.Count;

    private bool HasNoMoreWave() => waveIndex >= levelWaves.Length;

    public void DeactivateWaveManager() => isGameBegun = false;
}
