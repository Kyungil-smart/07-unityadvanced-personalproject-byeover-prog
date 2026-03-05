using UnityEngine;
using System;

public sealed class RhythmConductor : MonoBehaviour
{
    [Header("리듬 설정")]
    [SerializeField] private float bpm = 130f;
    [SerializeField] private AudioSource musicSource;

    public bool IsRunning { get; private set; }

    public float SongTime
    {
        get
        {
            if (musicSource == null) return 0f;
            return musicSource.time;
        }
    }

    public float CurrentBeat
    {
        get
        {
            return SongTime * (bpm / 60f);
        }
    }

    public event Action<int> OnBeat;

    private int lastBeat = -1;

    private void Update()
    {
        if (!IsRunning) return;

        int beat = Mathf.FloorToInt(CurrentBeat);

        if (beat != lastBeat)
        {
            lastBeat = beat;
            OnBeat?.Invoke(beat);
        }
    }

    public void Play()
    {
        if (musicSource == null) return;

        musicSource.Play();
        IsRunning = true;
        lastBeat = -1;
    }

    public void Stop()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        IsRunning = false;
    }

    public void PauseMusic()
    {
        if (musicSource == null) return;

        musicSource.Pause();
        IsRunning = false;
    }

    public void ResumeMusic()
    {
        if (musicSource == null) return;

        musicSource.UnPause();
        IsRunning = true;
    }
}