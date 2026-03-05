using System;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [CreateAssetMenu(menuName = "그날이후/리듬/스폰 패턴", fileName = "SO_RhythmSpawnPattern")]
    public sealed class RhythmSpawnPatternSO : ScriptableObject
    {
        [Serializable]
        public sealed class SpawnCue
        {
            [Header("타이밍")]
            public int beat;
            public float subBeat;

            [Header("레인")]
            [Tooltip("0,1,2 또는 Node_00 같은 문자열도 허용")]
            public string laneId;

            [Header("스폰")]
            public GameObject prefab;
            [Min(1)] public int count = 1;
            [Min(0f)] public float withinCueSpacingBeats;
        }

        [Header("패턴")]
        [Min(1)] public int lengthBeats = 16;
        public bool loop = false;

        [Header("큐")]
        public SpawnCue[] cues;

        public int ResolveLaneIndex(string laneIdStr, int laneCount)
        {
            if (laneCount <= 0) return 0;

            int idx = 0;

            if (!string.IsNullOrEmpty(laneIdStr))
            {
                int parsed = -1;

                if (int.TryParse(laneIdStr, out int direct))
                {
                    parsed = direct;
                }
                else
                {
                    int value = 0;
                    bool hasDigit = false;

                    for (int i = 0; i < laneIdStr.Length; i++)
                    {
                        char c = laneIdStr[i];
                        if (c < '0' || c > '9') continue;
                        hasDigit = true;
                        value = (value * 10) + (c - '0');
                    }

                    if (hasDigit) parsed = value;
                }

                if (parsed >= 0) idx = parsed;
            }

            return Mathf.Clamp(idx, 0, laneCount - 1);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (lengthBeats < 1) lengthBeats = 1;

            if (cues == null) return;

            for (int i = 0; i < cues.Length; i++)
            {
                var cue = cues[i];
                if (cue == null) continue;

                if (cue.count < 1) cue.count = 1;
                if (cue.withinCueSpacingBeats < 0f) cue.withinCueSpacingBeats = 0f;
                if (cue.subBeat < 0f) cue.subBeat = 0f;
            }
        }
#endif
    }
}