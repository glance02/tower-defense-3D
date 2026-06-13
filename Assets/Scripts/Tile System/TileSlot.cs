using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;

public class TileSlot : MonoBehaviour
{
    // 单个地图格：可在编辑器里切换 mesh/material/collider/子物体，并决定是否可建塔。
    private int originalLayerIndex;

    private MeshRenderer tileMeshRenderer => GetComponent<MeshRenderer>();
    private MeshFilter tileMeshFilter => GetComponent<MeshFilter>();
    private Collider tileCollider => GetComponent<Collider>();
    private NavMeshSurface tileNavMesh => GetComponentInParent<NavMeshSurface>(true);
    private TileSetHolder tileSetHolder => GetComponentInParent<TileSetHolder>(true);

    void Awake()
    {
        originalLayerIndex = gameObject.layer;    
    }

    public void SwitchTile(GameObject referenceTile)
    {
        // 用 referenceTile 的外观、碰撞、子物体和层级覆盖当前格。
        gameObject.name = referenceTile.name;

        TileSlot newTile = referenceTile.GetComponent<TileSlot>();

        tileMeshRenderer.material = newTile.GetMaterial();
        tileMeshFilter.mesh = newTile.GetMesh();

        UpdateCollider(newTile.GetCollider());
        UpdateChildrenObject(newTile);
        UpdateLayer(referenceTile);
        UpdateNavMesh();

        TurnIntoBuildSlot(referenceTile);
        // DisableShadowIfNeeded();
    }

    public List<GameObject> GetAllChildren()
    {
        // Unity Transform 迭代只能拿 Transform，这里转成 GameObject 列表方便复制/删除。
        List<GameObject> children = new();

        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        return children;
    }

    // Change the current collider to a new one when switching different tile
    public void UpdateCollider(Collider referenceCollider)
    {
        // 切换地块时同步碰撞体形状，保证点击、寻路和物理都符合新模型。
        DestroyImmediate(tileCollider);

        if (referenceCollider is BoxCollider)
        {
            BoxCollider reference = referenceCollider.GetComponent<BoxCollider>();
            BoxCollider newCollider = transform.AddComponent<BoxCollider>();

            newCollider.center = reference.center;
            newCollider.size = reference.size;
        }

        if (referenceCollider is MeshCollider)
        {
            MeshCollider reference = referenceCollider.GetComponent<MeshCollider>();
            MeshCollider newCollider = transform.AddComponent<MeshCollider>();

            newCollider.sharedMesh = reference.sharedMesh;
            newCollider.convex = reference.convex;
        }
    }

    private void UpdateChildrenObject(TileSlot newTile)
    {
        // 先清掉旧装饰物，再复制新地块 prefab 上的装饰物。
        foreach (GameObject child in GetAllChildren())
        {
            // Use this because Detroy doesn't work in Editor
            DestroyImmediate(child);
        }

        // Find every child game object in the new tile
        // and create them in the selected tile
        foreach (GameObject obj in newTile.GetAllChildren())
        {
            Instantiate(obj, transform);
        }
    }

    public void TurnIntoBuildSlot(GameObject refTile)
    {
        // 只有 field 地块允许建造；道路、桥、山体等都移除 BuildSlot。
        BuildSlot buildSlot = GetComponent<BuildSlot>();

        if (refTile != tileSetHolder.tileField)
        {
            if (buildSlot != null)
                DestroyImmediate(buildSlot);
        }
        else
        {
            if (buildSlot == null)
                gameObject.AddComponent<BuildSlot>();
        }
    }

    public void AdjustYRotation(int dir)
    {
        // 编辑器按钮调用：每次按 90 度旋转地块。
        transform.Rotate(0, 90 * dir, 0);
        UpdateNavMesh();
    }

    public void AdjustYPosition(int verticalDir)
    {
        // 编辑器按钮调用：微调地块高度，适合桥/坡道拼接。
        transform.position += new Vector3(0, 0.1f * verticalDir, 0);
        UpdateNavMesh();
    }

    public Material GetMaterial() => tileMeshRenderer.sharedMaterial;

    public Mesh GetMesh() => tileMeshFilter.sharedMesh;

    public Collider GetCollider() => tileCollider;

    public void MakeNonInteractable(bool isUninteractable)
    {
        // 地图动画期间切到不可交互层，避免玩家点击正在移动的格子。
        gameObject.layer = isUninteractable ? 15 : originalLayerIndex;
    }

    private void UpdateLayer(GameObject referenceObj)
    {
        gameObject.layer = referenceObj.layer;
        originalLayerIndex = gameObject.layer;
    }

    private void UpdateNavMesh() => tileNavMesh.BuildNavMesh();
}
