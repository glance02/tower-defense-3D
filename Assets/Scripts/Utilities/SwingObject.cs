using UnityEngine;

public class SwingObject : MonoBehaviour
{
    // 通用摆动脚本：让物体围绕指定轴做往返摇摆。
    [Header("Swing settings")]
    [SerializeField] private float swingDegree = 10;
    [SerializeField] private float swingSpeed = 1;
    [SerializeField] private Vector3 swingAxis;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        Swing();
    }

    // Smoothly rotate object back and forth along the specified axis
    private void Swing()
    {
        // Calculate the current angle of the swing
        // Mathf.Sin(Time.time * swingSpeed) produce a smooth oscillation between -1 and 1
        // Multiplying by swingDegree to scales to oscillation to the desired angle limit
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingDegree;

        // Quaternion.AngleAxis(angle, swingAxis) creates a rotation by angle degrees around the specified swingAxis
        // Combine with startRotation, ensuring the swing occurs relative to the original rotation;
        transform.localRotation = startRotation * Quaternion.AngleAxis(angle, swingAxis);
    }
}
