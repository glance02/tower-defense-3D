using UnityEngine;

public class RotateObject : MonoBehaviour
{
    // 通用旋转脚本：用于阀门、装饰件等持续自转物体。
    [SerializeField] private Vector3 rotationVector;
    [SerializeField] private float rotateSpeed;

    void Update()
    {
        float newRotationSpeed = rotateSpeed * 100;
        transform.Rotate(rotationVector * newRotationSpeed * Time.deltaTime);
    }

    public void AdjustRotationSpeed(float newSpeed)
    {
        // 允许其他脚本临时改变旋转速度，例如锤塔攻击时加速阀门。
        rotateSpeed = newSpeed;
    }
}
