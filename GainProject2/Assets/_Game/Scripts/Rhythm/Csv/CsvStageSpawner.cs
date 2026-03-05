using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using _Game.Scripts.Rhythm;
using GnalIhu.Rhythm;

public sealed class CsvStageSpawner : MonoBehaviour
{
    [System.Serializable]
    private struct MonsterEntry
    {
        [Tooltip("CSV의 monsterId")] public int id;
        [Tooltip("스폰할 프리팹")] public GameObject prefab;
    }

    private struct SpawnEvent
    {
        public float impactBeat;
        public int lane;
        public int monsterId;
    }

    [Header("참조")]
    [SerializeField] private RhythmConductor conductor;

    [Header("몬스터 매핑")]
    [SerializeField, Tooltip("CSV monsterId -> 프리팹 매핑")] private MonsterEntry[] monsterEntries;

    [Header("디버그")]
    [SerializeField, Tooltip("경고 로그 출력")] private bool verboseLog = true;

    private readonly Dictionary<int, GameObject> idToPrefab = new Dictionary<int, GameObject>(32);
    private readonly HashSet<int> missingLoggedIds = new HashSet<int>();

    private CsvSpawnPatternSO pattern;
    private float bpm;
    private float travelTime;
    private float songLength;
    private float travelBeat;

    private readonly List<SpawnEvent> events = new List<SpawnEvent>(256);
    private int spawnIndex;
    private bool spawning;

    private Transform[] laneSpawnPoints;

    public void SetConductor(RhythmConductor conductorRef)
    {
        if (conductorRef != null) conductor = conductorRef;
    }

    public void SetLaneSpawnPoints(Transform[] lanes)
    {
        laneSpawnPoints = lanes;
    }

    public void Configure(
        CsvSpawnPatternSO patternSO,
        MonsterCatalogSO monsterCatalogSO,
        float stageBpm,
        float nodeTravelTime,
        float forcedSongLength)
    {
        pattern = patternSO;
        bpm = stageBpm;
        travelTime = nodeTravelTime;
        songLength = forcedSongLength;

        if (conductor == null)
            conductor = FindFirstObjectByType<RhythmConductor>();

        if (bpm <= 0f) bpm = 120f;

        travelBeat = travelTime * (bpm / 60f);

        BuildPrefabMap();
        ParseCsv();

        spawnIndex = 0;
        spawning = true;
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    public void NotifyPassedHitLine(int lane, NodeMonster node)
    {
        if (node == null) return;
    }

    private void Update()
    {
        if (!spawning) return;
        if (conductor == null) return;
        if (!conductor.IsRunning) return;
        if (laneSpawnPoints == null || laneSpawnPoints.Length == 0) return;
        if (spawnIndex >= events.Count) return;

        if (songLength > 0f && conductor.SongTime >= songLength)
        {
            spawning = false;
            return;
        }

        float currentBeat = conductor.CurrentBeat;
        SpawnEvent e = events[spawnIndex];

        float spawnBeat = e.impactBeat - travelBeat;
        if (spawnBeat < 0f) spawnBeat = 0f;

        if (currentBeat >= spawnBeat)
        {
            Spawn(e);
            spawnIndex++;
        }
    }

    private void Spawn(SpawnEvent e)
    {
        int laneIndex = e.lane - 1;
        if (laneIndex < 0 || laneIndex >= laneSpawnPoints.Length) return;

        if (!idToPrefab.TryGetValue(e.monsterId, out GameObject prefab) || prefab == null)
        {
            if (verboseLog && !missingLoggedIds.Contains(e.monsterId))
            {
                missingLoggedIds.Add(e.monsterId);
                Debug.LogWarning($"[CsvStageSpawner] monsterId 매핑 누락: {e.monsterId}");
            }
            return;
        }

        Transform spawn = laneSpawnPoints[laneIndex];
        if (spawn == null) return;

        Instantiate(prefab, spawn.position, Quaternion.identity);
    }

    private void BuildPrefabMap()
    {
        idToPrefab.Clear();
        missingLoggedIds.Clear();

        if (monsterEntries == null) return;

        for (int i = 0; i < monsterEntries.Length; i++)
        {
            MonsterEntry e = monsterEntries[i];
            if (e.prefab == null) continue;
            idToPrefab[e.id] = e.prefab;
        }
    }

    private void ParseCsv()
    {
        events.Clear();

        if (pattern == null || pattern.Csv == null)
        {
            if (verboseLog) Debug.LogWarning("[CsvStageSpawner] CsvSpawnPatternSO 또는 CSV(TextAsset)가 비어있음");
            return;
        }

        string text = pattern.Csv.text;
        if (string.IsNullOrEmpty(text))
        {
            if (verboseLog) Debug.LogWarning("[CsvStageSpawner] CSV 내용이 비어있음");
            return;
        }

        text = text.Replace("\r", "");

        string[] lines = text.Split('\n');
        int start = pattern.HasHeader ? 1 : 0;

        for (int i = start; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            line = line.Trim();
            if (line.Length == 0) continue;

            string[] cols = line.Split(pattern.Delimiter);
            if (cols.Length < 3)
            {
                if (verboseLog) Debug.LogWarning($"[CsvStageSpawner] CSV 컬럼 부족(line {i + 1}): {line}", pattern);
                continue;
            }

            string beatStr = cols[0].Trim().Trim('\uFEFF');
            string laneStr = cols[1].Trim();
            string idStr = cols[2].Trim();

            if (!float.TryParse(beatStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float beat))
            {
                if (verboseLog) Debug.LogWarning($"[CsvStageSpawner] impactBeat 파싱 실패(line {i + 1}): '{beatStr}' | {line}", pattern);
                continue;
            }

            if (!int.TryParse(laneStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int lane))
            {
                if (verboseLog) Debug.LogWarning($"[CsvStageSpawner] lane 파싱 실패(line {i + 1}): '{laneStr}' | {line}", pattern);
                continue;
            }

            if (!int.TryParse(idStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int monsterId))
            {
                if (verboseLog) Debug.LogWarning($"[CsvStageSpawner] monsterId 파싱 실패(line {i + 1}): '{idStr}' | {line}", pattern);
                continue;
            }

            if (lane < 1) lane = 1;
            if (lane > 4) lane = 4;

            SpawnEvent e;
            e.impactBeat = beat;
            e.lane = lane;
            e.monsterId = monsterId;

            events.Add(e);
        }

        events.Sort((a, b) => a.impactBeat.CompareTo(b.impactBeat));

        if (verboseLog)
            Debug.Log($"[CsvStageSpawner] CSV 로드 완료: {events.Count}개 이벤트");
    }
}