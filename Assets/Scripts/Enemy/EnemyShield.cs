using System.Collections;
using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    // 重甲敌人的护盾视觉：受击时缩放并提高 Fresnel 发光。
    [Header("Impact Details")]
    [SerializeField] private float defaultShieldGlow = 1;
    [SerializeField] private float impactShieldGlow = 3;
    [SerializeField] private float impactSpeed;
    [SerializeField] private float impactResetDur = .1f;
    [SerializeField] private float impactScaleMulti = .97f;
    [SerializeField] private Material shieldMat;

    private float defaultScale;
    private string fresnelPara = "_FresnelPower";
    private Coroutine currentCo;

    void Start()
    {
        // 实例化材质，避免一个敌人的护盾受击影响所有护盾。
        defaultScale = transform.localScale.x;
        shieldMat = Instantiate(shieldMat);
    }

    public void ActivateShieldImpact()
    {
        // 连续受击时重启反馈协程，让冲击表现更及时。
        if (currentCo != null)
            StopCoroutine(currentCo);

        currentCo = StartCoroutine(ImpactCo());
    }

    private IEnumerator ImpactCo()
    {
        // 先快速变亮/收缩，再恢复默认状态。
        yield return StartCoroutine(ShieldChangeCo(impactShieldGlow, impactSpeed, defaultScale * impactScaleMulti));
        StartCoroutine(ShieldChangeCo(defaultShieldGlow, impactResetDur, defaultScale));
    }

    private IEnumerator ShieldChangeCo(float targetGlow, float glowDuration, float targetScale)
    {
        // 同时插值缩放和 shader 参数。
        float time = 0;
        float startGlow = shieldMat.GetFloat(fresnelPara);
        Vector3 initialScale = transform.localScale;
        Vector3 newScale = new(targetScale, targetScale, targetScale);

        while (time < glowDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, newScale, time / glowDuration);

            float newGlow = Mathf.Lerp(startGlow, targetGlow, time / glowDuration);
            shieldMat.SetFloat(fresnelPara, newGlow);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = newScale;
        shieldMat.SetFloat(fresnelPara, targetGlow);
    }
}
