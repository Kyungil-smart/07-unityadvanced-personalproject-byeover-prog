
using System;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [Serializable]
    public sealed class ChartData
    {
        public float[] noteTimes;
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
}