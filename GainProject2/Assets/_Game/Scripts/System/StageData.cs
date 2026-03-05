using System;
using UnityEngine;

[Serializable]
public struct StageData
{
    [Header("기본")]
    [SerializeField] private int stageIndex;
    [SerializeField] private string stageName;

    [Header("비주얼")]
    [SerializeField] private Sprite nodeSprite;

    [Header("오디오")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float bpm;
    [SerializeField] private float firstBeatOffset;
    [SerializeField] private float forcedSongLength;

    [Header("프리팹")]
    [SerializeField] private GameObject bossPrefab;

    [Header("방해요소")]
    [SerializeField] private StageObstacleType obstacleType;

    public int StageIndex => stageIndex;
    public string StageName => stageName;
    public Sprite NodeSprite => nodeSprite;
    public AudioClip MusicClip => musicClip;
    public float Bpm => bpm;
    public float FirstBeatOffset => firstBeatOffset;
    public float ForcedSongLength => forcedSongLength;
    public GameObject BossPrefab => bossPrefab;
    public StageObstacleType ObstacleType => obstacleType;
}