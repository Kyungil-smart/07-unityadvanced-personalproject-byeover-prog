// UTF-8
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
        [Tooltip("예: 130")]
        [SerializeField] private float bpm = 130f;

        [Tooltip("4/4면 4")]
        [SerializeField] private int beatsPerBar = 4;

        [Header("스크롤 속도(연출)")]
        [Tooltip("X=노래 시간(초), Y=스크롤 배율(1=기본). 클라이맥스 구간을 여기서 조절")]
        [SerializeField] private AnimationCurve scrollSpeedCurve = AnimationCurve.Linear(0f, 1f, 180f, 1f);

        [Tooltip("전역 배율(디버그/연출용). 1=기본")]
        [SerializeField] private float scrollSpeedGlobal = 1f;

        public bool IsPlaying
        {
            get
            {
                if (audioSource == null) return false;
                return audioSource.isPlaying;
            }
        }

        public double SongTimeSeconds
        {
            get
            {
                if (audioSource == null) return 0.0;
                return audioSource.time;
            }
        }

        public float Bpm => bpm;
        public int BeatsPerBar => beatsPerBar;

        public float SecPerBeat => 60f / Mathf.Max(1f, bpm);

        public float GetScrollSpeed(double songTimeSec)
        {
            float curve = scrollSpeedCurve != null ? scrollSpeedCurve.Evaluate((float)songTimeSec) : 1f;
            return Mathf.Max(0.01f, curve * scrollSpeedGlobal);
        }

        public void SetScrollSpeedGlobal(float value)
        {
            scrollSpeedGlobal = Mathf.Max(0.01f, value);
        }

        public void Play()
        {
            if (audioSource == null) return;
            audioSource.Play();
        }

        public void Stop()
        {
            if (audioSource == null) return;
            audioSource.Stop();
        }
    }
}