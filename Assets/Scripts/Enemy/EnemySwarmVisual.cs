using UnityEngine;

public class EnemySwarmVisual : EnemyVisual
{
    // 群体小敌人的视觉：随机选择外观变体，并上下弹跳。
    [Header("Visual variants")]
    [SerializeField] private GameObject[] variants;

    [Header("Bounce Settings")]
    [SerializeField] private float bounceSpeed;
    [SerializeField] private float minHeight = .1f;
    [SerializeField] private float maxHeight = .3f;
    [SerializeField] private AnimationCurve bounceCurve;
    private float bounceTimer;


    protected override void Awake()
    {
        ChooseVisualVariant();
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        BounceEffect();
    }

    private void BounceEffect()
    {
        // 使用 AnimationCurve 控制弹跳节奏，比纯 Sin 更容易调手感。
        bounceTimer += bounceSpeed * Time.deltaTime;

        float bounceValue = bounceCurve.Evaluate(bounceTimer % 1);
        float bounceHeight = Mathf.Lerp(minHeight, maxHeight, bounceValue);

        visuals.localPosition = new Vector3(visuals.localPosition.x, bounceHeight, visuals.localPosition.z);
    }

    private void ChooseVisualVariant()
    {
        // 每次生成时只启用一个随机外观。
        foreach (var option in variants)
        {
            option.SetActive(false);
        }

        int randomIndex = Random.Range(0, variants.Length);
        GameObject newVisuals = variants[randomIndex];

        newVisuals.SetActive(true);
        visuals = newVisuals.transform;
    }
}
