using System.Collections.Generic;
using UnityEngine;

public class HarpoonVisual : MonoBehaviour
{
    // 鱼叉塔视觉：用多段 link 组成链条，并在命中敌人后挂电击特效。
    [SerializeField] private int maxLinks = 100;
    [SerializeField] private float linkDistance = .2f;
    [SerializeField] private GameObject linkPrefab;
    [SerializeField] private Transform linkParent;
    [Space]

    [SerializeField] private Transform startPoint; // gun point
    [SerializeField] private Transform endPoint; // harpoon back point
    [Space]

    [SerializeField] private GameObject onElectrifyVFX;
    private GameObject currentVFX;

    private ObjectPoolManager objectPool;
    private List<ProjectileHarpoonLink> links = new();

    void Start()
    {
        // 链条节点只创建一次，之后根据距离显示/隐藏。
        InitializeLinks();
        objectPool = ObjectPoolManager.instance;
    }

    void Update()
    {
        if (endPoint == null)
            return;

        ActivateLinksIfNeeded();
    }

    private void InitializeLinks()
    {
        // 预生成固定数量链条，避免攻击时频繁 Instantiate。
        for (int i = 0; i < maxLinks; i++)
        {
            ProjectileHarpoonLink newLink =
                Instantiate(linkPrefab, startPoint.position, Quaternion.identity, linkParent).GetComponent<ProjectileHarpoonLink>();

            links.Add(newLink);
        }
    }

    private void ActivateLinksIfNeeded()
    {
        // 根据起点到终点的距离决定需要显示多少节链条。
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float distance = Vector3.Distance(startPoint.position, endPoint.position);

        int activeLinkAmount = Mathf.Min(maxLinks, Mathf.CeilToInt(distance / linkDistance));

        for (int i = 0; i < links.Count; i++)
        {
            if (i < activeLinkAmount)
            {
                Vector3 newPosition = startPoint.position + direction * linkDistance * (i + 2);
                links[i].EnableLinK(true, newPosition);
            }
            else
                links[i].EnableLinK(false, Vector3.zero);


            if (i != links.Count - 1)
                links[i].UpdateLineRenderer(links[i], links[i + 1]);
        }
    }

    public void EnableChainVisual(bool enable, Transform newEndPoint = null)
    {
        // 鱼叉命中前终点是投射物连接点；关闭时终点回到枪口。
        if (enable)
            endPoint = newEndPoint;

        if (enable == false)
        {
            endPoint = startPoint;
            DestroyElectrifyVFX();
        }
    }

    public void CreateElectrifyVFX(Transform targetTransform)
    {
        // 电击特效挂到敌人身上，跟随敌人移动。
        currentVFX = objectPool.Get(onElectrifyVFX, targetTransform.position, Quaternion.identity, targetTransform);
    }

    private void DestroyElectrifyVFX()
    {
        // 鱼叉攻击结束时回收电击特效。
        if (currentVFX != null)
            objectPool.Remove(currentVFX);
    }
}
