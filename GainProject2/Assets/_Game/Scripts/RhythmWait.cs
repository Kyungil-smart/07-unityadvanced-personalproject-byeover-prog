// UTF-8
using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Rhythm
{
    public sealed class WaitForSongTime : CustomYieldInstruction
    {
        private readonly RhythmConductor _conductor;
        private readonly double _targetTime;

        public WaitForSongTime(RhythmConductor conductor, double targetTimeSeconds)
        {
            _conductor = conductor;
            _targetTime = targetTimeSeconds;
        }

        public override bool keepWaiting
        {
            get
            {
                if (_conductor == null) return false;
                if (!_conductor.IsRunning) return false;
                return _conductor.SongTime < _targetTime;
            }
        }
    }

    public static class RhythmWait
    {
        public static WaitForSongTime Until(RhythmConductor conductor, double absoluteSongTimeSeconds)
            => new WaitForSongTime(conductor, absoluteSongTimeSeconds);

        public static WaitForSongTime After(RhythmConductor conductor, double deltaSeconds)
        {
            double now = conductor != null ? conductor.SongTime : 0.0;
            return new WaitForSongTime(conductor, now + deltaSeconds);
        }
    }
}