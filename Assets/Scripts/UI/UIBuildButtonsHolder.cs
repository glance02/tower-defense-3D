using System.Collections.Generic;
using UnityEngine;

public class UIBuildButtonsHolder : MonoBehaviour
{
    // 建造按钮容器：负责展开/收起菜单、数字快捷键、确认建造和旋转预览。
    [SerializeField] private float yPosOffset;
    [SerializeField] private float openAnimDuration = .1f;
    [SerializeField] private List<UIBuildButton> unlockedButtons;

    private bool isBuildMenuActive;
    private UIAnimator uIAnimator;
    private UIBuildButtonHoverEffect[] buildButtonEffects;
    private UIBuildButton[] buildButtons;

    private UIBuildButton lastSelectedButton;
    private Transform towerPreview;

    void Awake()
    {
        uIAnimator = GetComponentInParent<UIAnimator>();
        buildButtonEffects = GetComponentsInChildren<UIBuildButtonHoverEffect>();
        buildButtons = GetComponentsInChildren<UIBuildButton>();
    }

    void Update()
    {
        CheckBuildButtonHotkey();
    }

    private void CheckBuildButtonHotkey()
    {
        // 建造菜单打开时，数字键 1/2/3... 可以快速选择已解锁塔。
        if (isBuildMenuActive == false)
            return;

        for (int i = 0; i < unlockedButtons.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectNewButton(i);
                    break;
                }
            }

        if (lastSelectedButton != null)
        {
            // Space/左键确认建造，Q/E 旋转当前塔预览。
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                lastSelectedButton.ConfirmTowerBuilding();
                towerPreview = null;
            }

            if (Input.GetKeyDown(KeyCode.Q))
                RotateTarget(towerPreview, -90);

            if (Input.GetKeyDown(KeyCode.E))
                RotateTarget(towerPreview, 90);
        }
    }

    private void RotateTarget(Transform target, float angle)
    {
        // 旋转前方攻击塔时，同步更新射程线方向。
        if (target == null)
            return;

        target.Rotate(0, angle, 0);

        if (target.TryGetComponent<ForwardAttackDisplay>(out var attackVisual))
            attackVisual.UpdateLines();

    }

    public void SelectNewButton(int buttonIndex)
    {
        if (buttonIndex >= unlockedButtons.Count)
            return;

        foreach (var button in unlockedButtons)
        {
            button.TogglePreviewVisual(false);
        }

        UIBuildButton selectedButton = unlockedButtons[buttonIndex];
        selectedButton.TogglePreviewVisual(true);
    }

    public void UpdateUnlockedButton()
    {
        // 扫描所有按钮，把当前已解锁按钮缓存给快捷键逻辑使用。
        unlockedButtons = new List<UIBuildButton>();

        foreach (var button in buildButtons)
        {
            if (button.isUnlocked)
                unlockedButtons.Add(button);
        }
    }

    public void ShowBuildButtons(bool enable)
    {
        // BuildSlot 被选中/取消时调用，移动整个按钮条并启停按钮浮动效果。
        isBuildMenuActive = enable;

        float changeYOffset = isBuildMenuActive ? yPosOffset : -yPosOffset;
        float methodDelay = isBuildMenuActive ? openAnimDuration : 0;

        uIAnimator.StartChangePosition(transform, new(0, changeYOffset), openAnimDuration);
        Invoke(nameof(ToggleButtonMovement), methodDelay);
    }

    private void ToggleButtonMovement()
    {
        foreach (var button in buildButtonEffects)
        {
            button.ToggleMovement(isBuildMenuActive);
        }
    }

    public void SetLastSelected(UIBuildButton newLastSelected, Transform newPreview)
    {
        lastSelectedButton = newLastSelected;
        towerPreview = newPreview;
    }

    public UIBuildButton[] GetBuildButtons() => buildButtons;

    public List<UIBuildButton> GetUnlockedButtons() => unlockedButtons;

    public UIBuildButton GetLastSelectedButton() => lastSelectedButton;
}
