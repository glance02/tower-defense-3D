using TMPro;
using UnityEngine;

public class UIInGame : MonoBehaviour
{
    // 游戏内 HUD：显示血量、金币、剩余敌人、下一波按钮和胜负面板。
    [SerializeField] private TextMeshProUGUI healthPointsText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [Space]

    [SerializeField] private float nextWaveButtonOffset;
    [SerializeField] private UITextBlinkEffect nextWaveButtonTextBlinkEffect;
    [SerializeField] private Transform nextWaveButtonTrans;
    [SerializeField] private Coroutine nextWaveButtonMoveRoutine;

    [Header("Victory & Defeat")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject defeatUI;

    private UI ui;
    private UIAnimator uIAnimator;
    private UIPauseMenu uiPauseMenu;
    private Vector3 nextWaveButtonDefaultPos; 

    void Awake()
    {
        ui = GetComponentInParent<UI>();
        uIAnimator = GetComponentInParent<UIAnimator>();
        uiPauseMenu = ui.GetComponentInChildren<UIPauseMenu>(true);

        if (nextWaveButtonTrans != null)
            nextWaveButtonDefaultPos = nextWaveButtonTrans.localPosition;
    }

    void Update()
    {
        // 游戏内按 Esc 打开暂停菜单。
        if (Input.GetKeyDown(KeyCode.Escape))
            ui.SwitchUIElement(uiPauseMenu.gameObject);
    }

    public void EnableVictoryUI(bool isEnable)
    {
        if (victoryUI != null)
            victoryUI.SetActive(isEnable);
    }

    public void EnableGameOverUI(bool isEnable)
    {
        if (defeatUI != null)
            defeatUI.SetActive(isEnable);
    }

    public void UpdateHealthPointUIText(int changeValue, int maxValue)
    {
        // 这里显示的是 Threat，数值越高代表已经漏掉的敌人越多。
        int newValue = maxValue - changeValue;
        healthPointsText.text = "Threat : " + newValue + "/" + maxValue;
    }

    public void ToggleNextWaveButton(bool enable)
    {
        // 下一波按钮通过上下移动进入/离开屏幕。
        RectTransform nextWaveButtonTransform = nextWaveButtonTrans.GetComponent<RectTransform>();
        
        float yOffset = enable ? -nextWaveButtonOffset : nextWaveButtonOffset;
        Vector3 offset = new(0, yOffset, 0);

        nextWaveButtonMoveRoutine = StartCoroutine(uIAnimator.ChangePositionCo(nextWaveButtonTransform, offset));
        nextWaveButtonTextBlinkEffect.ToggleBlinkEffect(true);
    }

    public void DefaultNextWaveButonPos()
    {
        if (nextWaveButtonTrans == null)
            return;

        if (nextWaveButtonMoveRoutine != null)
            StopCoroutine(nextWaveButtonMoveRoutine);

        nextWaveButtonTrans.localPosition = nextWaveButtonDefaultPos;
    }

    public void ActivateNextWave()
    {
        // 按钮事件入口：找到当前 WaveManager 并启动下一波。
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.StartNewWave();
    }

    public void UpdateEnemyCountText(int remainingEnemy) => enemyCountText.text = "Enemies : " + remainingEnemy;

    public void UpdateCurrencyText(int changeCurrency) => currencyText.text = "Currency : " + changeCurrency;

    public void ShakeCurrencyUI() => ui.uiAnimator.StartShake(currencyText.transform.parent);

    public void ShakeHealthUI() => ui.uiAnimator.StartShake(healthPointsText.transform.parent);

}
