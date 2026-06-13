using UnityEngine;

public class ForwardAttackDisplay : MonoBehaviour
{
    // 给只攻击正前方的塔显示两条射程线。
    [SerializeField] private float attackRange;
    [SerializeField] private LineRenderer leftLine;
    [SerializeField] private LineRenderer rightLine;


    void Awake()
    {
        leftLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;    
        rightLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;    
    }

    public void CreateLines(bool showLines, float newRange)
    {
        // 建造预览显示/隐藏时调用，并同步最新射程。
        leftLine.enabled = showLines;
        rightLine.enabled = showLines;

        if (showLines == false)
            return;

        attackRange = newRange;
        UpdateLines();
    }

    public void UpdateLines()
    {
        DrawLine(leftLine);
        DrawLine(rightLine);
    }

    private void DrawLine(LineRenderer line)
    {
        // 每条线从自身位置沿塔的 forward 方向延伸。
        Vector3 start = line.transform.position;

        // Extend the forward direction by desired distance 
        // and moves start forward by that much
        Vector3 end = start + transform.forward * attackRange;

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}
