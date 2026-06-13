using System.Collections;
using UnityEngine;

public class SpiderLeg : MonoBehaviour
{
    // 蜘蛛腿移动控制：每条腿追随地面参考点，并和对侧腿交替移动。
    [Header("Movement Details")]
    [SerializeField] private float legMoveSpeed = 2.5f;
    [SerializeField] private float moveThreshold = .45f;
    private bool canMove = true;
    private bool shouldMove;
    private Coroutine moveCo;
    [Space]

    [Header("Reference Details")]
    [SerializeField] private Vector3 placementOffset;
    [SerializeField] private SpiderLeg oppositeLeg;
    [SerializeField] private SpiderLegRef legRef;
    [SerializeField] private Transform actualTarget;
    [SerializeField] private Transform bottomLeg;
    [SerializeField] private Transform worldTargetRef;
    private EnemySpiderVisual spiderVisual;

    private ObjectPoolManager objectPool;

    void Awake()
    {
        // worldTargetRef 脱离蜘蛛父物体后留在世界空间，作为脚落点。
        objectPool = ObjectPoolManager.instance;
        spiderVisual = GetComponentInParent<EnemySpiderVisual>();
        worldTargetRef = Instantiate(worldTargetRef, actualTarget.position, Quaternion.identity).transform;

        worldTargetRef.gameObject.name = legRef.gameObject.name + "_world";
        legMoveSpeed = spiderVisual.legSpeed;
    }

    void OnEnable()
    {
        ParentLegReference(false);
    }

    void OnDisable()
    {
        ParentLegReference(true);
    }

    public void UpdateLeg()
    {
        // actualTarget 是骨骼/IK 目标，始终跟随 worldTargetRef 加偏移。
        actualTarget.position = worldTargetRef.position + placementOffset;

        // Only move when the distance between leg contact point and target position is exceeded the desired threshold
        shouldMove = Vector3.Distance(worldTargetRef.position, legRef.ContactPoint()) > moveThreshold;

        if (bottomLeg != null)
            bottomLeg.forward = Vector3.down;

        if (shouldMove && canMove)
        {
            if (moveCo != null)
                StopCoroutine(moveCo);

            moveCo = StartCoroutine(LegMoveCo());
        }
    }

    public void SpeedUpLeg()
    {
        StartCoroutine(SpeedUpLegCo());
    }

    private IEnumerator LegMoveCo()
    {
        // 移动当前腿时暂时锁住对侧腿，避免两条腿同时抬起。
        oppositeLeg.CanMove(false);

        while (Vector3.Distance(worldTargetRef.position, legRef.ContactPoint()) > 0.1f)
        {
            worldTargetRef.position = Vector3.MoveTowards(worldTargetRef.position, legRef.ContactPoint(), legMoveSpeed * Time.deltaTime);
            yield return null;
        }

        oppositeLeg.CanMove(true);
    }

    private IEnumerator SpeedUpLegCo()
    {
        // 蜘蛛切路点时短暂加快腿速，让移动看起来更有冲刺感。
        legMoveSpeed = spiderVisual.increaseLegSpeed;

        yield return new WaitForSeconds(1f);

        legMoveSpeed = spiderVisual.legSpeed;
    }

    private void ParentLegReference(bool isParent)
    {
        // 对象池回收蜘蛛时，把世界参考点挂回池管理器，避免场景根节点残留。
        if (worldTargetRef == null)
            return;

        worldTargetRef.transform.parent = isParent ? objectPool.transform : null;
    }

    private void CanMove(bool enableMovement) => canMove = enableMovement;
}
