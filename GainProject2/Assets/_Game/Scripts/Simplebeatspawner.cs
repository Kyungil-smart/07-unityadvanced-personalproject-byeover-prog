using System;
using UnityEngine;
using _Game.Scripts.Monster;

/// <summary>
/// 비트 스포너 (구간별 난이도 변화 지원)
/// 
/// 한 스테이지 안에서 Phase를 나눠서 패턴이 바뀜.
/// 예: 0~64박 쉬움 → 65~192박 보통 → 193~345박 어려움
/// </summary>
[DisallowMultipleComponent]
public sealed class SimpleBeatSpawner : MonoBehaviour
{
    [Serializable]
    public class LaneSetup
    {
        public GameObject prefab;
        [Tooltip("패턴 (1=스폰, 0=빈박). 루프됨")]
        public string pattern = "";
    }

    [Serializable]
    public class Phase
    {
        [Tooltip("이 Phase가 시작되는 박자")]
        public int startBeat = 0;
        [Tooltip("Phase 이름 (디버그용)")]
        public string phaseName = "Phase";
        public LaneSetup lane1 = new LaneSetup();
        public LaneSetup lane2 = new LaneSetup();
        public LaneSetup lane3 = new LaneSetup();
        public LaneSetup lane4 = new LaneSetup();
    }

    [Serializable]
    public class StagePreset
    {
        public string stageName;
        [Tooltip("구간별 패턴. startBeat 오름차순으로 정렬!")]
        public Phase[] phases;
        [Tooltip("이 박자에서 스폰 중단 (0=무제한)")]
        public int stopAfterBeat = 0;
    }

    [Header("시스템")]
    [SerializeField] private RhythmConductor conductor;

    [Header("스폰 포인트")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform spawnPoint3;
    [SerializeField] private Transform spawnPoint4;

    [Header("스테이지 프리셋")]
    [SerializeField] private StagePreset[] stagePresets;

    [Header("디버그")]
    [SerializeField] private bool logSpawn = true;

    private bool active;
    private GameManager gameManager;
    private StagePreset currentPreset;
    private Phase currentPhase;
    private int currentPhaseIndex;
    private Transform[] spawnPoints;

    private void Awake()
    {
        if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();
        spawnPoints = new[] { spawnPoint1, spawnPoint2, spawnPoint3, spawnPoint4 };
    }

    private void OnEnable()
    {
        if (conductor != null) conductor.OnBeat += OnBeat;
    }

    private void OnDisable()
    {
        if (conductor != null) conductor.OnBeat -= OnBeat;
    }

    public void LoadStage(int stageIndex)
    {
        if (stagePresets == null || stagePresets.Length == 0) return;
        int idx = Mathf.Clamp(stageIndex, 0, stagePresets.Length - 1);
        currentPreset = stagePresets[idx];
        currentPhaseIndex = 0;
        currentPhase = (currentPreset.phases != null && currentPreset.phases.Length > 0)
            ? currentPreset.phases[0] : null;

        if (logSpawn)
            Debug.Log($"[SimpleBeatSpawner] LoadStage({stageIndex}) → {currentPreset.stageName} phases={currentPreset.phases?.Length ?? 0} stopBeat={currentPreset.stopAfterBeat}");
    }

    public void StartSpawning() => active = true;
    public void StopSpawning() => active = false;

    private void OnBeat(int beatIndex)
    {
        if (!active || currentPreset == null) return;
        if (currentPreset.stopAfterBeat > 0 && beatIndex >= currentPreset.stopAfterBeat)
        {
            active = false;
            return;
        }

        // ★ 구간 전환 체크
        UpdatePhase(beatIndex);

        if (currentPhase == null) return;

        gameManager ??= GameManager.Instance;

        var lanes = new[] { currentPhase.lane1, currentPhase.lane2, currentPhase.lane3, currentPhase.lane4 };

        for (int i = 0; i < 4; i++)
        {
            var lane = lanes[i];
            if (lane == null || lane.prefab == null) continue;
            if (string.IsNullOrEmpty(lane.pattern)) continue;
            if (spawnPoints[i] == null) continue;

            // ★ Phase 시작 박자 기준으로 패턴 인덱스 계산
            int beatInPhase = beatIndex - currentPhase.startBeat;
            if (beatInPhase < 0) continue;

            int patternIndex = beatInPhase % lane.pattern.Length;
            if (lane.pattern[patternIndex] != '1') continue;

            SpawnMonster(lane.prefab, spawnPoints[i], i + 1, beatIndex);
        }
    }

    /// <summary>현재 박자에 맞는 Phase로 전환</summary>
    private void UpdatePhase(int beatIndex)
    {
        if (currentPreset.phases == null || currentPreset.phases.Length == 0) return;

        // 다음 Phase 확인
        int nextIdx = currentPhaseIndex + 1;
        while (nextIdx < currentPreset.phases.Length && beatIndex >= currentPreset.phases[nextIdx].startBeat)
        {
            currentPhaseIndex = nextIdx;
            currentPhase = currentPreset.phases[nextIdx];

            if (logSpawn)
                Debug.Log($"[SimpleBeatSpawner] ▶ Phase 전환: {currentPhase.phaseName} (beat {beatIndex})");

            nextIdx++;
        }
    }

    private void SpawnMonster(GameObject prefab, Transform sp, int laneNumber, int beatIndex)
    {
        var go = Instantiate(prefab, sp.position, sp.rotation);

        if (go.TryGetComponent(out NodeMonster node))
        {
            node.Initialize(gameManager, n => Destroy(n.gameObject));
            node.Spawn(sp.position, null);
        }

        InitializeAllMovers(go);

        if (logSpawn)
            Debug.Log($"[SimpleBeatSpawner] beat={beatIndex} lane={laneNumber} monster={prefab.name}");
    }

    private void InitializeAllMovers(GameObject go)
    {
        var ghost = go.GetComponent<GhostMover>();
        if (ghost != null) ghost.Initialize(conductor);

        var boar = go.GetComponent<BoarMover>();
        if (boar != null) boar.Initialize(conductor);

        var snake = go.GetComponent<SnakeMover>();
        if (snake != null) snake.Initialize(conductor);

        var raven = go.GetComponent<RavenMover>();
        if (raven != null) raven.Initialize(conductor);

        var beatStep = go.GetComponent<BeatStepMover>();
        if (beatStep != null && gameManager != null)
            beatStep.Initialize(gameManager.Events, gameManager.Settings);
    }
}