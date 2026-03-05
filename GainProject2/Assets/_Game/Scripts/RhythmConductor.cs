using System;
using UnityEngine;

namespace GnalIhu.Rhythm
{
    public class RhythmConductor : MonoBehaviour
    {
        [Header("리듬")]
        [SerializeField, Min(1f)]
        [Tooltip("BPM(1분당 박자 수). 예: 120이면 0.5초마다 1박입니다.")]
        private float bpm = 120f;

        [SerializeField, Min(0f)]
        [Tooltip("시작 후 첫 박(0박)이 시작되기 전 대기 시간(초). 인트로/카운트인 용도.")]
        private float leadInSeconds = 0f;

        [SerializeField]
        [Tooltip("true면 Start()에서 자동 재생합니다.")]
        private bool playOnStart = true;

        [Header("오디오(선택)")]
        [SerializeField]
        [Tooltip("리듬에 맞출 음악 AudioSource(선택). 비워도 동작합니다.")]
        private AudioSource musicSource;

        [SerializeField]
        [Tooltip("true면 AudioSettings.dspTime 기반으로 시간 계산(리듬게임 권장). false면 Time.timeAsDouble 사용.")]
        private bool useDspTime = true;

        public event Action OnStarted;
        public event Action<int> OnBeat;

        private bool _running;
        private bool _isPaused;
        private double _startTime;
        private double _pauseStartTime;
        private int _lastBeatIndex = -1;

        public bool IsRunning => _running;

        public double BeatDuration => 60.0 / Math.Max(0.0001, bpm);
        
        public double SongTime
        {
            get
            {
                if (!_running && !_isPaused) return 0;
                
                // 일시정지 중이면 멈췄던 시점의 시간을 반환합니다.
                if (_isPaused) return _pauseStartTime - _startTime; 

                double now = useDspTime ? AudioSettings.dspTime : Time.timeAsDouble;
                return now - _startTime;
            }
        }
        
        public double CurrentBeat => SongTime / BeatDuration;

        private void Start()
        {
            if (playOnStart)
                Play();
        }

        public void SetStageMusic(AudioClip clip, float stageBpm, float offset)
        {
            if (musicSource != null)
            {
                musicSource.clip = clip;
            }
            bpm = stageBpm;
            leadInSeconds = offset;
        }

        public void Play()
        {
            if (_running) return;

            _running = true;
            _isPaused = false;
            _lastBeatIndex = -1;

            double now = useDspTime ? AudioSettings.dspTime : Time.timeAsDouble;
            _startTime = now + leadInSeconds;

            if (musicSource != null)
            {
                musicSource.Stop();

                if (useDspTime)
                {
                    musicSource.PlayScheduled(_startTime);
                }
                else
                {
                    if (leadInSeconds <= 0f) musicSource.Play();
                    else Invoke(nameof(PlayMusicTimeBased), leadInSeconds);
                }
            }

            OnStarted?.Invoke();
        }

        private void PlayMusicTimeBased()
        {
            if (musicSource != null)
                musicSource.Play();
        }

        public void Stop()
        {
            _running = false;
            _isPaused = false;
            _lastBeatIndex = -1;

            if (musicSource != null)
                musicSource.Stop();
        }

        // 튜토리얼 매니저를 위한 일시정지(Pause) 함수
        public void PauseMusic()
        {
            if (!_running || _isPaused) return;

            _isPaused = true;
            _running = false;
            
            // 일시정지를 누른 정확한 시간을 기록합니다.
            _pauseStartTime = useDspTime ? AudioSettings.dspTime : Time.timeAsDouble;

            if (musicSource != null)
                musicSource.Pause();
        }

        // 튜토리얼 매니저를 위한 재생 재개(Resume) 함수
        public void ResumeMusic()
        {
            if (!_isPaused) return;

            _isPaused = false;
            _running = true;

            double now = useDspTime ? AudioSettings.dspTime : Time.timeAsDouble;
            double pausedDuration = now - _pauseStartTime; // 얼마나 오래 멈춰있었는지 계산
            
            _startTime += pausedDuration; 

            if (musicSource != null)
                musicSource.UnPause();
        }

        private void Update()
        {
            if (!_running) return;

            // leadIn 구간에서는 비트 이벤트를 내지 않음
            if (SongTime < 0) return;

            int beatIndex = (int)Math.Floor(CurrentBeat);
            if (beatIndex <= _lastBeatIndex) return;

            // 프레임이 튀어도 누락 없이 비트 이벤트 발생
            for (int b = _lastBeatIndex + 1; b <= beatIndex; b++)
            {
                _lastBeatIndex = b;
                OnBeat?.Invoke(b);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bpm < 1f) bpm = 1f;
        }
#endif
    }
}