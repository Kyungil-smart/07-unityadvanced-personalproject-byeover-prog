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
    [SerializeField, Tooltip("BPM")] private float bpm;
    [SerializeField, Tooltip("첫 박자 오프셋(초)")] private float firstBeatOffset;
    [SerializeField, Tooltip("곡 길이(초) 강제값, 0이면 클립 길이 사용")] private float forcedSongLength;

    [Header("노드 스폰")]
    [SerializeField, Tooltip("몇 박자마다 1개 스폰(1이면 매 박자)")] private int spawnEveryNBeats;

    [Header("프리팹")]
    [SerializeField] private GameObject nodeMonsterPrefab;
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
    public int SpawnEveryNBeats => spawnEveryNBeats <= 0 ? 1 : spawnEveryNBeats;
    public GameObject NodeMonsterPrefab => nodeMonsterPrefab;
    public GameObject BossPrefab => bossPrefab;
    public StageObstacleType ObstacleType => obstacleType;
}