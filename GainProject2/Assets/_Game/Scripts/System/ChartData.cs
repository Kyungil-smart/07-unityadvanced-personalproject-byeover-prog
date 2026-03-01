using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [Serializable]
    public sealed class ChartData
    {
        public float[] noteTimes;   // 각 노트의 히트 타이밍 (초 단위)
        public float[] eliteTimes;
        public float songLength;
    }

    public static class ChartDataLoader
    {
        public static bool TryLoad(TextAsset json, out ChartData data)
        {
            data = null;
            if (json == null || string.IsNullOrWhiteSpace(json.text)) return false;

            try
            {
                data = JsonUtility.FromJson<ChartData>(json.text);
                return data != null && data.noteTimes != null;
            }
            catch
            {
                data = null;
                return false;
            }
        }
    }

    public static class ChartGenerator
    {
        // 매 비트(4분음표)마다 노트를 생성
        // bpm>BPM
        // firstBeatOffset 첫 비트 오프셋(초)
        // <totalBeats 총 비트 수
        
        public static string GenerateEveryBeat(float bpm, float firstBeatOffset, int totalBeats)
        {
            float secPerBeat = 60f / bpm;
            var times = new List<float>();

            for (int i = 0; i < totalBeats; i++)
            {
                times.Add(firstBeatOffset + i * secPerBeat);
            }

            var chart = new ChartData
            {
                noteTimes = times.ToArray(),
                eliteTimes = new float[0],
                songLength = firstBeatOffset + totalBeats * secPerBeat
            };

            return JsonUtility.ToJson(chart, true);
        }

        // 4/4 박자에서 1, 3번째 비트(강박)에만 노트 생성
        
        public static string GenerateOnDownbeats(float bpm, float firstBeatOffset, int totalBars)
        {
            float secPerBeat = 60f / bpm;
            var times = new List<float>();

            for (int bar = 0; bar < totalBars; bar++)
            {
                // 1번째 비트 (강박)
                times.Add(firstBeatOffset + (bar * 4) * secPerBeat);
                // 3번째 비트
                times.Add(firstBeatOffset + (bar * 4 + 2) * secPerBeat);
            }

            var chart = new ChartData
            {
                noteTimes = times.ToArray(),
                eliteTimes = new float[0],
                songLength = firstBeatOffset + totalBars * 4 * secPerBeat
            };

            return JsonUtility.ToJson(chart, true);
        }
    }
}
