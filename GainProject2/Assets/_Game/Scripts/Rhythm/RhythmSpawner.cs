using System;
using System.Collections.Generic;
using UnityEngine;

namespace GnalIhu.Rhythm
{
    public class RhythmSpawner : MonoBehaviour
    {
        [Serializable]
        public class LaneBinding
        {
            public string id;
            public RhythmLane lane;
        }

        [Header("참조")]
        [SerializeField] private RhythmConductor conductor;
        [SerializeField] private RhythmSpawnPatternSO pattern;
        [SerializeField] private List<LaneBinding> lanes = new List<LaneBinding>();

        [Header("스폰 옵션")]
        [SerializeField] private Transform spawnedParent;
        [Tooltip("디버그 로그 출력")]
        [SerializeField]private bool logSpawn = false;
        
        

        private readonly Dictionary<string, RhythmLane> laneMap = new Dictionary<string, RhythmLane>();
        private readonly List<CachedSpawnEvent> cachedEvents = new List<CachedSpawnEvent>();
        private readonly List<GameObject> activeMonsters = new List<GameObject>(); // 몬스터 청소용

        private int eventIndex;
        private double cycleOffsetBeats;
        private bool cacheBuilt;

        private struct CachedSpawnEvent
        {
            public double spawnBeat;
            public string laneId;
            public GameObject prefab;
        }
        
        public void AssignConductor(RhythmConductor c)
        {
            conductor = c;
        }

        private void Awake() { RebuildLaneMap(); }

        // 스테이지가 바뀔 때마다 새로운 패턴을 세팅하는 함수
        public void SetPattern(RhythmSpawnPatternSO newPattern)
        {
            pattern = newPattern;
            RebuildLaneMap();
            RebuildCache();
        }

        public void ClearSpawnedMonsters()
        {
            foreach (var mon in activeMonsters)
                if (mon != null) Destroy(mon);
            activeMonsters.Clear();
        }

        private void RebuildLaneMap()
        {
            laneMap.Clear();
            foreach (var b in lanes)
            {
                if (b != null && !string.IsNullOrWhiteSpace(b.id) && b.lane != null)
                    laneMap[b.id] = b.lane;
            }
        }

        private void RebuildCache()
        {
            cachedEvents.Clear();
            cacheBuilt = false;
            if (pattern == null) return;

            foreach (var cue in pattern.cues)
            {
                if (cue.prefab == null || cue.count < 1) continue;
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
            if (conductor == null || pattern == null || !conductor.IsRunning) return;
            if (!cacheBuilt || cachedEvents.Count == 0) return;
            if (conductor.SongTime < 0) return;

            double currentBeat = conductor.CurrentBeat;

            while (true)
            {
                if (eventIndex >= cachedEvents.Count)
                {
                    if (pattern.loop) { eventIndex = 0; cycleOffsetBeats += pattern.lengthBeats; }
                    else break;
                }

                double targetBeat = cycleOffsetBeats + cachedEvents[eventIndex].spawnBeat;
                if (currentBeat < targetBeat) break;

                SpawnOne(cachedEvents[eventIndex], targetBeat);
                eventIndex++;
            }
        }

        private void SpawnOne(CachedSpawnEvent ev, double scheduledBeat)
        {
            if (!laneMap.TryGetValue(ev.laneId, out RhythmLane lane) || lane == null) return;

            GameObject go = spawnedParent == null ? Instantiate(ev.prefab) : Instantiate(ev.prefab, spawnedParent);
            go.transform.position = lane.StartPosition;

            var mover = go.GetComponent<RhythmLaneMover>();
            if (mover == null) mover = go.AddComponent<RhythmLaneMover>();

            mover.Initialize(conductor, lane, scheduledBeat);
            activeMonsters.Add(go);
        }
    }
}