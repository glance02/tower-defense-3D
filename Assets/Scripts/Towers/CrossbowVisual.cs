using System.Collections;
using UnityEngine;

public class CrossbowVisual : MonoBehaviour
{
    // 弩塔视觉表现：负责光束、命中特效、发光充能、弦线和转轮装填动画。
    [Header("Attack Visuals")]
    [SerializeField] private float attackVisualDur = .1f;
    [SerializeField] private LineRenderer attackLineVisual;
    [SerializeField] private GameObject onHitFX;
    private Vector3 hitPoint;
    [Space]

    [Header("Tower Head Emission Visual")]
    [SerializeField] private float maxIntensity = 150f;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private LineRenderer[] lineRenderers;
    private float currentIntensity;
    [Space]

    [Header("Front String Visual")]
    [SerializeField] private LineRenderer frontLine_L;
    [SerializeField] private LineRenderer frontLine_R;
    [SerializeField] private Transform frontStartPoint_L;
    [SerializeField] private Transform frontStartPoint_R;
    [SerializeField] private Transform frontEndPoint_L;
    [SerializeField] private Transform frontEndPoint_R;
    [Space]

    [Header("Back String Visual")]
    [SerializeField] private LineRenderer backLine_L;
    [SerializeField] private LineRenderer backLine_R;
    [SerializeField] private Transform backStartPoint_L;
    [SerializeField] private Transform backStartPoint_R;
    [SerializeField] private Transform backEndPoint_L;
    [SerializeField] private Transform backEndPoint_R;
    [Space]

    [Header("Rotor Visual")]
    [SerializeField] private Transform rotor;
    [SerializeField] private Transform rotorUnloadPoint;
    [SerializeField] private Transform rotorLoadPoint;
    [Space]

    private TowerCrossbow mainTower;
    private Material materialInstance;
    private ObjectPoolManager objectPool;

    void Awake()
    {
        // 为当前塔创建独立材质实例，避免修改 emission 时影响其他弩塔。
        mainTower = GetComponent<TowerCrossbow>();

        materialInstance = new Material(meshRenderer.material);
        meshRenderer.material = materialInstance;

        SetupMaterialLR();

        StartCoroutine(ChangeEmissionCoroutine(1));
    }

    void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    void Update()
    {
        // 每帧刷新弦线端点和发光颜色，让视觉跟随模型运动。
        UpdateEmissionColor();

        UpdateStringVisual(frontLine_L, frontStartPoint_L, frontEndPoint_L);
        UpdateStringVisual(frontLine_R, frontStartPoint_R, frontEndPoint_R);
        UpdateStringVisual(backLine_L, backStartPoint_L, backEndPoint_L);
        UpdateStringVisual(backLine_R, backStartPoint_R, backEndPoint_R);

        // Update the attack visual everyframe until it's disabled and enemy can't be found
        // Make the attack line follow the enemy instead of staying in place
        if (attackLineVisual.enabled && hitPoint != Vector3.zero)
            attackLineVisual.SetPosition(1, hitPoint);
    }

    public void CreateOnHitVFX(Vector3 hitPoint)
    {
        objectPool.Get(onHitFX, hitPoint, Random.rotation);
    }

    public void PlayAttackVFX(Vector3 startPoint, Vector3 endPoint)
    {
        StartCoroutine(AttackVFXCoroutine(startPoint, endPoint));
    }

    public void PlayReloadVFX(float duration)
    {
        // 装填时间取攻击冷却的一半，用于充能和转轮复位。
        float newDuration = duration / 2;

        StartCoroutine(ChangeEmissionCoroutine(newDuration));
        StartCoroutine(ChangeRotorPosition(newDuration)); 
    }

    private void SetupMaterialLR()
    {
        // 让所有线条共用同一个材质实例，发光颜色保持一致。
        foreach (var line in lineRenderers)
        {
            line.material = materialInstance;
        }
    }

    private void UpdateStringVisual(LineRenderer lineRenderer, Transform startPoint, Transform endPoint)
    {
        // 弩弦由 LineRenderer 连接两个挂点。
        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, endPoint.position);
    }

    private void UpdateEmissionColor()
    {
        // currentIntensity 越高，塔头和线条越接近 endColor。
        Color emissionColor = Color.Lerp(startColor, endColor, currentIntensity / maxIntensity);

        // Convert the current emission color to gamma space
        // Make sure the color is not too dark or too bright
        emissionColor *= Mathf.LinearToGammaSpace(currentIntensity);

        materialInstance.SetColor("_EmissionColor", emissionColor);
    }

    private IEnumerator AttackVFXCoroutine(Vector3 startPoint, Vector3 endPoint)
    {
        // 短暂显示攻击线，模拟瞬发弩箭/能量束。
        hitPoint = endPoint;

        attackLineVisual.enabled = true;
        attackLineVisual.SetPosition(0, startPoint);
        attackLineVisual.SetPosition(1, endPoint);

        yield return new WaitForSeconds(attackVisualDur);

        attackLineVisual.enabled = false;
    }

    private IEnumerator ChangeEmissionCoroutine(float duration)
    {
        // 攻击后逐渐把发光强度充回最大值。
        // Record the moment when this coroutine start
        float startTime = Time.time;
        float startIntensity = 0f;

        // Repeat every frame until the full duration has passed
        while (Time.time - startTime < duration)
        {
            // Calculate the proportion of the duration has elapsed since this coroutine start
            float tValue = (Time.time - startTime) / duration;
            // Smoothly increase the intensity based on current progress (tValue)
            currentIntensity = Mathf.Lerp(startIntensity, maxIntensity, tValue);
            yield return null; // wait for the next frame
        }

        currentIntensity = maxIntensity;
    }

    private IEnumerator ChangeRotorPosition(float duration)
    {
        // 转轮从卸载位置回到装填位置，表现冷却过程。
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float tValue = (Time.time - startTime) / duration;
            rotor.position = Vector3.Lerp(rotorUnloadPoint.position, rotorLoadPoint.position, tValue);
            yield return null;
        }

        rotor.position = rotorLoadPoint.position;
    }
}
