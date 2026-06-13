using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // 关卡流程管理器：负责主菜单/游戏场景切换、清场、重开和异步加载关卡。
    private UI ui;
    private TileAnimator tileAnimator;
    private CameraEffects cameraEffects;
    private GridBuilder currentActiveGrid;

    public string currentSceneName { get; private set; }

    void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        currentActiveGrid = FindFirstObjectByType<GridBuilder>();
    }

    IEnumerator Start()
    {
        // 开场先等主菜单地图动画完成，再显示主菜单 UI。
        ui.EnableMainMenuUI(false);

        yield return tileAnimator.GetCurrentActiveRoutine();

        ui.EnableMainMenuUI(true);
    }

    private void RemoveAllEnemies()
    {
        // 返回菜单或重开前清掉场上所有敌人，并走敌人自身的回收逻辑。
        Enemy[] enemiesArray = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemiesArray)
        {
            enemy.RemoveEnemy();
        }
    }

    private void RemoveAllTowers()
    {
        // 塔不是对象池对象，直接销毁。
        Tower[] towersArray = FindObjectsByType<Tower>(FindObjectsSortMode.None);

        foreach (Tower tower in towersArray)
        {
            Destroy(tower.gameObject);
        }
    }

    private void CleanUpScene()
    {
        // 切换流程前统一清理敌人、塔和旧 NavMesh 数据。
        RemoveAllEnemies();
        RemoveAllTowers();

        if (currentActiveGrid != null)
            currentActiveGrid.ClearNavMeshData();
    }

    private IEnumerator LoadLevelFromMenuCo()
    {
        // 从主菜单进入当前场景：清场、切镜头、等地图动画，然后正式准备关卡。
        CleanUpScene();
        ui.EnableMainMenuUI(false);
        cameraEffects.SwitchToGameView();

        yield return tileAnimator.GetCurrentActiveRoutine();

        ui.EnableInGameUI(true);
        cameraEffects.EnableCameraEffect();

        GameManager.instance.PrepareLevel();
    }

    private IEnumerator LoadMainMenuCo()
    {
        // 回到主菜单时隐藏游戏 HUD，等镜头过渡结束后显示主菜单。
        CleanUpScene();
        ui.EnableInGameUI(false);

        yield return cameraEffects.GetActiveCameraCo();

        ui.EnableMainMenuUI(true);
    }

    private IEnumerator LoadLevelCo(string sceneName)
    {
        // 重开/切关卡：先清当前内容，再卸载旧场景并异步加载目标场景。
        CleanUpScene();
        ui.EnableInGameUI(false);
        cameraEffects.SwitchToGameView();

        yield return tileAnimator.GetCurrentActiveRoutine();

        UnloadCurrentScene();
        LoadScene(sceneName);
    }

    public void LoadMainMenu() => StartCoroutine(LoadMainMenuCo());

    public void LoadLevelFromMenu() => StartCoroutine(LoadLevelFromMenuCo());

    public void RestartGame() => StartCoroutine(LoadLevelCo(currentSceneName));

    public void UpdateCurrentGrid(GridBuilder newGrid) => currentActiveGrid = newGrid;

    // Unload a scene in the background as the current scene run asynchronously
    public void UnloadCurrentScene() => SceneManager.UnloadSceneAsync(currentSceneName);

    // Load a scene in the background as the current scene run asynchronously
    private void LoadScene(string sceneName)
    {
        // Additive 加载让 UI、管理器等常驻对象可以留在主场景中。
        currentSceneName = sceneName;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }
}
