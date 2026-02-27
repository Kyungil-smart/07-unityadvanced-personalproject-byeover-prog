using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    public sealed class WaitForSongTime2D : CustomYieldInstruction
    {
        private readonly RhythmConductor2D _conductor;
        private readonly double _targetTime;

        public WaitForSongTime2D(RhythmConductor2D conductor, double targetTimeSeconds)
        {
            _conductor = conductor;
            _targetTime = targetTimeSeconds;
        }

        public override bool keepWaiting
        {
            get
            {
                if (_conductor == null) return false;
                if (!_conductor.IsPlaying) return false;
                return _conductor.SongTimeSeconds < _targetTime;
            }
        }
    }

    public static class RhythmWait2D
    {
        public static WaitForSongTime2D Until(RhythmConductor2D conductor, double absoluteSongTimeSeconds)
            => new WaitForSongTime2D(conductor, absoluteSongTimeSeconds);

        public static WaitForSongTime2D After(RhythmConductor2D conductor, double deltaSeconds)
        {
            double now = conductor != null ? conductor.SongTimeSeconds : 0.0;
            return new WaitForSongTime2D(conductor, now + deltaSeconds);
        }
    }
}