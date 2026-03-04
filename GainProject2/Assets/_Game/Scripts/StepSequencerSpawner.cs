using UnityEngine;
using GnalIhu.Rhythm;
using _Game.Scripts.Monster;

namespace _Game.Scripts.System
{
    [DisallowMultipleComponent]
    public sealed class StepSequencerSpawner : MonoBehaviour
    {
        [Header("시스템 연결")]
        [SerializeField] private RhythmConductor conductor;
        [SerializeField] private SequencerStageDataSO stageData;

        [Header("레인 시작점")]
        [SerializeField] private Transform lane1Spawn;
        [SerializeField] private Transform lane2Spawn;
        [SerializeField] private Transform lane3Spawn;
        [SerializeField] private Transform lane4Spawn;

        private int currentStep = -1;

        private void Update()
        {
            if (conductor == null || !conductor.IsRunning || stageData == null) return;

            double currentBeat = conductor.CurrentBeat;
            int stepIndex = Mathf.FloorToInt((float)currentBeat * 2f);

            if (stepIndex > currentStep)
            {
                currentStep = stepIndex;
                ProcessStep(currentStep);
            }
        }

        private void ProcessStep(int stepIndex)
        {
            if (stageData.PatternLength <= 0) return;

            int loopStep = stepIndex % stageData.PatternLength;

            CheckAndSpawn(stageData.Lane1Pattern, loopStep, stageData.Lane1Prefab, lane1Spawn);
            CheckAndSpawn(stageData.Lane2Pattern, loopStep, stageData.Lane2Prefab, lane2Spawn);
            CheckAndSpawn(stageData.Lane3Pattern, loopStep, stageData.Lane3Prefab, lane3Spawn);
            CheckAndSpawn(stageData.Lane4Pattern, loopStep, stageData.Lane4Prefab, lane4Spawn);
        }

        private void CheckAndSpawn(string pattern, int step, GameObject prefab, Transform spawnPoint)
        {
            if (string.IsNullOrEmpty(pattern) || step >= pattern.Length) return;
            
            if (pattern[step] == '1')
            {
                SpawnMonster(prefab, spawnPoint);
            }
        }

        private void SpawnMonster(GameObject prefab, Transform spawnPoint)
        {
            if (prefab == null || spawnPoint == null) return;

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