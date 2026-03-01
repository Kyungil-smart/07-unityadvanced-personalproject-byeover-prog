using System;
using System.Collections.Generic;
using UnityEngine;

namespace GnalIhu.Rhythm
{
    /// <summary>
    /// 패턴(SO)을 읽어서 "박 기준"으로 스폰한다.
    /// - loop=true면 lengthBeats마다 반복.
    /// - 스폰된 오브젝트에 LaneMover를 초기화해 리듬 이동을 보장.
    /// </summary>
    public class RhythmSpawner : MonoBehaviour
    {
        [Serializable]
        public class LaneBinding
        {
            [Tooltip("패턴에서 참조할 레인 ID. 예: A")]
            public string id;

            [Tooltip("씬에 있는 RhythmLane 오브젝트")]
            public RhythmLane lane;
        }

        [Header("참조")]
        [SerializeField]
        [Tooltip("리듬의 기준(Conductor).")]
        private RhythmConductor conductor;

        [SerializeField]
        [Tooltip("스폰 패턴(ScriptableObject).")]
        private RhythmSpawnPatternSO pattern;

        [SerializeField]
        [Tooltip("레인 ID와 씬 레인 오브젝트를 연결.")]
        private List<LaneBinding> lanes = new List<LaneBinding>();

        [Header("스폰 옵션")]
        [SerializeField]
        [Tooltip("스폰된 몹들을 이 Transform 아래로 정리합니다(선택). 비우면 월드 루트에 생성.")]
        private Transform spawnedParent;

        [SerializeField]
        [Tooltip("디버그 로그 출력")]
        private bool logSpawn = false;

        private readonly Dictionary<string, RhythmLane> laneMap = new Dictionary<string, RhythmLane>();
        private readonly List<CachedSpawnEvent> cachedEvents = new List<CachedSpawnEvent>();

        private int eventIndex;
        private double cycleOffsetBeats; // 0, length, 2*length...
        private bool cacheBuilt;

        private struct CachedSpawnEvent
        {
            public double spawnBeat;     // 0~length 사이(또는 그 이상일 수도 있음)
            public string laneId;
            public GameObject prefab;
        }

        private void Awake()
        {
            RebuildLaneMap();
            RebuildCache();
        }

        private void OnValidate()
        {
            RebuildLaneMap();
            RebuildCache();
        }

        private void RebuildLaneMap()
        {
            laneMap.Clear();

            foreach (var b in lanes)
            {
                if (b == null) continue;
                if (string.IsNullOrWhiteSpace(b.id)) continue;
                if (b.lane == null) continue;

                // 같은 ID가 중복이면 마지막이 덮어쓴다(초보 실수 방지)
                laneMap[b.id] = b.lane;
            }
        }

        private void RebuildCache()
        {
            cachedEvents.Clear();
            cacheBuilt = false;

            if (pattern == null) return;

            // cue 정렬이 안 되어 있어도 내부 캐시는 정렬해서 사용
            foreach (var cue in pattern.cues)
            {
                if (cue.prefab == null) continue;
                if (cue.count < 1) continue;

                double baseBeat = cue.beat + cue.subBeat;
                double spacing = Math.Max(0.0, cue.withinCueSpacingBeats);

                for (int i = 0; i < cue.count; i++)
                {
                    cachedEvents.Add(new CachedSpawnEvent
                    {
                        spawnBeat = baseBeat + spacing * i,
                        laneId = cue.laneId,
                        prefab = cue.prefab
                    });
                }
            }

            cachedEvents.Sort((a, b) => a.spawnBeat.CompareTo(b.spawnBeat));

            eventIndex = 0;
            cycleOffsetBeats = 0;
            cacheBuilt = true;
        }

        private void Update()
        {
            if (conductor == null || pattern == null) return;
            if (!conductor.IsRunning) return;
            if (!cacheBuilt || cachedEvents.Count == 0) return;

            // leadIn 동안은 스폰 안 함(원하면 정책 변경 가능)
            if (conductor.SongTime < 0) return;

            double currentBeat = conductor.CurrentBeat;

            // 현재 박이 다음 이벤트 박을 지났으면 계속 스폰
            while (true)
            {
                if (eventIndex >= cachedEvents.Count)
                {
                    if (pattern.loop)
                    {
                        eventIndex = 0;
                        cycleOffsetBeats += pattern.lengthBeats;
                    }
                    else
                    {
                        break;
                    }
                }

                double targetBeat = cycleOffsetBeats + cachedEvents[eventIndex].spawnBeat;

                if (currentBeat < targetBeat)
                    break;

                SpawnOne(cachedEvents[eventIndex], targetBeat);
                eventIndex++;
            }
        }

        private void SpawnOne(CachedSpawnEvent ev, double scheduledBeat)
        {
            if (!laneMap.TryGetValue(ev.laneId, out RhythmLane lane) || lane == null)
            {
                if (logSpawn)
                    Debug.LogWarning($"[RhythmSpawner] LaneId '{ev.laneId}' 를 찾지 못했습니다. Spawner의 Lane Bindings를 확인하세요.", this);
                return;
            }

            GameObject go = spawnedParent == null
                ? Instantiate(ev.prefab)
                : Instantiate(ev.prefab, spawnedParent);

            // 레인 시작점으로 일단 이동(실제 위치는 mover가 박 기준으로 다시 맞춘다)
            go.transform.position = lane.StartPosition;

            var mover = go.GetComponent<RhythmLaneMover>();
            if (mover == null)
                mover = go.AddComponent<RhythmLaneMover>();

            mover.Initialize(conductor, lane, scheduledBeat);

            if (logSpawn)
                Debug.Log($"[RhythmSpawner] Spawn '{go.name}' lane={ev.laneId} beat={scheduledBeat:0.000}", this);
        }

        // 에디터/툴에서 빠르게 연결하기 위한 함수(선택)
        public void AssignConductor(RhythmConductor c) => conductor = c;
    }
}