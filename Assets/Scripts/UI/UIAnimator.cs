using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimator : MonoBehaviour
{
    // 通用 UI 动画工具：位置、缩放、透明度和抖动反馈都集中在这里。
    [Header("UI Feedbacl - Shake Effect")]
    [SerializeField] private float shakeMagnitude;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeRotationMagnitude;
    [Space]
    [SerializeField] private float defaultUIScale = 1.5f;
    [SerializeField] private bool scaleChangeAvailable;
    

    public void StartChangePosition(Transform transform, Vector3 offset, float duration = .1f)
    {
        // 对外传 Transform 更方便，内部转 RectTransform 后播放协程。
        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        StartCoroutine(ChangePositionCo(rectTransform, offset, duration));
    }

    public void StartChangeScale(Transform transform, float targetScale, float duration = .25f)
    {
        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        StartCoroutine(ChangeScaleCo(rectTransform, targetScale, duration));
    }

    public void StartChangeColor(Image image, float targetAlpha, float duration)
    {
        StartCoroutine(ChangeColorCo(image, targetAlpha, duration));
    }

    public void StartShake(Transform transformToShake)
    {
        RectTransform rectTransform = transformToShake.GetComponent<RectTransform>();
        StartCoroutine(ShakeCo(rectTransform));
    }

    public IEnumerator ChangePositionCo(RectTransform rectTransform, Vector3 offset, float duration = .1f)
    {
        // anchoredPosition 适合 UI 元素在 Canvas 内移动。
        float time = 0;

        Vector3 initialPos = rectTransform.anchoredPosition;
        Vector3 targetPos = initialPos + offset;

        while (time < duration)
        {
            rectTransform.anchoredPosition = Vector3.Lerp(initialPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
    }

    public IEnumerator ChangeScaleCo(RectTransform rectTransform, float newScale, float duration = .25f)
    {
        // 用 unscaledDeltaTime，暂停菜单 Time.timeScale 为 0 时 UI 动画仍能播放。
        float time = 0;
        Vector3 initialScale = rectTransform.localScale;
        Vector3 targetScale = new(newScale, newScale, newScale);

        while (time < duration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.localScale = targetScale;
    }

    private IEnumerator ChangeColorCo(Image image, float targetAlpha, float duration)
    {
        // 只改 alpha，保留原本 RGB 颜色。
        float time = 0;
        Color currentcolor = image.color;
        float startAlpha = currentcolor.a;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            image.color = new(currentcolor.r, currentcolor.g, currentcolor.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        image.color = new(currentcolor.r, currentcolor.g, currentcolor.b, targetAlpha);
    }

    private IEnumerator ShakeCo(RectTransform rectTransform)
    {
        // 抖动用于金币不足、受伤等反馈，结束后恢复原位置和旋转。
        float time = 0;
        Vector3 originalPosition = rectTransform.anchoredPosition;

        float currentScale = rectTransform.localScale.x;

        if (scaleChangeAvailable)
            StartCoroutine(ChangeScaleCo(rectTransform, currentScale * 1.1f, shakeDuration / 2));

        while (time < shakeDuration)
        {
            float xOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            float yOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            float randomRotation = Random.Range(-shakeRotationMagnitude, shakeRotationMagnitude);

            rectTransform.anchoredPosition = originalPosition + new Vector3(xOffset, yOffset);
            rectTransform.localRotation = Quaternion.Euler(0, 0, randomRotation);

            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
        

        if (scaleChangeAvailable)
            StartCoroutine(ChangeScaleCo(rectTransform, defaultUIScale, shakeDuration / 2));
    }
}
