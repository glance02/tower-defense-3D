using UnityEngine;
using UnityEngine.EventSystems;

public class BuildSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    // 单个可建造格子：响应鼠标悬停/点击，并通知 BuildManager 当前选择。
    private UI ui;
    private TileAnimator tileAnimator;
    private BuildManager buildManager;
    private Vector3 defaultPosition;
    private Coroutine currentMovementUpCo;
    private Coroutine moveToDefaultCo;

    private bool canMoveTile = true;
    private bool isBuildSlotAvailable = true;

    void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        buildManager = FindFirstObjectByType<BuildManager>();
        defaultPosition = transform.position;
    }

    void Start()
    {
        if (isBuildSlotAvailable == false)
            transform.position += new Vector3(0, .1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 地图动画播放中或格子已不可用时，不允许选择建造。
        if (isBuildSlotAvailable == false || tileAnimator.IsGridMoving())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // Prevent running the same functions 
        // from selecting the same tile over and over
        if (buildManager.GetSelectedBuildSlot() == this)
            return;

        SnapToBeforeBuildPostion();
        
        // 先显示建造菜单，再记录选中格子；BuildManager 内部依赖这个顺序。
        // EnableBuildMenu need to be above SelectBuildSlot 
        // so the build menu can be enable correctly    
        buildManager.EnableBuildMenu();
        buildManager.SelectBuildSlot(this);

        MoveTileUp();

        canMoveTile = false;

        ui.uiBuildButton.GetLastSelectedButton()?.TogglePreviewVisual(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标移入时让格子升起，给玩家一个可交互反馈。
        if (isBuildSlotAvailable == false || tileAnimator.IsGridMoving())
            return;

        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2))
            return;

        if (canMoveTile == false)
            return;

        MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标离开且格子没有被选中时，恢复默认高度。
        if (isBuildSlotAvailable == false || tileAnimator.IsGridMoving())
            return;
            
        if (canMoveTile == false)
            return;

        if (currentMovementUpCo != null)
            Invoke(nameof(MoveTileDefault), tileAnimator.GetYTravelDuration());
        else
            MoveTileDefault();

        MoveTileDefault();
    }

    public void SnapToBeforeBuildPostion()
    {
        // 被选中时直接吸附到“建造预览高度”，避免动画过程造成塔预览错位。
        Vector3 targetPosition = defaultPosition + new Vector3(0, tileAnimator.GetBuildOffset(), 0);
        transform.position = targetPosition;
    }

    public void UnSelectTile()
    {
        // 取消选择后恢复悬停能力。
        MoveTileDefault();
        canMoveTile = true;
    }

    private void MoveTileUp()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, tileAnimator.GetBuildOffset(), 0);
        currentMovementUpCo = StartCoroutine(tileAnimator.MoveTileCo(transform, targetPosition));
    }

    private void MoveTileDefault()
    {
        moveToDefaultCo = StartCoroutine(tileAnimator.MoveTileCo(transform, defaultPosition));
    }

    public void SnapToDefaultPosition()
    {
        if (moveToDefaultCo != null)
            StopCoroutine(moveToDefaultCo);

        transform.position = defaultPosition;
    }

    public void SetSlotAvailable(bool enable) => isBuildSlotAvailable = enable;

    public Vector3 GetBuildPosition(float yPosOffset) => defaultPosition + new Vector3(0, yPosOffset);
}
