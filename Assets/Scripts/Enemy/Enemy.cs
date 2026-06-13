using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType {None, Basic, Fast, Swarm, Heavy, Stealth, Flying, SpiderBoss}

public class Enemy : MonoBehaviour, IDamagable
{
    // 敌人基类：处理生命、移动、隐身、死亡、金币奖励和对象池回收。
    public float currentHP = 0;
    public float maxHP = 100;

    public EnemyVisual visual { get; private set; }

    [SerializeField] private int enemyWorth;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private EnemyType enemyType;
    [SerializeField] protected Vector3[] enemyWaypoints;

    private int originalLayerIndex;
    private float totalDistance;
    private GameManager gameManager;
    private Coroutine hideCo;
    private Coroutine disableHideCo;

    protected bool canBeHidden = true;
    protected bool isHidden = true;
    protected bool isDead;
    protected int nextWaypointIndex;
    protected int currentWavepointIndex;
    protected float originalSpeed;
    protected EnemyPortal enemyPortal;
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected ObjectPoolManager objectPool;

    protected virtual void Awake()
    {
        // NavMeshAgent 负责寻路，Rigidbody/Visual/Pool 等组件在这里统一缓存。
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);

        visual = GetComponent<EnemyVisual>();
        originalLayerIndex = gameObject.layer;

        gameManager = FindFirstObjectByType<GameManager>();
        originalSpeed = agent.speed;

        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void Start()
    {
        // This is just for override
    }

    protected virtual void Update()
    {
        // 每帧朝向当前路径目标，并在接近路点时切到下一个目标。
        FaceTarget(agent.steeringTarget);
        SetNextDestination();
    }

    public void SlowEnemy(float slowMultiplier, float duration)
    {
        // 塔的减速效果入口；实际速度恢复由协程处理。
        StartCoroutine(SlowEnemyCo(slowMultiplier, duration));
    }

    private IEnumerator SlowEnemyCo(float slowMultiplier, float duration)
    {
        agent.speed = originalSpeed;
        agent.speed *= slowMultiplier;

        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;
    }

    public void DisableHide(float duration)
    {
        // 某些塔/效果可以短时间禁止敌人进入隐身状态。
        if (disableHideCo != null)
            StopCoroutine(disableHideCo);
        
        disableHideCo = StartCoroutine(DisableHideCo(duration));
    }

    protected virtual IEnumerator DisableHideCo(float duration)
    {
        canBeHidden = false;
        yield return new WaitForSeconds(duration);
        canBeHidden = true;
    } 

    public void HideEnemy(float duration)
    {
        // 隐身时切到 Untargetable 层，让塔的检测 LayerMask 无法选中它。
        if (canBeHidden == false)
            return;

        if (hideCo != null)
            StopCoroutine(hideCo);

        hideCo = StartCoroutine(HideEnemyCo(duration));
    }

    private IEnumerator HideEnemyCo(float duration)
    {
        gameObject.layer = LayerMask.NameToLayer("Untargetable");
        visual.MakeTransparent(true);
        isHidden = true;

        yield return new WaitForSeconds(duration);

        gameObject.layer = originalLayerIndex;
        visual.MakeTransparent(false);
        isHidden = false;
    }

    public void SetupEnemyWaypoint(EnemyPortal referencePortal)
    {
        // 敌人从传送门生成后，复制该传送门当前的路径点并开始移动。
        enemyPortal = referencePortal;

        UpdateWaypoint(enemyPortal.currentWaypoints);
        CalculateTotalDistance();
        ResetEnemy();
        BeginMovement();
    }

    private void BeginMovement()
    {
        currentWavepointIndex = 0;
        nextWaypointIndex = 0;
        ChangeWayPoint();
    }

    protected void ResetEnemy()
    {
        // 对象池复用敌人时必须重置血量、速度、层级和显示状态。
        gameObject.layer = originalLayerIndex;
        visual.MakeTransparent(false);
        currentHP = maxHP;
        isDead = false;
        agent.speed = originalSpeed;
        agent.enabled = true;
    }

    private void UpdateWaypoint(Vector3[] newWaypoints)
    {
        enemyWaypoints = new Vector3[newWaypoints.Length];

        for (int i = 0; i < enemyWaypoints.Length; i++)
        {
            enemyWaypoints[i] = newWaypoints[i];
        }
    }

    // Calculate the distance between each waypoint and the add each of them to the total distance
    private void CalculateTotalDistance()
    {
        // 预先计算完整路径距离，方便塔优先攻击“离终点最近”的敌人。
        for (int i = 0; i < enemyWaypoints.Length - 1; i++)
        {
            float distance = Vector3.Distance(enemyWaypoints[i], enemyWaypoints[i + 1]);
            totalDistance += distance;
        }
    }

    protected virtual bool ShouldChangeWaypoint()
    {
        // 普通敌人接近当前目标点时，提前切换到下一个路点，移动会更顺滑。
        if (nextWaypointIndex >= enemyWaypoints.Length)
            return false;

        if (agent?.remainingDistance <= 0.5f)
            return true;
        
        Vector3 currentWaypoint = enemyWaypoints[currentWavepointIndex];
        Vector3 nextWaypoint = enemyWaypoints[nextWaypointIndex];

        float distanceToNextWaypoint = Vector3.Distance(transform.position, nextWaypoint);
        float distanceBetweenWaypoints = Vector3.Distance(currentWaypoint, nextWaypoint);

        return distanceToNextWaypoint < distanceBetweenWaypoints;
    }

    protected virtual void ChangeWayPoint()
    {
        // 只有 Agent 位于 NavMesh 上时才能设置目的地。
        if (agent.isOnNavMesh == false)
            return;
            
        agent.SetDestination(GetNextWayPoint());
    }

    protected Vector3 GetFinalWayPoint()
    {
        if (enemyWaypoints.Length == 0)
            return transform.position;

        return enemyWaypoints[enemyWaypoints.Length - 1];
    }

    // Returns the position of the next waypoint in the sequence.
    // If all waypoints have been reached, it returns the current position instead.
    private Vector3 GetNextWayPoint()
    {
        // 取出下一个路点，同时维护 totalDistance，供塔判断敌人威胁程度。
        if (nextWaypointIndex >= enemyWaypoints.Length)
            return transform.position;

        Vector3 targetPosition = enemyWaypoints[nextWaypointIndex];

        // Subtract the distance between the current waypoint and the previous one from the total distance
        // Start from index 1 because index 0 has no previous waypoint (avoid out of bound error)
        if (nextWaypointIndex > 0)
        {
            float distance = Vector3.Distance(enemyWaypoints[nextWaypointIndex], enemyWaypoints[nextWaypointIndex - 1]);
            totalDistance -= distance;
        }

        nextWaypointIndex++;

        currentWavepointIndex = nextWaypointIndex - 1;

        return targetPosition;
    }

    // Set a destination for the next waypoint when the enemy remaining distance is close to the current waypoint
    private void SetNextDestination()
    {
        if (ShouldChangeWaypoint())
            ChangeWayPoint();
    }

    // Smoothly rotate the enemy game object to face the given target position
    private void FaceTarget(Vector3 newTarget)
    {
        // Calculate the direction from current position to next target
        Vector3 directionToTarget = newTarget - transform.position;

        // Ignore any difference in vertical position
        directionToTarget.y = 0;

        // Create a new rotation that points the forward vector (y) of the game object up to the calculated direction
        // Since we ignore the vertical position above, the game object can only rotate horizontally
        Quaternion newRotation = Quaternion.LookRotation(directionToTarget);

        // Create a smooth rotation from the current rotation to the target rotation at a defined speed
        // Time.deltaTime makes this framerate independent
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, turnSpeed * Time.deltaTime);
    }

    public virtual void TakeDamage(float damage)
    {
        // isDead 防止同一帧内多个攻击重复触发死亡逻辑。
        currentHP -= damage;

        if (rb != null && currentHP <= 0 && isDead == false)
        {
            // Use flag isDead in case Die() is called twice
            isDead = true;
            GetKilled();
        }
    }

    public virtual void GetKilled()
    {
        // 正常被击杀时会奖励金币；进城堡时调用 RemoveEnemy，不会发奖励。
        RemoveEnemy();
        gameManager.IncreaseCurrencyFromKill(enemyWorth);
    }

    public virtual void RemoveEnemy()
    {
        // 回收敌人并通知传送门更新活动敌人列表。
        if (visual != null)
            visual.CreateDeathVFX();

        objectPool.Remove(gameObject);

        // Prevent enemy from being too far from nav mesh when disabled
        agent.enabled = false;

        if (enemyPortal != null)
            enemyPortal.RemoveActiveEnemy(gameObject);
    }

    protected virtual void OnEnable()
    {
        // Used for override
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        CancelInvoke();
    }

    public virtual float CalculateDistanceToGoal() => totalDistance + agent.remainingDistance;

    public Vector3 GetCenterPoint() => centerPoint.position;

    public EnemyType GetEnemyType() => enemyType;

}
