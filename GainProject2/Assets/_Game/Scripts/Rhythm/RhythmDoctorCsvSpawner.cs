using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using GnalIhu.Rhythm;

[DisallowMultipleComponent]
public sealed class RhythmDoctorCsvSpawner : MonoBehaviour
{
    [Serializable]
    private struct Row
    {
        public double orderBeat;
        public int lane;
        public string monsterId;
    }

    [Header("참조")]
    [SerializeField] private RhythmConductor conductor;
    [SerializeField] private CsvSpawnPatternSO csvPatternSO;
    [SerializeField] private MonsterCatalogSO monsterCatalogSO;

    [Header("레인 활성 개수")]
    [SerializeField, Tooltip("1~4")] private int activeLaneCount = 1;

    [Header("스폰 포인트")]
    [SerializeField] private Transform lane1Spawn;
    [SerializeField] private Transform lane2Spawn;
    [SerializeField] private Transform lane3Spawn;
    [SerializeField] private Transform lane4Spawn;

    [Header("스폰 정책")]
    [SerializeField, Tooltip("히트라인 통과 후 '다음 박자'에 스폰")] private bool spawnOnNextBeat = true;

    [Header("디버그")]
    [SerializeField] private bool debugLog;

    private readonly List<string>[] laneQueues =
    {
        null,
        new List<string>(256),
        new List<string>(256),
        new List<string>(256),
        new List<string>(256)
    };

    private readonly bool[] laneReady = { false, false, false, false, false };
    private readonly int[] laneCurrentInstanceId = { 0, 0, 0, 0, 0 };

    private bool running;

    private void Awake()
    {
        if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();
    }

    private void OnEnable()
    {
        if (conductor != null) conductor.OnBeat += HandleBeat;
    }

    private void OnDisable()
    {
        if (conductor != null) conductor.OnBeat -= HandleBeat;
    }

    public void Configure(CsvSpawnPatternSO patternSO, MonsterCatalogSO catalogSO, int laneCount)
    {
        csvPatternSO = patternSO;
        monsterCatalogSO = catalogSO;
        activeLaneCount = Mathf.Clamp(laneCount, 1, 4);

        BuildQueuesFromCsv();

        for (int lane = 1; lane <= 4; lane++)
        {
            laneReady[lane] = false;
            laneCurrentInstanceId[lane] = 0;
        }

        running = true;

        SpawnInitial();
    }

    private void SpawnInitial()
    {
        for (int lane = 1; lane <= activeLaneCount; lane++)
        {
            if (laneQueues[lane].Count == 0) continue;
            if (spawnOnNextBeat) laneReady[lane] = true;
            else SpawnNext(lane);
        }
    }

    private void HandleBeat(int beatIndex)
    {
        if (!running) return;

        for (int lane = 1; lane <= activeLaneCount; lane++)
        {
            if (!laneReady[lane]) continue;
            if (laneCurrentInstanceId[lane] != 0) continue;
            if (laneQueues[lane].Count == 0) continue;

            SpawnNext(lane);
            laneReady[lane] = false;
        }
    }

    public void NotifyPassedHitLine(int lane, NodeMonster node)
    {
        if (!running) return;
        if (lane < 1 || lane > 4) return;
        if (lane > activeLaneCount) return;
        if (node == null) return;

        int id = node.GetInstanceID();
        if (laneCurrentInstanceId[lane] != id) return;

        laneCurrentInstanceId[lane] = 0;

        if (laneQueues[lane].Count == 0) return;

        if (spawnOnNextBeat) laneReady[lane] = true;
        else SpawnNext(lane);

        if (debugLog) Debug.Log($"[RhythmDoctor] Lane {lane} passed hitline -> open next", this);
    }

    private void SpawnNext(int lane)
    {
        if (monsterCatalogSO == null) return;
        if (csvPatternSO == null) return;

        if (laneQueues[lane].Count == 0) return;

        string id = laneQueues[lane][0];
        laneQueues[lane].RemoveAt(0);

        if (!monsterCatalogSO.TryGetPrefab(id, out var prefab) || prefab == null)
        {
            if (debugLog) Debug.LogWarning($"[RhythmDoctor] monsterId 매핑 실패: {id}", this);
            return;
        }

        Transform sp = GetSpawnPoint(lane);
        if (sp == null)
        {
            if (debugLog) Debug.LogWarning($"[RhythmDoctor] SpawnPoint 누락: lane={lane}", this);
            return;
        }

        var go = Instantiate(prefab, sp.position, sp.rotation);
        laneCurrentInstanceId[lane] = go.GetInstanceID();

        if (debugLog) Debug.Log($"[RhythmDoctor] Spawn lane={lane} id={id}", this);
    }

    private Transform GetSpawnPoint(int lane)
    {
        return lane switch
        {
            1 => lane1Spawn,
            2 => lane2Spawn,
            3 => lane3Spawn,
            4 => lane4Spawn,
            _ => null
        };
    }

    private void BuildQueuesFromCsv()
    {
        for (int lane = 1; lane <= 4; lane++)
            laneQueues[lane].Clear();

        if (csvPatternSO == null || csvPatternSO.Csv == null) return;

        string text = csvPatternSO.Csv.text;
        if (string.IsNullOrWhiteSpace(text)) return;

        var rows = new List<Row>(512);

        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        int start = csvPatternSO.HasHeader ? 1 : 0;

        for (int i = start; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith("#", StringComparison.Ordinal)) continue;

            var cols = line.Split(csvPatternSO.Delimiter);
            if (cols.Length < 3) continue;

            if (!TryParseDouble(cols[0], out double beat)) beat = i;
            if (!int.TryParse(cols[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int lane)) continue;

            string id = cols[2].Trim();
            if (lane < 1 || lane > 4) continue;
            if (string.IsNullOrWhiteSpace(id)) continue;

            rows.Add(new Row { orderBeat = beat, lane = lane, monsterId = id });
        }

        rows.Sort((a, b) =>
        {
            int c = a.orderBeat.CompareTo(b.orderBeat);
            if (c != 0) return c;
            return a.lane.CompareTo(b.lane);
        });

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            laneQueues[r.lane].Add(r.monsterId);
        }

        if (debugLog)
        {
            Debug.Log($"[RhythmDoctor] Queue built. L1={laneQueues[1].Count}, L2={laneQueues[2].Count}, L3={laneQueues[3].Count}, L4={laneQueues[4].Count}", this);
        }
    }

    private static bool TryParseDouble(string s, out double v)
    {
        return double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }
}