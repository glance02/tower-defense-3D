using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 游戏内自由相机控制：右键旋转、滚轮缩放、中键/键盘平移。
    public bool canControl;

    [SerializeField] private float maxDistanceFromCenter;
    [SerializeField] private Vector3 levelCenterPoint;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float maxFocusPointDistance = 15f;
    [SerializeField] private Transform focusPoint;
    [Space]
    [SerializeField] private float minPitch = 5f;
    [SerializeField] private float maxPitch = 85f;
    private float pitch;

    [Header("Zooming")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;

    [Header("Movement")]
    [SerializeField] private float keyboardMoveSpeed = 90f;
    [SerializeField] private float mouseMoveSpeed = 5f;
    [SerializeField] private float edgeMovementSpeed = 10f;
    [SerializeField] private float screenEdgePadding = 10f;
    private float screenHeight;
    private float screenWidth;


    private float smoothTime = .1f;
    private Vector3 lastMousePosition;
    private Vector3 movementVelocity = Vector3.zero;
    private Vector3 zoomVelocity = Vector3.zero;
    private Vector3 mouseMovementVelocity = Vector3.zero;
    private Vector3 edgeMovementVelocity = Vector3.zero;

    void Start()
    {
        screenHeight = Screen.height;
        screenWidth = Screen.width;

        focusPoint.GetComponent<MeshRenderer>().enabled = false;
    }

    void Update()
    {
        // 镜头过渡或菜单状态下会禁用玩家控制。
        if (canControl == false)
            return;

        HandleZoom();
        HandleRotation();
        HandleMouseMovement();
        HandleKeyBoardMovement();
        // HandleEdgeMovement();

        // focusPoint.position = transform.position + transform.forward * GetFocusPointDistance();
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxFocusPointDistance))
            focusPoint.position = transform.position + transform.forward * hit.distance;
    }

    private void HandleRotation()
    {
        // 围绕 focusPoint 旋转，相机始终看向这个焦点。
        // Check if the right mouse button is being hold
        if (Input.GetMouseButton(1))
        {
            float hRotation = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            float vRotation = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

            // Adjusts the vertical angle and Clamp it to keep it in safe ranges
            pitch = Mathf.Clamp(pitch - vRotation, minPitch, maxPitch);

            // Rotate horizontally around the focus point using the world's up axis
            transform.RotateAround(focusPoint.position, Vector3.up, hRotation);

            // Rotate vertically around the point using local right axis
            // Smoothly adjust the angle by the difference between desired pitch and current x rotation
            transform.RotateAround(focusPoint.position, transform.right, pitch - transform.eulerAngles.x);

            // Make the camera always look at the focus point
            transform.LookAt(focusPoint);
        }
    }

    private void HandleZoom()
    {
        // 沿相机 forward 方向推进/拉远，同时用高度限制最大最小缩放。
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Determines the direction and strength of the zoom
        Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;

        // Calculate new camera position after zooming
        Vector3 targetPosition = transform.position + zoomDirection;

        if (transform.position.y < minZoom && scroll > 0)
            return;

        if (transform.position.y > maxZoom && scroll < 0)
            return;

        // Smoothly move the camera from its current position to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref zoomVelocity, smoothTime);
    }

    private void HandleKeyBoardMovement()
    {
        // WASD/方向键移动，相对当前镜头朝向的水平投影。
        Vector3 targetPosition = transform.position;
        float vInput = Input.GetAxisRaw("Vertical");
        float hInput = Input.GetAxisRaw("Horizontal");

        if (vInput == 0 && hInput == 0)
            return;

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        if (vInput > 0)
            targetPosition += flatForward * keyboardMoveSpeed * Time.deltaTime;
        if (vInput < 0)
            targetPosition -= flatForward * keyboardMoveSpeed * Time.deltaTime;

        if (hInput > 0)
            targetPosition += transform.right * keyboardMoveSpeed * Time.deltaTime;
        if (hInput < 0)
            targetPosition -= transform.right * keyboardMoveSpeed * Time.deltaTime;

        // Restrict the camera position to stay within the distance to the level center point
        if (Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
            targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref movementVelocity, smoothTime);
    }

    private void HandleMouseMovement()
    {
        // 中键拖动地图，移动方向与鼠标拖拽方向相反，类似 RTS 相机。
        // Store the current mouse position when press middle mouse button
        if (Input.GetMouseButtonDown(2))
            lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(2))
        {
            // Calculate the change in position between the current mouse input and the last
            Vector3 positionDifference = Input.mousePosition - lastMousePosition;

            // Convert mouse movement into world space movement
            Vector3 moveRight = transform.right * (-positionDifference.x) * mouseMoveSpeed * Time.deltaTime;
            Vector3 moveForward = transform.forward * (-positionDifference.y) * mouseMoveSpeed * Time.deltaTime;

            // Prevent vertical movement
            moveRight.y = 0;
            moveForward.y = 0;

            // Combine right and forward movement into a single movement vector
            Vector3 movement = moveRight + moveForward;
            Vector3 targetPosition = transform.position + movement;

            // Restrict the camera position to stay within the distance to the level center point
            if (Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
                targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref mouseMovementVelocity, smoothTime);

            // Save the current mouse position to compare it in the next frame
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleEdgeMovement()
    {
        // 屏幕边缘移动逻辑目前未启用，保留给之后需要 RTS 边缘滚屏时使用。
        Vector3 mousePosition = Input.mousePosition;
        Vector3 targetPosition = transform.position;
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        // Check if mouse position is pass far right side of the screen
        if (mousePosition.x > screenWidth - screenEdgePadding)
            targetPosition += transform.right * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.x < screenEdgePadding)
            targetPosition -= transform.right * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.y > screenHeight - screenEdgePadding)
            targetPosition += flatForward * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.y < screenEdgePadding)
            targetPosition -= flatForward * edgeMovementSpeed * Time.deltaTime;

        // Restrict the camera position to stay within the distance to the level center point
        if (Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
            targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref edgeMovementVelocity, smoothTime);
    }

    private float GetFocusPointDistance()
    {
        // 射线命中地面时把焦点放在命中点，否则使用最大焦距。
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxFocusPointDistance))
            return hit.distance;

        return maxFocusPointDistance;
    }

    public void EnableCamControl(bool enable) => canControl = enable;

    public float AdjustPicthValue(float changeValue) => pitch = changeValue;

    public float AdjustKeyboardMoveSpeed(float changeValue) => keyboardMoveSpeed = changeValue;

    public float AdjustMouseMoveSpeed(float changeValue) => mouseMoveSpeed = changeValue;

}
