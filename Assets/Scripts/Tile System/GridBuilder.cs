using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    // 编辑器内的地图格子生成器，同时负责当前格子的 NavMesh 数据刷新。
    [SerializeField] private bool hadFirstLoad;
    [SerializeField] private int gridLength = 10;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private GameObject mainPrefab;
    [SerializeField] private List<GameObject> createdTiles;

    private NavMeshSurface myNavMesh => GetComponent<NavMeshSurface>();

    public bool IsOnFirstLoad()
    {
        // 给其他系统判断“是否第一次加载地图”，第一次调用返回 true，之后返回 false。
        if (hadFirstLoad == false)
        {
            return hadFirstLoad = true;
        }

        return false;
    }

    private void CreateTile(float xPosition, float zPosition)
    {
        // 按网格坐标生成地块，并默认把它转换成可建造格。
        Vector3 newPosition = new(xPosition, 0, zPosition);
        GameObject newTile = Instantiate(mainPrefab, newPosition, Quaternion.identity, transform);
        createdTiles.Add(newTile);

        newTile.GetComponent<TileSlot>().TurnIntoBuildSlot(mainPrefab);
    }

    [ContextMenu("Build Grid")]
    private void BuildGrid()
    {
        // 右键组件菜单可调用：根据 gridLength/gridWidth 重新生成整张地图。
        // Make sure you cannot create more tiles on top of existing tiles
        ClearGrid();

        for (int x = 0; x < gridLength; x++)
        {
            for (int z = 0; z < gridWidth; z++)
            {
                CreateTile(x, z);
            }
        }
    }

    [ContextMenu("Clear Grid")]
    private void ClearGrid()
    {
        foreach (GameObject tile in createdTiles)
        {
            DestroyImmediate(tile);
        }

        createdTiles.Clear();
    }

    public void ClearNavMeshData()
    {
        // 切换/重开关卡前清掉旧寻路数据，避免残留 NavMesh 影响新关卡。
        myNavMesh.RemoveData();
    }

    public List<GameObject> GetTileSetup() => createdTiles;

    public void UpdateNewNavMesh() => myNavMesh.BuildNavMesh();

    public void MakeTileNonInteractable(bool isUninteractable)
    {
        // 地图动画或切换期间，可统一关闭格子交互。
        foreach (var tile in createdTiles)
        {
            tile.GetComponent<TileSlot>().MakeNonInteractable(isUninteractable);
        }
    }
}
