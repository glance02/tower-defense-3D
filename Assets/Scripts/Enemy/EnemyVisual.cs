using System.Collections.Generic;
using UnityEngine;

public class EnemyVisual : MonoBehaviour
{
    // 敌人通用视觉：坡面贴合、死亡特效、隐身透明材质切换。
    [Header("Movement Details")]
    [SerializeField] private float verticalRotationSpeed;
    [SerializeField] private LayerMask roadLayer;
    [Space]

    [Header("Visual Details")]
    [SerializeField] private float deathVFXScale = .5f;
    [SerializeField] protected Transform visuals;
    [SerializeField] private GameObject deathVFX;
    [Space]

    [Header("Transparency Details")]
    [SerializeField] private Material transparentMat;
    private List<Material> originalMats;
    private MeshRenderer[] myMeshes;

    private ObjectPoolManager objectPool;

    protected virtual void Awake()
    {
        CollectDefaultMat();
    }

    protected virtual void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void Update()
    {
        AlignWithSlope();
    }

    public void CreateDeathVFX()
    {
        // 敌人被移除时由 Enemy 调用，特效本身也走对象池。
        GameObject createdDeathVFX = objectPool.Get(deathVFX, transform.position + new Vector3(0, .15f, 0), Quaternion.identity);
        createdDeathVFX.transform.localScale = new Vector3(deathVFXScale, deathVFXScale, deathVFXScale);
    }

    public void MakeTransparent(bool isTransparent)
    {
        // 隐身时替换成透明材质，解除隐身时恢复 Awake 中记录的原材质。
        for (int i = 0; i < myMeshes.Length; i++)
        {
            myMeshes[i].material = isTransparent ? transparentMat : originalMats[i];
        }
    }

    protected void CollectDefaultMat()
    {
        // 记录每个 MeshRenderer 的原材质，便于从透明状态恢复。
        myMeshes = GetComponentsInChildren<MeshRenderer>();
        originalMats = new();

        foreach (var renderer in myMeshes)
        {
            originalMats.Add(renderer.material);
        }
    }

    private void AlignWithSlope()
    {
        // 让敌人视觉模型贴合路面坡度，但不直接改变 NavMeshAgent 所在物体的寻路方向。
        if (visuals == null)
            return;

        // Check if a ray cast from visual position hit road layer and store the infos in RaycastHit hit
        if (Physics.Raycast(visuals.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, roadLayer))
        {
            // Calculate the rotation difference needed the visual up vector match the slope's normal
            // Multiply its by the current rotation to get the desired orientation
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            // Smoothly rotate the visuals from their current rotation toward targetRotation
            visuals.rotation = Quaternion.Slerp(visuals.rotation, targetRotation, verticalRotationSpeed * Time.deltaTime);
        }
    }
}
