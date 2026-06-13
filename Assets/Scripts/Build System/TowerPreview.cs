using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerPreview : MonoBehaviour
{
    // 建塔前的预览对象：复用塔 prefab 的模型，但移除战斗逻辑，只保留透明模型和攻击范围显示。
    private bool towerAttacksForward;
    private float attackRange;

    private TowerAttackRangeDisplay attackDisplay;
    private ForwardAttackDisplay forwardAttack;
    private MeshRenderer[] meshRenderers;

    private List<System.Type> compToKeep = new();

    public void SetupTowerPreview(GameObject towerToBuild)
    {
        // 根据真实塔的数据创建预览：攻击范围、是否前方攻击、透明材质都从 prefab/BuildManager 读取。
        Tower tower = towerToBuild.GetComponent<Tower>();

        // Attach the TowerAttackRangeDisplay component to this object
        // then assign it to attackDisplay variable
        attackDisplay = transform.AddComponent<TowerAttackRangeDisplay>();
        forwardAttack = tower.GetComponent<ForwardAttackDisplay>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        attackRange = tower.GetAttackRange();
        towerAttacksForward = tower.isAttackForward;

        SecureComponents();
        MakeAllMeshTransparent();
        DestroyExtraComponents();

        gameObject.SetActive(false);
    }

    public void ShowPreview(bool isPreviewShow, Vector3 previewPosition)
    {
        // 圆形范围塔显示圆圈；前方攻击塔显示两条射程线。
        transform.position = previewPosition;

        if (towerAttacksForward == false)
            attackDisplay.CreateCircle(isPreviewShow, attackRange);
        else
            forwardAttack.CreateLines(isPreviewShow, attackRange);
    }

    private void DestroyExtraComponents()
    {
        // 预览体不应该真的攻击、播放逻辑或参与碰撞，所以删除不需要的组件。
        Component[] components = GetComponents<Component>();

        foreach (var componentToCheck in components)
        {
            if (ComponentSecured(componentToCheck) == false)
                Destroy(componentToCheck);
        }
    }

    private void SecureComponents()
    {
        // 这些组件是预览必须保留的白名单。
        compToKeep.Add(typeof(TowerPreview));
        compToKeep.Add(typeof(TowerAttackRangeDisplay));
        compToKeep.Add(typeof(Transform));
        compToKeep.Add(typeof(LineRenderer));
        compToKeep.Add(typeof(ForwardAttackDisplay));
    }

    private bool ComponentSecured(Component compToCheck)
    {
        return compToKeep.Contains(compToCheck.GetType());
    }

    private void MakeAllMeshTransparent()
    {
        // 所有子 Mesh 使用统一预览材质，避免真实塔材质太实影响可读性。
        Material previewMat = FindFirstObjectByType<BuildManager>().GetBuildPreviewMaterial();

        foreach (var mesh in meshRenderers)
        {
            mesh.material = previewMat;
        }
    }
}
