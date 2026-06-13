using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    // UI 总入口：缓存各个 UI 子模块，并负责不同 UI 面板之间的互斥切换。
    [SerializeField] private GameObject[] uiElement;
    [SerializeField] private Image fadeImageUI;

    [Header("UI SFX")]
    public AudioSource onHoverSFX;
    public AudioSource onClickSFX;

    private UISetting uiSetting;
    private UIMainMenu uiMainMenu;

    public UIInGame uiInGame { get; private set; }
    public UIAnimator uiAnimator { get; private set; }
    public UIBuildButtonsHolder uiBuildButton { get; private set; }

    void Awake()
    {
        // true 表示即使子物体未激活，也能在初始化时找到对应 UI 组件。
        uiBuildButton = GetComponentInChildren<UIBuildButtonsHolder>(true);
        uiSetting = GetComponentInChildren<UISetting>(true);
        uiMainMenu = GetComponentInChildren<UIMainMenu>(true);
        uiInGame = GetComponentInChildren<UIInGame>(true);
        uiAnimator = GetComponent<UIAnimator>();

        // ActivateFadeEffect(true);
        SwitchUIElement(uiSetting.gameObject);
        SwitchUIElement(uiMainMenu.gameObject);

        if (GameManager.instance.IsTestingLevel())
            SwitchUIElement(uiInGame.gameObject);   
    }

    public void EnableMainMenuUI(bool isEnable)
    {
        // 主菜单显示时会关闭其他 UI；隐藏时传 null 表示全部关闭。
        if (isEnable)
            SwitchUIElement(uiMainMenu.gameObject);
        else
            SwitchUIElement(null);
    }

    public void EnableInGameUI(bool isEnable)
    {
        // 离开游戏 UI 时顺便把下一波按钮位置重置，避免下次进入残留偏移。
        if (isEnable)
            SwitchUIElement(uiInGame.gameObject);
        else
        {
            uiInGame.DefaultNextWaveButonPos();
            SwitchUIElement(null);
        }
    }

    public void SwitchUIElement(GameObject uiToEnable)
    {
        // 这个项目同一时间只显示一个主面板，所以先全部隐藏再开启目标面板。
        foreach (GameObject element in uiElement)
        {
            element.SetActive(false);
        }

        if (uiToEnable != null)
            uiToEnable.SetActive(true);
    }

    public void QuitToDesktop()
    {
        // 编辑器里退出播放模式；打包后退出应用。
        if (EditorApplication.isPlaying)
            EditorApplication.isPlaying = false;
        else
            Application.Quit();
    }

    public void ActivateFadeEffect(bool fadeIn)
    {
        // 使用 UIAnimator 改变黑幕透明度，预留给转场淡入淡出。
        if (fadeIn)
            uiAnimator.StartChangeColor(fadeImageUI, 0, 2);
        else
            uiAnimator.StartChangeColor(fadeImageUI, 1, 2);
    }
}
