using System.Collections.Generic;
using UnityEngine;

public sealed class LightningVfxPool : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private LightningFx prefab;

    [Header("풀")]
    [SerializeField] private int prewarm = 16;

    [Header("스폰")]
    [SerializeField, Tooltip("타겟 위로 시작 높이")] private float startHeight = 6f;
    [SerializeField, Tooltip("타겟 기준 X 오프셋")] private float xOffset = 0f;

    private readonly Queue<LightningFx> pool = new Queue<LightningFx>(64);
    private Transform target;
    private GameManager gameManager;

    public void Initialize(GameManager manager, Transform bossTarget)
    {
        if (gameManager != null)
            gameManager.Events.NodeSuccess -= SpawnForTarget;

        gameManager = manager;
        target = bossTarget;

        if (prefab == null || gameManager == null) return;

        if (pool.Count == 0)
        {
            for (int i = 0; i < prewarm; i++)
                CreateNew();
        }

        gameManager.Events.NodeSuccess += SpawnForTarget;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private LightningFx CreateNew()
    {
        var fx = Instantiate(prefab, transform);
        fx.gameObject.SetActive(false);
        pool.Enqueue(fx);
        return fx;
    }

    private LightningFx Get()
    {
        if (pool.Count == 0) return CreateNew();
        return pool.Dequeue();
    }

    private void Return(LightningFx fx)
    {
        fx.gameObject.SetActive(false);
        fx.transform.SetParent(transform, false);
        pool.Enqueue(fx);
    }

    private void SpawnForTarget()
    {
        if (target == null || prefab == null) return;

        var fx = Get();
        var to = target.position + new Vector3(xOffset, 0f, 0f);
        var from = to + Vector3.up * startHeight;

        fx.Play(from, to, Return);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.Events.NodeSuccess -= SpawnForTarget;
    }
}