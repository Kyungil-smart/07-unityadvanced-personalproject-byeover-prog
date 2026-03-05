using UnityEngine;
using System;

public sealed class RhythmTimeProvider : MonoBehaviour
{
    [Header("리듬 설정")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float leadInSeconds = 0f;

    private double _startTime;
    private bool _isPlaying;

    public static RhythmTimeProvider Instance { get; private set; }
    public double SongTime => _isPlaying ? AudioSettings.dspTime - _startTime : 0;
    public float BeatDuration => 60f / bpm;
    public double CurrentBeat => SongTime / BeatDuration;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayMusic()
    {
        _startTime = AudioSettings.dspTime + leadInSeconds;
        musicSource.PlayScheduled(_startTime);
        _isPlaying = true;
    }
}