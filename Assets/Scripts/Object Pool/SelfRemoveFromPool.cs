using System.Collections;
using UnityEngine;

public class SelfRemoveFromPool : MonoBehaviour
{
    // 给一次性 VFX 使用：启用后播放粒子，延迟一段时间自动回收到对象池。
    [SerializeField] private float removeDelay = 1;

    private ObjectPoolManager objPool;
    private ParticleSystem particle;

    void Awake()
    {
        objPool = ObjectPoolManager.instance;
        particle = GetComponentInChildren<ParticleSystem>();
    }

    void OnEnable()
    {
        // 对象池复用粒子时先 Clear 再 Play，避免上次播放残留。
        if (particle != null)
        {
            particle.Clear();
            particle.Play();
        }

        StartCoroutine(RemoveWithDelayCo());
    }

    private IEnumerator RemoveWithDelayCo()
    {
        // 等特效播放完，再释放回池。
        yield return new WaitForSeconds(removeDelay);
        objPool.Remove(gameObject);
    }
}
