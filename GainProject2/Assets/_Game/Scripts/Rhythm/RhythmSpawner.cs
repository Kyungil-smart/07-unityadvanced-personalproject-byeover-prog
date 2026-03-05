using System;
using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Rhythm;

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
        [SerializeField] private bool logSpawn = false;

        private readonly Dictionary<string, RhythmLane> laneMap = new Dictionary<string, RhythmLane>();
        private readonly List<CachedSpawnEvent> cachedEvents = new List<CachedSpawnEvent>();
        private readonly List<GameObject> activeMonsters = new List<GameObject>();

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

        private void Awake()
        {
            RebuildLaneMap();
        }

        public void SetPattern(RhythmSpawnPatternSO newPattern)
        {
            pattern = newPattern;
            RebuildLaneMap();
            RebuildCache();
        }

        public void ClearSpawnedMonsters()
        {
            foreach (var mon in activeMonsters)
            {
                if (mon != null) Destroy(mon);
            }
            activeMonsters.Clear();
        }

        private void RebuildLaneMap()
        {
            laneMap.Clear();
            foreach (var b in lanes)
            {
                if (b == null) continue;
                if (string.IsNullOrEmpty(b.id)) continue;
                if (b.lane == null) continue;
                laneMap[b.id] = b.lane;
            }
        }

        private void RebuildCache()
        {
            cachedEvents.Clear();
            cacheBuilt = false;
            eventIndex = 0;
            cycleOffsetBeats = 0;

            if (pattern == null || pattern.cues == null || pattern.cues.Length == 0) return;

            for (int i = 0; i < pattern.cues.Length; i++)
            {
                var c = pattern.cues[i];
                if (c == null) continue;
                if (c.prefab == null) continue;

                double baseBeat = c.beat + c.subBeat;

                int count = Mathf.Max(1, c.count);
                float spacing = Mathf.Max(0f, c.withinCueSpacingBeats);

                for (int k = 0; k < count; k++)
                {
                    cachedEvents.Add(new CachedSpawnEvent
                    {
                        spawnBeat = baseBeat + (spacing * k),
                        laneId = c.laneId,
                        prefab = c.prefab
                    });
                }
            }

            cachedEvents.Sort((a, b) => a.spawnBeat.CompareTo(b.spawnBeat));
            cacheBuilt = true;
        }

        private void Update()
        {
            if (conductor == null) return;
            if (pattern == null) return;

            if (!cacheBuilt) RebuildCache();
            if (!cacheBuilt || cachedEvents.Count == 0) return;

            while (eventIndex < cachedEvents.Count)
            {
                double targetBeat = cycleOffsetBeats + cachedEvents[eventIndex].spawnBeat;
                if (conductor.CurrentBeat < targetBeat) break;

                var ev = cachedEvents[eventIndex];
                SpawnOne(ev.prefab, ev.laneId);

                eventIndex++;

                if (eventIndex >= cachedEvents.Count)
                {
                    if (pattern.loop)
                    {
                        eventIndex = 0;
                        cycleOffsetBeats += pattern.lengthBeats;
                    }
                }
            }
        }

        private void SpawnOne(GameObject prefab, string laneId)
        {
            if (prefab == null) return;

            if (!laneMap.TryGetValue(laneId, out var lane) || lane == null)
            {
                if (logSpawn) Debug.LogWarning($"[RhythmSpawner] laneId 매핑 실패: {laneId}", this);
                return;
            }

            Vector3 pos = lane.transform.position;
            Quaternion rot = lane.transform.rotation;

            Transform parent = spawnedParent != null ? spawnedParent : null;
            var go = Instantiate(prefab, pos, rot, parent);
            activeMonsters.Add(go);

            if (logSpawn) Debug.Log($"[RhythmSpawner] Spawn: beat={conductor.CurrentBeat:F2}, lane={laneId}, prefab={prefab.name}", this);
        }
    }
}