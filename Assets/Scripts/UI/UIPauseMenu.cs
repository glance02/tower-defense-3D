using UnityEngine;

public class UIPauseMenu : MonoBehaviour
{
    // 暂停菜单：打开时暂停游戏时间，内部可切换设置/暂停子面板。
    [SerializeField] private GameObject[] pauseUIElements;

    private UI ui;
    private UIInGame inGameUI;

    void Awake()
    {
        ui = GetComponentInParent<UI>();
        inGameUI = ui.GetComponentInChildren<UIInGame>(true);
    }

    void Update()
    {
        // 暂停菜单里再按 Esc 返回游戏 HUD。
        if (Input.GetKeyDown(KeyCode.Escape))
            ui.SwitchUIElement(inGameUI.gameObject);
    }

    public void SwitchPauseUIElements(GameObject uiElement)
    {
        // 暂停菜单内部同样只显示一个子面板。
        foreach (GameObject obj in pauseUIElements)
        {
            obj.SetActive(false);
        }

        uiElement.SetActive(true);
    }

    void OnEnable()
    {
        // 暂停大部分 gameplay 更新，但 UIAnimator 使用 unscaledDeltaTime 的动画仍可运行。
        Time.timeScale = 0;
    }

    void OnDisable()
    {
        // 退出暂停菜单后恢复游戏时间。
        Time.timeScale = 1;
    }
}
