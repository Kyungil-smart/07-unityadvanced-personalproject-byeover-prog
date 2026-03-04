using System;
using System.Collections.Generic;
using UnityEngine;

namespace GnalIhu.Rhythm
{
    [CreateAssetMenu(menuName = "리듬 도사/리듬/스폰 패턴", fileName = "SO_RhythmSpawnPattern")]
    public class RhythmSpawnPatternSO : ScriptableObject
    {
        [Header("패턴")]
        [Min(1)]
        [Tooltip("패턴 길이(박). loop=true면 이 길이마다 반복됩니다.")]
        public int lengthBeats = 16;

        [Tooltip("true면 lengthBeats 단위로 반복 스폰합니다. false면 한 번만 실행하고 끝.")]
        public bool loop = true;

        [Header("스폰 큐")]
        [Tooltip("스폰 타이밍은 '초'가 아니라 '박' 기준입니다. beat=0은 0박(시작 박)입니다.")]
        public List<SpawnCue> cues = new List<SpawnCue>();

        [Serializable]
        public struct SpawnCue
        {
            [Min(0)]
            [Tooltip("몇 번째 박에 스폰할지(0부터).")]
            public int beat;

            [Range(0f, 0.999f)]
            [Tooltip("박 내부 오프셋(0=정박, 0.5=반박 등).")]
            public float subBeat;

            [Tooltip("스폰할 레인 ID(Spawner의 Lane Bindings에 있는 ID와 동일해야 함).")]
            public string laneId;

            [Tooltip("스폰할 몹 프리팹.")]
            public GameObject prefab;

            [Min(1)]
            [Tooltip("한 큐에서 스폰할 개수.")]
            public int count;

            [Min(0f)]
            [Tooltip("count>1일 때 추가 스폰 간격(박). 1이면 1박마다 1개씩.")]
            public float withinCueSpacingBeats;
        }

        [ContextMenu("스폰 큐 정렬(beat+subBeat)")]
        public void SortCues()
        {
            cues.Sort((a, b) =>
            {
                float ta = a.beat + a.subBeat;
                float tb = b.beat + b.subBeat;
                return ta.CompareTo(tb);
            });
        }
    }
}