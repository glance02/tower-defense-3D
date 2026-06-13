using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    // 镜头效果控制：菜单/游戏视角切换、聚焦城堡、屏幕震动和控制权启停。
    [Header("Camera Transition")]
    [SerializeField] private float transitionDuration = 3;
    [Space]

    [SerializeField] private Vector3 inMenuPosition;
    [SerializeField] private Quaternion inMenuRotation;
    [Space]

    [SerializeField] private Vector3 inGamePosition;
    [SerializeField] private Quaternion inGameRotation;
    [Space]

    // [SerializeField] private Vector3 levelSelectPosition;
    // [SerializeField] private Quaternion levelSelectRotation;

    [Header("Camera Shake")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float shakeMagnitude;
    [Range(0.1f, 3f)]
    [SerializeField] private float shakeDuration;

    [Header("Castle Focus")]
    [SerializeField] private float focusOnCastleDuration = 2;
    [SerializeField] private float yOffset = 3;
    [SerializeField] private float distanceToCastle = 7;

    private Coroutine cameraCo;
    private CameraController cameraController;

    void Awake()
    {
        cameraController = GetComponent<CameraController>();
    }

    void Start()
    {
        // 测试场景直接允许控制相机；正式流程先停在菜单视角。
        if (GameManager.instance.IsTestingLevel())
        {
            cameraController.EnableCamControl(true);
            return;
        }

        SwitchToMenuView();
    }

    public void FocusOnCastle()
    {
        // 胜利/失败时把镜头移动到能看见城堡的位置。
        Transform castle = FindFirstObjectByType<PlayerCastle>().transform;

        if (castle == null)
            return;

        Vector3 directionToCastle = (castle.position - transform.position).normalized;
        Vector3 targetPosition = castle.position - (directionToCastle * distanceToCastle);
        targetPosition.y = castle.position.y + yOffset;

        Quaternion targetRotation = Quaternion.LookRotation(castle.position - targetPosition);

        if (cameraCo != null)
            StopCoroutine(cameraCo);

        cameraCo = StartCoroutine(ChangePositionAndRotation(targetPosition, targetRotation, focusOnCastleDuration));
    }

    public void SwitchToMenuView()
    {
        // 切回主菜单预设机位。
        if (cameraCo != null)
            StopCoroutine(cameraCo);

        cameraController.AdjustPicthValue(inMenuRotation.eulerAngles.x);
        cameraCo = StartCoroutine(ChangePositionAndRotation(inMenuPosition, inMenuRotation, transitionDuration));
    }

    public void SwitchToGameView()
    {
        // 进入游戏预设机位。
        if (cameraCo != null)
            StopCoroutine(cameraCo);

        cameraController.AdjustPicthValue(inGameRotation.eulerAngles.x);
        cameraCo = StartCoroutine(ChangePositionAndRotation(inGamePosition, inGameRotation, transitionDuration));
    }

    public void ShakeScreen(float refDuration, float refMagnitude)
    {
        StartCoroutine(ScreenShakeFX(refDuration, refMagnitude));
    }

    private IEnumerator ChangePositionAndRotation(Vector3 targetPosition, Quaternion targetRotation, float duration = 3, float delay = 0)
    {
        // 平滑移动镜头期间禁用玩家控制，避免输入打断过渡。
        yield return new WaitForSeconds(delay);

        cameraController.EnableCamControl(false);

        float time = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (time < duration)
        {
            transform.SetPositionAndRotation(Vector3.Lerp(startPosition, targetPosition, time / duration), Quaternion.Lerp(startRotation, targetRotation, time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private IEnumerator ScreenShakeFX(float duration, float magnitude)
    {
        // 在原始位置周围随机偏移，形成短促震屏反馈。
        Vector3 originalPosition = cameraController.transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float x = Random.Range(-1, 1) * magnitude;
            float y = Random.Range(-1, 1) * magnitude;

            cameraController.transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraController.transform.position = originalPosition;
    }

    private IEnumerator EnableCameraControlCo(float delay)
    {
        yield return new WaitForSeconds(delay);
        cameraController.EnableCamControl(true);
    }

    public Coroutine GetActiveCameraCo() => cameraCo;

    public void EnableCameraEffect() => StartCoroutine(EnableCameraControlCo(transitionDuration + .5f));
}
