using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    // 建造系统总控：管理当前选中的建造格、建塔扣费、预览材质和取消建造。
    public WaveManager waveManager;
    public GridBuilder currentGrid;

    [SerializeField] private LayerMask whatToIgnore;

    [Header("Build Material")]
    [SerializeField] private Material attackRangeMat;
    [SerializeField] private Material buildPreviewMat;

    [Header("Build Details")]
    [SerializeField] private float towerCenterY = .5f;
    [SerializeField] private float cameraShakeDuration = .15f;
    [SerializeField] private float cameraShakeMagnitude = .02f;

    private bool isMouseOverUI;
    
    private UI ui;
    private BuildSlot selectedBuildSlot;
    private GameManager gameManager;
    private CameraEffects cameraEffects;

    void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();

        // 如果后续波次会改变某些地块，就提前禁止这些位置建塔，避免塔被地形变化覆盖。
        MakeSlotNotAvailableIfNeeded(waveManager, currentGrid);
    }

    void Start()
    {
        gameManager = GameManager.instance;
    }

    void Update()
    {
        // Esc 或点击非建造格区域时，取消当前建造选择。
        if (Input.GetKeyDown(KeyCode.Escape))
            CancelBuildAction();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (isMouseOverUI)
                return;

            // Collect hit collider info by using Raycast from the camera to the clicked mouse position
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, ~whatToIgnore))
            {
                // Return true or false whether BuildSlot component was found on the hit object
                bool isBuildSlotNotClicked = hit.collider.GetComponent<BuildSlot>() == null;

                if (isBuildSlotNotClicked)
                    CancelBuildAction();
            }
        }
    }

    public void UpdateBuildManager(WaveManager newWaveManager)
    {
        MakeSlotNotAvailableIfNeeded(newWaveManager, currentGrid);
    }

    public void BuildTower(GameObject towerToBuild, int towerPrice, Transform newTowerPreview)
    {
        // UI 建造按钮调用这里：先检查金币，再在当前选中格子上生成塔。
        if (gameManager.HasEnoughCurrency(towerPrice) == false)
        {
            ui.uiInGame.ShakeCurrencyUI();
            return;
        }

        if (towerToBuild == null)
        {
            Debug.LogWarning("Didn't tower assigned to this button");
            return;
        }

        // Check if we have the current selected button
        if (ui.uiBuildButton.GetLastSelectedButton() == null)
            return;

        Transform towerPreview = newTowerPreview;
        BuildSlot slotToUse = GetSelectedBuildSlot();
        CancelBuildAction();

        // 建造完成后把格子锁定，防止同一个位置重复建塔。
        slotToUse.SnapToDefaultPosition();
        slotToUse.SetSlotAvailable(false);

        ui.uiBuildButton.SetLastSelected(null, null);

        cameraEffects.ShakeScreen(cameraShakeDuration, cameraShakeMagnitude);

        GameObject newTower = Instantiate(towerToBuild, slotToUse.GetBuildPosition(towerCenterY), Quaternion.identity);
        newTower.transform.rotation = towerPreview.rotation;
    }

    public void MakeSlotNotAvailableIfNeeded(WaveManager waveManager, GridBuilder currentGrid)
    {
        // 对比当前地图和未来波次地图，凡是之后会变化的格子都不允许建造。
        foreach (var wave in waveManager.GetLevelWaves())
        {
            if (wave.waveGrid == null)
                continue;

            List<GameObject> grid = currentGrid.GetTileSetup();
            List<GameObject> nextWaveGrid = wave.waveGrid.GetTileSetup();

            for (int i = 0; i < grid.Count; i++)
            {
                TileSlot currentTile = grid[i].GetComponent<TileSlot>();
                TileSlot nextTile = nextWaveGrid[i].GetComponent<TileSlot>();

                bool isTileNotTheSame = currentTile.GetMesh() != nextTile.GetMesh() ||
                    currentTile.GetMaterial() != nextTile.GetMaterial() ||
                    currentTile.GetAllChildren().Count != nextTile.GetAllChildren().Count;

                if (isTileNotTheSame == false)
                    continue;

                if (grid[i].TryGetComponent<BuildSlot>(out var buildSlot))
                    buildSlot.SetSlotAvailable(false);
            }
        }
    }

    public void CancelBuildAction()
    {
        // 取消时关闭塔预览、还原格子高度，并隐藏建造按钮。
        if (selectedBuildSlot == null)
            return;
            
        ui.uiBuildButton.GetLastSelectedButton()?.TogglePreviewVisual(false);

        selectedBuildSlot.UnSelectTile();
        selectedBuildSlot = null;
        DisableBuildMenu();
    }

    public void SelectBuildSlot(BuildSlot buildSlot)
    {
        // 选中新格子前，先把上一个格子恢复。
        if (selectedBuildSlot != null)
            selectedBuildSlot.UnSelectTile();

        selectedBuildSlot = buildSlot;
    }

    public void EnableBuildMenu()
    {
        if (selectedBuildSlot != null)
            return;

        ui.uiBuildButton.ShowBuildButtons(true);
    }

    private void DisableBuildMenu()
    {
        ui.uiBuildButton.ShowBuildButtons(false);
    }

    public void OnMouseOverUI(bool isOverUI) => isMouseOverUI = isOverUI;

    public BuildSlot GetSelectedBuildSlot() => selectedBuildSlot;

    public Material GetAttackRangeMaterial() => attackRangeMat;

    public Material GetBuildPreviewMaterial() => buildPreviewMat;
}
