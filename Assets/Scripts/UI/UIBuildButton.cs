using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 单个建塔按钮：保存塔 prefab/价格，控制预览显示，并最终通知 BuildManager 建造。
    [SerializeField] private string towerName;
    [SerializeField] private int towerPrice = 50;
    [SerializeField] private float towerAttackRange = 3;
    [SerializeField] private GameObject towerToBuild;
    [Space]

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerPriceText;

    private UI ui;
    private BuildManager buildManager;
    private UIBuildButtonsHolder buildButtonHolder;
    private UIBuildButtonHoverEffect onHoverEffect;

    private TowerPreview towerPreview;

    public bool isUnlocked { get; private set; }

    void Awake()
    {
        // 从 towerToBuild 读取攻击范围，避免按钮上的数值和真实塔不一致。
        onHoverEffect = GetComponent<UIBuildButtonHoverEffect>();

        ui = GetComponentInParent<UI>();
        buildButtonHolder = GetComponentInParent<UIBuildButtonsHolder>();

        buildManager = FindFirstObjectByType<BuildManager>();

        if (towerToBuild != null)
            towerAttackRange = towerToBuild.GetComponent<Tower>().GetAttackRange();
    }

    void Start()
    {
        // 每个按钮启动时都创建自己的塔预览对象，之后只负责显示/隐藏。
        CreateTowerPreview();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标进入 UI 时通知 BuildManager，避免点击按钮同时取消建造。
        buildManager.OnMouseOverUI(true);

        // Turn off the preview visual for other button in UIBuildButtonHolder
        foreach (var button in buildButtonHolder.GetBuildButtons())
        {
            if (button.gameObject.activeSelf)
                button.TogglePreviewVisual(false);
        }

        // Toggle the visual for the selected button
        TogglePreviewVisual(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildManager.OnMouseOverUI(false);
    }

    // Toggle tower visual preview during tower placement
    public void TogglePreviewVisual(bool isSelect)
    {
        // 只有已经选中建造格时，按钮预览才有具体摆放位置。
        BuildSlot buildSlot = buildManager.GetSelectedBuildSlot();

        if (buildSlot == null)
            return;

        Vector3 previewPosition = buildSlot.GetBuildPosition(.5f);

        towerPreview.gameObject.SetActive(isSelect);
        towerPreview.ShowPreview(isSelect, previewPosition);
        onHoverEffect.ShowButton(isSelect);
        buildButtonHolder.SetLastSelected(this, towerPreview.transform);
    }

    // Create a preview game object version of a tower
    private void CreateTowerPreview()
    {
        // 直接实例化 tower prefab 作为预览，再由 TowerPreview 清掉真实逻辑组件。
        GameObject newPreview = Instantiate(towerToBuild, Vector3.zero, Quaternion.identity);

        // Add TowerPreview component to newPreview and store TowerPreview in towerPreview for future use
        towerPreview = newPreview.AddComponent<TowerPreview>();
        towerPreview.SetupTowerPreview(newPreview);
        towerPreview.transform.parent = buildManager.transform;
    }

    public void UnlockTower(string towerNameToCheck, bool unlockStatus)
    {
        // 预留给关卡/升级系统：按塔名解锁或隐藏按钮。
        if (towerNameToCheck != towerName)
            return;

        isUnlocked = unlockStatus;
        gameObject.SetActive(unlockStatus);
    }

    public void ConfirmTowerBuilding()
    {
        // 空格、鼠标点击或按钮事件都会走这里完成建造。
        buildManager.BuildTower(towerToBuild, towerPrice, towerPreview.transform);
    }

    private void OnValidate()
    {
        // Inspector 中改塔名/价格时，同步按钮文本和对象名。
        towerNameText.text = towerName;
        towerPriceText.text = towerPrice + "";
        gameObject.name = "Build Button - " + towerName;
    }

}
