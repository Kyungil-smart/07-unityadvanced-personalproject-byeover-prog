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
    private GameObject[] randomPrefabs; // 랜덤 몬스터 배열
    private string currentPattern;      // 현재 리듬 패턴
    
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

        bool prefabChanged = (nodePrefab != stageData.NodeMonsterPrefab) || (randomPrefabs != stageData.RandomMonsterPrefabs);

        nodePrefab = stageData.NodeMonsterPrefab;
        randomPrefabs = stageData.RandomMonsterPrefabs; // 랜덤 배열 할당
        currentPattern = stageData.SpawnPattern;        // 패턴 할당
        
        nodeSprite = stageData.NodeSprite;
        spawnEveryNBeats = Mathf.Max(1, stageData.SpawnEveryNBeats);

        if (prefabChanged)
            RebuildPool();
    }

    public void StartSpawning()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
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

        if (nodePrefab == null && (randomPrefabs == null || randomPrefabs.Length == 0)) return;

        for (int i = 0; i < prewarm; i++)
        {
            var node = CreateNew();
            if (node != null) Release(node);
        }
    }

    private NodeMonster CreateNew()
    {
        GameObject prefabToSpawn = nodePrefab;

        // 랜덤 몬스터가 등록되어 있다면 무작위로 하나를 뽑아서 생성
        if (randomPrefabs != null && randomPrefabs.Length > 0)
        {
            prefabToSpawn = randomPrefabs[Random.Range(0, randomPrefabs.Length)];
        }

        if (prefabToSpawn == null) return null;

        var go = Instantiate(prefabToSpawn, poolRoot != null ? poolRoot : transform);
        var node = go.GetComponent<NodeMonster>();
        
        if (node == null)
        {
            Debug.LogError($"[NoteSpawner] 생성 실패! {prefabToSpawn.name} 프리팹에 'NodeMonster' 컴포넌트가 없습니다.", go);
            go.SetActive(false);
            return null;
        }

        node.Initialize(gameManager != null ? gameManager : GameManager.Instance, Release);
        go.SetActive(false);
        return node;
    }

    private NodeMonster Get()
    {
        NodeMonster node;

        if (pool.Count > 0) 
            node = pool.Dequeue();
        else 
            node = CreateNew();

        if (node != null && activeRoot != null)
            node.transform.SetParent(activeRoot, true);

        if (node != null) actives.Add(node);
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
        if (!running || spawnPoint == null) return;

        // 패턴 문자열이 있을 경우 찰진 패턴 적용
        if (!string.IsNullOrEmpty(currentPattern))
        {
            int index = beatInfo.BeatIndex % currentPattern.Length;
            if (currentPattern[index] != '1') 
                return; // '1'이 아니면 스폰하지 않고 쉽니다.
        }
        // 패턴이 없으면 기존 메트로놈 방식 사용
        else
        {
            if (beatInfo.BeatIndex % spawnEveryNBeats != 0)
                return;
        }

        var node = Get();
        if (node != null) node.Spawn(spawnPoint.position, nodeSprite);
    }

    private void OnDestroy() => StopSpawning();
}