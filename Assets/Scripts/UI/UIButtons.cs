using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    // 普通 UI 按钮反馈：悬停缩放、点击音效，以及可选的文字闪烁控制。
    [SerializeField] private float showCaseScale = 1.1f;
    [SerializeField] private float scaleUpDuration = .25f;
    [SerializeField] private UITextBlinkEffect uITextBlinkEffect;

    private UIAnimator uIAnim;
    private RectTransform myRect;
    private Coroutine scaleRoutine;
    private UI ui;

    void Awake()
    {
        ui = GetComponentInParent<UI>();
        uIAnim = GetComponent<UIAnimator>();
        myRect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 悬停时停止文字闪烁并放大按钮，突出当前可点击项。
        // Stop another coroutine from starting
        // Avoid same coroutine stacking on top of each other
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        AudioManager.instance?.PlaySFX(ui.onHoverSFX);

        if (uITextBlinkEffect != null)
            uITextBlinkEffect.ToggleBlinkEffect(false);

        scaleRoutine = StartCoroutine(uIAnim.ChangeScaleCo(myRect, showCaseScale, scaleUpDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标离开后恢复原尺寸，并重新允许文字闪烁。
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        if (uITextBlinkEffect != null)
            uITextBlinkEffect.ToggleBlinkEffect(true);

        scaleRoutine = StartCoroutine(uIAnim.ChangeScaleCo(myRect, 1, scaleUpDuration));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 点击时立刻恢复缩放，避免按钮停在放大状态。
        AudioManager.instance?.PlaySFX(ui.onClickSFX);
        myRect.localScale = new(1, 1, 1);
    }
}
