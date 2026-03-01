using System.Collections.Generic;
using UnityEngine;

public sealed class NoteSpawner : MonoBehaviour
{
    [Header("풀")]
    [SerializeField] private int prewarm = 32;

    private GameManager gameManager;
    private readonly Queue<NodeMonster> pool = new Queue<NodeMonster>(128);
    private readonly List<NodeMonster> actives = new List<NodeMonster>(128);

    private GameObject nodePrefab;
    private Sprite nodeSprite;
    private Transform spawnPoint;
    private Transform activeRoot;
    private Transform poolRoot;

    private int spawnEveryNBeats = 1;
    private bool running;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void Configure(StageData stageData, Transform spawn, Transform activeParent, Transform poolParent)
    {
        spawnPoint = spawn;
        activeRoot = activeParent;
        poolRoot = poolParent;

        var prefabChanged = nodePrefab != stageData.NodeMonsterPrefab;

        nodePrefab = stageData.NodeMonsterPrefab;
        nodeSprite = stageData.NodeSprite;
        spawnEveryNBeats = Mathf.Max(1, stageData.SpawnEveryNBeats);

        if (prefabChanged)
            RebuildPool();
    }

    public void StartSpawning()
    {
        if (gameManager == null || gameManager.Events == null) return;

        StopSpawning();
        running = true;
        gameManager.Events.Beat += OnBeat;
    }

    public void StopSpawning()
    {
        running = false;
        if (gameManager != null && gameManager.Events != null)
            gameManager.Events.Beat -= OnBeat;
    }

    public void ClearAll()
    {
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            var node = actives[i];
            if (node == null) continue;
            Release(node);
        }

        actives.Clear();
    }

    private void RebuildPool()
    {
        ClearAll();
        while (pool.Count > 0) pool.Dequeue();

        if (nodePrefab == null) return;

        for (int i = 0; i < prewarm; i++)
        {
            var node = CreateNew();
            Release(node);
        }
    }

    private NodeMonster CreateNew()
    {
        var go = Instantiate(nodePrefab, poolRoot != null ? poolRoot : transform);
        var node = go.GetComponent<NodeMonster>();
        node.Initialize(gameManager, Release);
        go.SetActive(false);
        return node;
    }

    private NodeMonster Get()
    {
        NodeMonster node;

        if (pool.Count > 0) node = pool.Dequeue();
        else node = CreateNew();

        if (activeRoot != null)
            node.transform.SetParent(activeRoot, true);

        actives.Add(node);
        return node;
    }

    private void Release(NodeMonster node)
    {
        if (node == null) return;

        actives.Remove(node);

        if (poolRoot != null)
            node.transform.SetParent(poolRoot, true);

        node.gameObject.SetActive(false);
        pool.Enqueue(node);
    }

    private void OnBeat(BeatInfo beatInfo)
    {
        if (!running) return;
        if (nodePrefab == null || spawnPoint == null) return;

        if (beatInfo.BeatIndex % spawnEveryNBeats != 0)
            return;

        var node = Get();
        node.Spawn(spawnPoint.position, nodeSprite);
    }

    private void OnDestroy()
    {
        StopSpawning();
    }
}