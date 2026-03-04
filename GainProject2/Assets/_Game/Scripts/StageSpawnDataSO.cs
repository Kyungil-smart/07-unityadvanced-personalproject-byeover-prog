using UnityEngine;
using System.Collections.Generic;
using System;

namespace _Game.Scripts.System
{
    [Serializable]
    public struct BeatSpawnEvent
    {
        public bool lane1;
        public bool lane2;
        public bool lane3;
        public bool lane4;
    }

    [CreateAssetMenu(fileName = "StageSpawnData_", menuName = "리듬 도사/Stage Spawn Data")]
    public sealed class StageSpawnDataSO : ScriptableObject
    {
        [Header("스테이지 레인별 몬스터 할당")]
        [Tooltip("1번 레인 (맨 아래)")]
        public GameObject lane1Prefab;

        [Tooltip("2번 레인")]
        public GameObject lane2Prefab;

        [Tooltip("3번 레인")]
        public GameObject lane3Prefab;

        [Tooltip("4번 레인 (맨 위)")]
        public GameObject lane4Prefab;

        [Header("레인별 랜덤 몬스터 풀")]
        [Tooltip("비어있지 않으면 단일 프리팹 대신 여기서 랜덤으로 뽑아 스폰합니다.")]
        public List<GameObject> lane1RandomPrefabs = new List<GameObject>();

        [Tooltip("비어있지 않으면 단일 프리팹 대신 여기서 랜덤으로 뽑아 스폰합니다.")]
        public List<GameObject> lane2RandomPrefabs = new List<GameObject>();

        [Tooltip("비어있지 않으면 단일 프리팹 대신 여기서 랜덤으로 뽑아 스폰합니다.")]
        public List<GameObject> lane3RandomPrefabs = new List<GameObject>();

        [Tooltip("비어있지 않으면 단일 프리팹 대신 여기서 랜덤으로 뽑아 스폰합니다.")]
        public List<GameObject> lane4RandomPrefabs = new List<GameObject>();

        [Header("스폰 패턴 루프 (시퀀서)")]
        [Tooltip("체크한 박자에만 해당 레인에서 몬스터가 나옵니다. (예: 8칸 = 8박자 루프)")]
        public List<BeatSpawnEvent> spawnPattern = new List<BeatSpawnEvent>();

        public GameObject GetLanePrefab(int laneIndex)
        {
            return laneIndex switch
            {
                1 => Pick(lane1RandomPrefabs, lane1Prefab),
                2 => Pick(lane2RandomPrefabs, lane2Prefab),
                3 => Pick(lane3RandomPrefabs, lane3Prefab),
                4 => Pick(lane4RandomPrefabs, lane4Prefab),
                _ => null
            };
        }

        private static GameObject Pick(List<GameObject> pool, GameObject fallback)
        {
            if (pool != null && pool.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                return pool[idx];
            }
            return fallback;
        }
    }
}