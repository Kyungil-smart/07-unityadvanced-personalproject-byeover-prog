using UnityEngine;
using GnalIhu.Rhythm;
using _Game.Scripts.Monster;

namespace _Game.Scripts.System
{
    [DisallowMultipleComponent]
    public sealed class FourLaneSpawner : MonoBehaviour
    {
        [Header("시스템 연결")]
        [SerializeField] private RhythmConductor conductor;
        [SerializeField] private StageSpawnDataSO currentStageData;

        [Header("레인 시작점 (Spawn Points)")]
        [SerializeField] private Transform lane1Spawn;
        [SerializeField] private Transform lane2Spawn;
        [SerializeField] private Transform lane3Spawn;
        [SerializeField] private Transform lane4Spawn;

        [Header("디버그")]
        [SerializeField, Tooltip("연결 누락 경고 로그")] private bool logWarnings = true;

        private void OnEnable()
        {
            if (conductor != null) conductor.OnBeat += HandleBeat;
        }

        private void OnDisable()
        {
            if (conductor != null) conductor.OnBeat -= HandleBeat;
        }

        private void HandleBeat(int beatIndex)
        {
            if (currentStageData == null || beatIndex < 0) return;
            if (currentStageData.spawnPattern == null || currentStageData.spawnPattern.Count == 0) return;

            int patternIndex = beatIndex % currentStageData.spawnPattern.Count;
            var currentBeatData = currentStageData.spawnPattern[patternIndex];

            if (currentBeatData.lane1) SpawnLane(1, lane1Spawn);
            if (currentBeatData.lane2) SpawnLane(2, lane2Spawn);
            if (currentBeatData.lane3) SpawnLane(3, lane3Spawn);
            if (currentBeatData.lane4) SpawnLane(4, lane4Spawn);
        }

        private void SpawnLane(int laneIndex, Transform spawnPoint)
        {
            if (spawnPoint == null)
            {
                if (logWarnings) Debug.LogWarning($"[FourLaneSpawner] SpawnPoint 누락: Lane {laneIndex}", this);
                return;
            }

            var prefab = currentStageData != null ? currentStageData.GetLanePrefab(laneIndex) : null;
            if (prefab == null)
            {
                if (logWarnings) Debug.LogWarning($"[FourLaneSpawner] Prefab 누락: Lane {laneIndex} (StageSpawnDataSO 확인)", this);
                return;
            }

            GameObject monsterObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            var ghost = monsterObj.GetComponent<GhostMover>();
            if (ghost != null) ghost.Initialize(conductor);

            var boar = monsterObj.GetComponent<BoarMover>();
            if (boar != null) boar.Initialize(conductor);

            var snake = monsterObj.GetComponent<SnakeMover>();
            if (snake != null) snake.Initialize(conductor);

            var raven = monsterObj.GetComponent<RavenMover>();
            if (raven != null) raven.Initialize(conductor);
        }
    }
}