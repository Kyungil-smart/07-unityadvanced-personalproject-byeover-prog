using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using GnalIhu.Rhythm;

[DisallowMultipleComponent]
public sealed class CsvStageSpawner : MonoBehaviour
{
    [Serializable]
    private struct SpawnRow
    {
        public double spawnBeat;
        public int lane;
        public string monsterId;
    }

    [Header("참조")]
    [SerializeField] private RhythmConductor conductor;

    [Header("스폰 포인트")]
    [SerializeField] private Transform lane1Spawn;
    [SerializeField] private Transform lane2Spawn;
    [SerializeField] private Transform lane3Spawn;
    [SerializeField] private Transform lane4Spawn;

    [Header("이동 시간")]
    [SerializeField, Tooltip("스폰 → 히트라인 도착 시간(초)")] private float travelTimeSeconds = 1.2f;

    [Header("스폰 간격 제한")]
    [SerializeField, Tooltip("같은 레인 최소 스폰 간격(박자)")] private float minBeatGapPerLane = 1f;
    [SerializeField, Tooltip("전체 최소 스폰 간격(박자)")] private float minBeatGapGlobal = 0.25f;

    [Header("리듬닥터 게이트")]
    [SerializeField, Tooltip("활성화하면 레인당 1마리만 유지(히트라인 통과 후 다음 박자에 다음 스폰)")]
    private bool rhythmDoctorGate = true;

    [Header("디버그")]
    [SerializeField] private bool verboseLog;

    private readonly List<SpawnRow> rows = new List<SpawnRow>(1024);

    private readonly List<SpawnRow>[] laneRows = new List<SpawnRow>[5];
    private readonly int[] laneCursor = { 0, 0, 0, 0, 0 };

    private readonly int[] laneCurrentId = { 0, 0, 0, 0, 0 };
    private readonly double[] laneNextAllowedBeat = { 0, 0, 0, 0, 0 };

    private int cursor;
    private bool running;

    private double bpm;
    private double beatDur;

    private MonsterCatalogSO catalog;
    private CsvSpawnPatternSO pattern;

    public void Configure(CsvSpawnPatternSO patternSO, MonsterCatalogSO catalogSO, float stageBpm, float travelSeconds)
    {
        pattern = patternSO;
        catalog = catalogSO;

        bpm = Math.Max(1.0, stageBpm);
        beatDur = 60.0 / bpm;

        travelTimeSeconds = Mathf.Max(0.05f, travelSeconds);

        BuildRows();

        cursor = 0;
        for (int lane = 1; lane <= 4; lane++)
        {
            laneCursor[lane] = 0;
            laneCurrentId[lane] = 0;
            laneNextAllowedBeat[lane] = 0.0;
        }

        running = true;

        if (verboseLog)
            Debug.Log($"[CsvStageSpawner] configured rows={rows.Count} bpm={bpm:0.##} travel={travelTimeSeconds:0.###}s gate={rhythmDoctorGate}", this);
    }

    private void Awake()
    {
        if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();

        for (int lane = 1; lane <= 4; lane++)
        {
            if (laneRows[lane] == null) laneRows[lane] = new List<SpawnRow>(256);
        }
    }

    private void Update()
    {
        if (!running) return;
        if (conductor == null) return;

        double nowBeat = conductor.SongTime / beatDur;

        if (!rhythmDoctorGate)
        {
            if (cursor >= rows.Count) return;

            while (cursor < rows.Count)
            {
                var r = rows[cursor];
                if (nowBeat + 1e-6 < r.spawnBeat) break;

                Spawn(r.lane, r.monsterId, false);
                cursor++;
            }

            return;
        }

        for (int lane = 1; lane <= 4; lane++)
        {
            if (laneCurrentId[lane] != 0) continue;
            if (nowBeat + 1e-6 < laneNextAllowedBeat[lane]) continue;
            if (laneCursor[lane] >= laneRows[lane].Count) continue;

            var r = laneRows[lane][laneCursor[lane]];
            if (nowBeat + 1e-6 < r.spawnBeat) continue;

            Spawn(lane, r.monsterId, true);
            laneCursor[lane]++;
        }
    }

    public void NotifyPassedHitLine(int lane, NodeMonster node)
    {
        if (!rhythmDoctorGate) return;
        if (lane < 1 || lane > 4) return;
        if (node == null) return;

        int id = node.GetInstanceID();
        if (laneCurrentId[lane] != id) return;

        laneCurrentId[lane] = 0;

        double nowBeat = (conductor != null) ? (conductor.SongTime / beatDur) : 0.0;
        double nextBeat = Math.Floor(nowBeat + 1e-6) + 1.0;
        laneNextAllowedBeat[lane] = nextBeat;

        if (verboseLog)
            Debug.Log($"[CsvStageSpawner] gate open lane={lane} nextAllowedBeat={laneNextAllowedBeat[lane]:0.###}", this);
    }

    private void BuildRows()
    {
        rows.Clear();
        for (int lane = 1; lane <= 4; lane++)
        {
            if (laneRows[lane] == null) laneRows[lane] = new List<SpawnRow>(256);
            laneRows[lane].Clear();
        }

        if (pattern == null || pattern.Csv == null) return;

        string text = pattern.Csv.text;
        if (string.IsNullOrWhiteSpace(text)) return;

        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        int start = pattern.HasHeader ? 1 : 0;
        double travelBeats = travelTimeSeconds / beatDur;

        for (int i = start; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith("#", StringComparison.Ordinal)) continue;

            var cols = line.Split(pattern.Delimiter);
            if (cols.Length < 3) continue;

            if (!TryParseDouble(cols[0], out double impactBeat)) continue;
            if (!int.TryParse(cols[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int lane)) continue;

            string id = cols[2].Trim();
            if (lane < 1 || lane > 4) continue;
            if (string.IsNullOrWhiteSpace(id)) continue;

            double spawnBeat = impactBeat - travelBeats;
            if (spawnBeat < 0) spawnBeat = 0;

            var row = new SpawnRow
            {
                spawnBeat = spawnBeat,
                lane = lane,
                monsterId = id
            };

            rows.Add(row);
        }

        rows.Sort((a, b) =>
        {
            int c = a.spawnBeat.CompareTo(b.spawnBeat);
            if (c != 0) return c;
            c = a.lane.CompareTo(b.lane);
            if (c != 0) return c;
            return string.Compare(a.monsterId, b.monsterId, StringComparison.OrdinalIgnoreCase);
        });

        ApplyBeatGaps();

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            laneRows[r.lane].Add(r);
        }

        if (verboseLog)
            Debug.Log($"[CsvStageSpawner] parsed rows={rows.Count} travelBeats={travelBeats:0.###} minLaneGap={minBeatGapPerLane:0.###} minGlobalGap={minBeatGapGlobal:0.###}", this);
    }

    private void ApplyBeatGaps()
    {
        if (rows.Count == 0) return;

        double laneGap = Math.Max(0.0, minBeatGapPerLane);
        double globalGap = Math.Max(0.0, minBeatGapGlobal);

        double[] lastLaneBeat = { -9999, -9999, -9999, -9999, -9999 };
        double lastGlobalBeat = -9999;

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];

            int lane = Mathf.Clamp(r.lane, 1, 4);
            double t = r.spawnBeat;

            if (laneGap > 0.0)
            {
                double prevLane = lastLaneBeat[lane];
                double minLane = prevLane + laneGap;
                if (t < minLane) t = minLane;
            }

            if (globalGap > 0.0)
            {
                double minGlobal = lastGlobalBeat + globalGap;
                if (t < minGlobal) t = minGlobal;
            }

            r.spawnBeat = t;
            rows[i] = r;

            lastLaneBeat[lane] = t;
            lastGlobalBeat = t;
        }
    }

    private void Spawn(int lane, string monsterId, bool applyGate)
    {
        if (catalog == null)
        {
            if (verboseLog) Debug.LogWarning("[CsvStageSpawner] MonsterCatalogSO 비어있음", this);
            return;
        }

        if (!catalog.TryGetPrefab(monsterId, out var prefab) || prefab == null)
        {
            if (verboseLog) Debug.LogWarning($"[CsvStageSpawner] monsterId 매핑 실패: {monsterId}", this);
            return;
        }

        Transform sp = lane switch
        {
            1 => lane1Spawn,
            2 => lane2Spawn,
            3 => lane3Spawn,
            4 => lane4Spawn,
            _ => null
        };

        if (sp == null)
        {
            if (verboseLog) Debug.LogWarning($"[CsvStageSpawner] SpawnPoint 누락: lane={lane}", this);
            return;
        }

        var go = Instantiate(prefab, sp.position, sp.rotation);

        if (applyGate)
        {
            laneCurrentId[lane] = go.GetInstanceID();
            laneNextAllowedBeat[lane] = double.PositiveInfinity;
        }
    }

    private static bool TryParseDouble(string s, out double v)
    {
        return double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }
}