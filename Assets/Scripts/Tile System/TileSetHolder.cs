using UnityEngine;

public class TileSetHolder : MonoBehaviour
{
    // 地块素材索引表：给 TileSlotEditor 按钮提供可切换的地块 prefab。
    [Header("Level Tile")]
    public GameObject tileLevel;

    [Header("Common Tiles")]
    public GameObject tileField;
    public GameObject tileRoad;
    public GameObject tileSideway;

    [Header("Corners")]
    public GameObject tileCornerInnerBig;
    public GameObject tileCornerInnerSmall;
    public GameObject tileCornerOuterSmall;
    public GameObject tileCornerOuterBig;

    [Header("Hills")]
    public GameObject tileHill_1;
    public GameObject tileHill_2;
    public GameObject tileHill_3;

    [Header("Bridges")]
    public GameObject tileBridgeField;
    public GameObject tileBridgeRoad;
    public GameObject tileBridgeSideway; 
}
