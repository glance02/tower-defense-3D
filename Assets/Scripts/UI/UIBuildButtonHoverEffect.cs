using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBuildButtonHoverEffect : MonoBehaviour
{
    // 建塔按钮的展示动画：选中时向上浮起，取消时回到默认高度。
    [SerializeField] private float adjustSpeed = 10f;
    [SerializeField] private float showcaseY;
    [SerializeField] private float defaultY;

    private float targetY;
    private bool canMove;

    void Update()
    {
        // 使用 Lerp 让按钮逐渐靠近目标 Y 位置。
        if (MathF.Abs(transform.position.y - targetY) > .01f && canMove)
        {
            float newPositionY = Mathf.Lerp(transform.position.y, targetY, adjustSpeed * Time.deltaTime);
            transform.position = new(transform.position.x, newPositionY, transform.position.z);
        }
    }

    public void ToggleMovement(bool isButtonActive)
    {
        // 建造菜单关闭时禁止继续移动，并强制回到默认位置。
        canMove = isButtonActive;
        SetTargetY(defaultY);

        if (isButtonActive == false)
            SetPositionToDefault();
    }

    private void SetPositionToDefault()
    {
        transform.position = new(transform.position.x, defaultY, transform.position.z);
    }

    public void ShowButton(bool isShow)
    {
        if (isShow)
            SetTargetY(showcaseY);
        else
            SetTargetY(defaultY);
    }

    private void SetTargetY(float newY) => targetY = newY;
}
