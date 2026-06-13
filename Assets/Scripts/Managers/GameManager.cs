using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 全局游戏状态管理器：负责金币、生命值、胜负流程，以及连接 UI 和波次系统。
    public static GameManager instance;

    public int totalCurrency;

    public UIInGame uiInGame { get; private set; }
    public WaveManager activeWaveManager { get; private set; }

    [SerializeField] private int maxHP;
    [SerializeField] private int currentHP;

    private bool isGameLost;
    private int enemiesKilled;
    private CameraEffects cameraEffects;
    private LevelManager levelManager;
    private EnemyPortal enemyPortal;

    void Awake()
    {
        instance = this;

        uiInGame = FindFirstObjectByType<UIInGame>(FindObjectsInactive.Include);
        levelManager = FindFirstObjectByType<LevelManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        activeWaveManager = FindFirstObjectByType<WaveManager>();
    }

    void Start()
    {
        // 测试场景下给大量资源，方便直接验证关卡逻辑，不受正式数值限制。
        if (IsTestingLevel())
        {
            maxHP += 9999;
            totalCurrency += 9999;
            PrepareLevel();
        }
    }

    public void CompleteLevel()
    {
        StartCoroutine(CompleteLevelCo());
    }

    public void PrepareLevel()
    {
        // 每次进入/重开关卡时，把核心状态重置到一局游戏刚开始的样子。
        isGameLost = false;
        enemiesKilled = 0;
        currentHP = maxHP;

        enemyPortal = activeWaveManager.enemyPortal;

        uiInGame.UpdateCurrencyText(totalCurrency);
        uiInGame.UpdateHealthPointUIText(currentHP, maxHP);
        uiInGame.UpdateEnemyCountText(0);

        activeWaveManager.ActivateWaveManager();
    }

    public void UpdateHP(int changeValue)
    {
        // changeValue 通常是负数；敌人进城堡时扣血。
        currentHP += changeValue;
        uiInGame.UpdateHealthPointUIText(currentHP, maxHP);
        uiInGame.ShakeHealthUI();

        if (currentHP <= 0 && isGameLost == false)
            StartCoroutine(FailLevel());
    }

    public void IncreaseCurrencyFromKill(int changeValue)
    {
        totalCurrency += changeValue;
        uiInGame.UpdateCurrencyText(totalCurrency);
    }

    public bool HasEnoughCurrency(int price)
    {
        // 建塔前先检查金币；成功时这里直接扣除，避免外部重复扣费。
        if (price <= totalCurrency)
        {
            totalCurrency -= price;
            uiInGame.UpdateCurrencyText(totalCurrency);
            return true;
        }

        return false;
    }

    public IEnumerator FailLevel()
    {
        // 失败时停止出怪，镜头转向城堡，再显示失败 UI。
        isGameLost = true;
        activeWaveManager.DeactivateWaveManager();
        cameraEffects.FocusOnCastle();

        yield return cameraEffects.GetActiveCameraCo();

        uiInGame.EnableGameOverUI(true);
    }

    public IEnumerator CompleteLevelCo()
    {
        // 胜利流程与失败类似：先切镜头，再弹出胜利界面。
        cameraEffects.FocusOnCastle();
        activeWaveManager.DeactivateWaveManager();

        yield return cameraEffects.GetActiveCameraCo();

        uiInGame.EnableVictoryUI(true);
    }

    public bool IsTestingLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("Test"))
            return true;

        return false;
    }

    public void IncreaseKilledEnemy() => enemiesKilled++;

    public int GetKilledEnemies() => enemiesKilled;
}
