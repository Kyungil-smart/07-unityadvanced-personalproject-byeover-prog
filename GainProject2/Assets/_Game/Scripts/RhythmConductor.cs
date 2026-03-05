using System;
using UnityEngine;

public class RhythmConductor : MonoBehaviour
{
    [Header("리듬")]
    [SerializeField, Min(1f)] private float bpm = 130f;
    [SerializeField, Min(0f)] private float leadInSeconds = 0f;
    [SerializeField] private bool playOnStart = false;

    [Header("오디오")]
    [SerializeField] private AudioSource musicSource;

    public event Action OnStarted;
    public event Action<int> OnBeat;

    private bool _running;
    private bool _isPaused;
    private double _startTime;
    private double _pauseStartTime;
    private int _lastBeatIndex = -1;

    public bool IsRunning => _running;
    public float Bpm { get => bpm; set => bpm = Mathf.Max(1f, value); }
    public double BeatDuration => 60.0 / Math.Max(0.0001, bpm);
    public double SongTime
    {
        get
        {
            if (!_running && !_isPaused) return 0;
            if (_isPaused) return _pauseStartTime - _startTime;
            return AudioSettings.dspTime - _startTime;
        }
    }
    public double CurrentBeat => SongTime / BeatDuration;

    private void Awake()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.playOnAwake = false;
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    public void SetStageMusic(AudioClip clip, float stageBpm, float offset)
    {
        if (musicSource != null) musicSource.clip = clip;
        bpm = Mathf.Max(1f, stageBpm);
        leadInSeconds = Mathf.Max(0f, offset);
    }

    /// <summary>★ 항상 리셋하고 처음부터 시작</summary>
    public void Play()
    {
        // 이미 돌고 있으면 먼저 정지
        if (_running || _isPaused) Stop();

        _running = true;
        _isPaused = false;
        _lastBeatIndex = -1;

        double now = AudioSettings.dspTime;
        _startTime = now + leadInSeconds;

        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Stop();
            musicSource.PlayScheduled(_startTime);
        }

        OnStarted?.Invoke();
    }

    public void Stop()
    {
        _running = false;
        _isPaused = false;
        _lastBeatIndex = -1;
        if (musicSource != null) musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (!_running || _isPaused) return;
        _isPaused = true;
        _running = false;
        _pauseStartTime = AudioSettings.dspTime;
        if (musicSource != null) musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (!_isPaused) return;
        _isPaused = false;
        _running = true;
        double now = AudioSettings.dspTime;
        _startTime += now - _pauseStartTime;
        if (musicSource != null) musicSource.UnPause();
    }

    private void Update()
    {
        if (!_running) return;
        if (SongTime < 0) return;

        int beatIndex = (int)Math.Floor(CurrentBeat);
        if (beatIndex <= _lastBeatIndex) return;

        for (int b = _lastBeatIndex + 1; b <= beatIndex; b++)
        {
            _lastBeatIndex = b;
            OnBeat?.Invoke(b);
        }
    }
}