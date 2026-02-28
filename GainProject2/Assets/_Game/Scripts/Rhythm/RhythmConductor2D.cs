using System;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class RhythmConductor2D : MonoBehaviour
    {
        [Header("오디오")]
        [Tooltip("재생할 AudioSource")]
        [SerializeField] private AudioSource audioSource;

        [Header("박자")]
        [Tooltip("BPM (예: 130)")]
        [SerializeField] private float bpm = 130f;

        [Tooltip("4/4면 4")]
        [SerializeField] private int beatsPerBar = 4;

        [Header("오프셋")]
        [Tooltip("음악 파일 앞 무음 구간(초). 첫 비트가 시작되는 시점")]
        [SerializeField] private float firstBeatOffsetSec = 0f;

        [Header("스크롤 속도(연출)")]
        [SerializeField] private AnimationCurve scrollSpeedCurve = AnimationCurve.Linear(0f, 1f, 180f, 1f);
        [SerializeField] private float scrollSpeedGlobal = 1f;
        
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public float Bpm => bpm;
        public int BeatsPerBar => beatsPerBar;
        public float SecPerBeat => 60f / Mathf.Max(1f, bpm);
        public float FirstBeatOffset => firstBeatOffsetSec;

        // >DSP 기반 정밀 곡 시간(초)
        public double SongTimeSeconds { get; private set; }

        // firstBeatOffset 기준 비트 시간. 비트 0 = 첫 다운비트
        public double SongTimeBeat => (SongTimeSeconds - firstBeatOffsetSec) / SecPerBeat;

        // 현재 비트 번호 (정수, floor)
        public int CurrentBeatIndex => Mathf.FloorToInt((float)SongTimeBeat);

        // 현재 비트 내 진행률 (0~1). 홉 애니메이션에 활용
        public float BeatProgress
        {
            get
            {
                double bt = SongTimeBeat;
                return Mathf.Clamp01((float)(bt - System.Math.Floor(bt)));
            }
        }

        // 새 비트가 시작될 때 발생. int = 비트 인덱스
        public event Action<int> OnBeat;
        
        private double _dspStartTime;
        private int _lastBeatFired = -1;
        
        public float GetScrollSpeed(double songTimeSec)
        {
            float curve = scrollSpeedCurve != null ? scrollSpeedCurve.Evaluate((float)songTimeSec) : 1f;
            return Mathf.Max(0.01f, curve * scrollSpeedGlobal);
        }

        public void SetScrollSpeedGlobal(float value)
        {
            scrollSpeedGlobal = Mathf.Max(0.01f, value);
        }

        // 특정 비트 인덱스의 절대 곡 시간(초)을 반환
        public double BeatToSeconds(int beatIndex)
        {
            return firstBeatOffsetSec + beatIndex * (double)SecPerBeat;
        }

        // 특정 곡 시간(초)이 몇 번째 비트인지 반환
        public double SecondsToBeat(double songSec)
        {
            return (songSec - firstBeatOffsetSec) / SecPerBeat;
        }

        public void Play()
        {
            if (audioSource == null) return;
            _dspStartTime = AudioSettings.dspTime;
            _lastBeatFired = -1;
            audioSource.Play();
        }

        public void Stop()
        {
            if (audioSource == null) return;
            audioSource.Stop();
        }

        private void Update()
        {
            if (!IsPlaying) return;

            // DSP 기반 정밀 시간 계산
            SongTimeSeconds = AudioSettings.dspTime - _dspStartTime;

            // 비트 이벤트 발생
            int beat = CurrentBeatIndex;
            if (beat > _lastBeatFired && SongTimeSeconds >= firstBeatOffsetSec)
            {
                _lastBeatFired = beat;
                OnBeat?.Invoke(beat);
            }
        }
    }
}
