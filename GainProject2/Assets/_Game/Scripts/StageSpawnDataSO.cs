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

    [CreateAssetMenu(fileName = "StageSpawnData_", menuName = "RhythmGame/Stage Spawn Data")]
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

        [Header("스폰 패턴 루프 (시퀀서)")]
        [Tooltip("체크한 박자에만 해당 레인에서 몬스터가 나옵니다. (예: 8칸 = 8박자 루프)")]
        public List<BeatSpawnEvent> spawnPattern = new List<BeatSpawnEvent>();
    }
}