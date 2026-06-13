using UnityEngine;

public class EnemySpiderVisual : EnemyVisual
{
    // 蜘蛛 Boss 视觉：身体上下浮动、烟雾特效，以及所有腿的落点更新。
    [Header("Leg Details")]
    public float legSpeed = 3;
    public float increaseLegSpeed = 10;
    private SpiderLeg[] legs;

    [Header("Body Animation")]
    [SerializeField] private float bodyAnimSpeed = 1;
    [SerializeField] private float maxHeight = .1f;
    [SerializeField] private Transform bodyTransform;

    [Header("Smoke VFX")]
    [SerializeField] private float smokeCooldown;
    [SerializeField] private ParticleSystem[] smokeVFXs;
    private float smokeTimer;

    private Vector3 startPosition;
    private float elaspedTime;

    protected override void Awake()
    {
        base.Awake();
        legs = GetComponentsInChildren<SpiderLeg>();
    }

    protected override void Start()
    {
        base.Start();

        startPosition = bodyTransform.localPosition;
    }

    protected override void Update()
    {
        base.Update();

        AnimateBody();
        ActivateSmokeVFX();
        UpdateSpiderLegs();
    }

    public void BrieflySpeedUpLegs()
    {
        // EnemySpider 切换路点时调用，让腿部动画短暂加速。
        foreach (var leg in legs)
        {
            leg.SpeedUpLeg();
        }
    }

    private void ActivateSmokeVFX()
    {
        // 周期性播放移动烟雾。
        smokeTimer -= Time.deltaTime;

        if (smokeTimer < 0)
        {
            smokeTimer = smokeCooldown;
            foreach (var smoke in smokeVFXs)
            {
                smoke.Play();
            }
        }
    }

    private void AnimateBody()
    {
        // 用正弦值让身体在本地 Y 轴轻微上下浮动。
        elaspedTime += Time.deltaTime * bodyAnimSpeed;

        // Plus 1 to prevent the body goes below the ground (from 0 to 2)
        // Divide by 2 so it goes from 0 and 1
        float sinValue = (Mathf.Sin(elaspedTime) + 1) / 2;
        float newY = Mathf.Lerp(0, maxHeight, sinValue);

        bodyTransform.localPosition = startPosition + new Vector3(0, newY, 0);
    }

    private void UpdateSpiderLegs()
    {
        // 每条腿自己判断是否需要迈步。
        foreach (var leg in legs)
        {
            leg.UpdateLeg();
        }
    }
}
