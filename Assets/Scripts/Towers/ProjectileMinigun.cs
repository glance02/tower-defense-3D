using UnityEngine;

public class ProjectileMinigun : MonoBehaviour
{
    // 机枪子弹视觉：实际命中由射线提前确定，这里负责飞到命中点后结算伤害和特效。
    [SerializeField] private GameObject onHitVFX;

    private float damage;
    private float speed;
    private float threshold = .01f;
    private bool isActive = true;
    private IDamagable damagable;
    private Vector3 target;
    private ObjectPoolManager objectPool;
    private TrailRenderer trail;

    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        // isActive 防止到达目标后的伤害和回收逻辑重复执行。
        if (isActive == false)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Use square magnitude to avoid calculation of the square root and only need to be close enough
        if ((transform.position - target).sqrMagnitude <= threshold * threshold)
        {
            // Set flag here so it can only activate once
            isActive = false;

            damagable.TakeDamage(damage);

            objectPool.Get(onHitVFX, transform.position, Quaternion.identity);
            objectPool.Remove(gameObject);
        }   
    }

    public void SetupProjectile(Vector3 targetPosition, IDamagable newDamagable, float newDamage, float newSpeed, ObjectPoolManager newObjPool)
    {
        // 对象池复用时重置目标、伤害、速度和拖尾。
        trail.Clear();
        isActive = true;
        target = targetPosition;
        damagable = newDamagable;
        damage = newDamage;
        speed = newSpeed;
        objectPool = newObjPool;
    }
}
