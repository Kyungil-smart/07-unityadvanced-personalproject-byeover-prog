using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("효과음 클립")]
    [SerializeField] private AudioClip beatTickClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip missClip;

    [Header("효과음 설정")]
    [SerializeField, Tooltip("매 박자마다 비트 효과음을 재생할지 여부")] 
    private bool playBeatTick = true;

    [Header("정밀 재생")]
    [SerializeField, Tooltip("DSP 예약 재생 지연(초)")] private double dspStartDelay = 0.1;

    private GameManager gameManager;
    private bool scheduled;
    private double dspStartTime;
    private float bpm;
    private float firstBeatOffset;
    private double nextBeatDspTime;
    private int beatIndex;
    private double beatDuration;
    private double songLength;
    private bool songEndedRaised;

    public float CurrentSongTime => scheduled ? Mathf.Max(0f, (float)(AudioSettings.dspTime - dspStartTime)) : 0f;
    public float CurrentSongLength => (float)songLength;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;

        gameManager.Events.Beat += OnBeat;
        gameManager.Events.NodeSuccess += OnNodeSuccess;
        gameManager.Events.NodeMiss += OnNodeMiss;
    }

    private void OnDestroy()
    {
        if (gameManager != null && gameManager.Events != null)
        {
            gameManager.Events.Beat -= OnBeat;
            gameManager.Events.NodeSuccess -= OnNodeSuccess;
            gameManager.Events.NodeMiss -= OnNodeMiss;
        }
    }

    public void PlayStageMusic(AudioClip clip, float stageBpm, float stageFirstBeatOffset, float forcedSongLengthSeconds)
    {
        if (musicSource == null || clip == null) return;

        musicSource.Stop();
        musicSource.clip = clip;

        bpm = Mathf.Max(1f, stageBpm);
        firstBeatOffset = Mathf.Max(0f, stageFirstBeatOffset);

        beatDuration = 60.0 / bpm;

        dspStartTime = AudioSettings.dspTime + dspStartDelay;
        musicSource.PlayScheduled(dspStartTime);

        scheduled = true;
        songEndedRaised = false;

        beatIndex = 0;
        nextBeatDspTime = dspStartTime + firstBeatOffset;

        var clipLen = (double)clip.length;
        songLength = forcedSongLengthSeconds > 0f ? forcedSongLengthSeconds : clipLen;
    }

    public void StopStageMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        scheduled = false;
        songEndedRaised = true;
    }

    public void SetPaused(bool paused)
    {
        if (musicSource == null) return;

        if (paused) musicSource.Pause();
        else musicSource.UnPause();
    }

    private void Update()
    {
        if (!scheduled) return;

        var now = AudioSettings.dspTime;

        if (!songEndedRaised)
        {
            var songDspTime = now - dspStartTime;
            if (songDspTime >= songLength)
            {
                songEndedRaised = true;
                gameManager.Events.RaiseSongEnded();
            }
        }

        if (now < nextBeatDspTime) return;

        var beatDur = (float)beatDuration;

        while (now >= nextBeatDspTime)
        {
            var info = new BeatInfo(beatIndex, nextBeatDspTime, beatDur);
            gameManager.Events.RaiseBeat(info);

            beatIndex++;
            nextBeatDspTime = dspStartTime + firstBeatOffset + (beatIndex * beatDuration);

            if (beatIndex > 200000) break;
        }
    }

    private void OnBeat(BeatInfo info)
    {
        if (!playBeatTick) return;
        if (beatTickClip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(beatTickClip);
        }
    }

    private void OnNodeSuccess()
    {
        if (hitClip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hitClip);
        }
    }

    private void OnNodeMiss()
    {
        if (missClip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(missClip);
        }
    }
}