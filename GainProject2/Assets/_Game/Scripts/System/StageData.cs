using System;
using UnityEngine;
using _Game.Scripts.Rhythm;

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

    [Header("구형 스폰 (사용 안함)")]
    [SerializeField] private int spawnEveryNBeats;
    [SerializeField] private string spawnPattern;

    [Header("정밀 패턴 (SO 방식 - 이걸 사용합니다!)")]
    [SerializeField] private RhythmSpawnPatternSO spawnPatternSO;

    [Header("프리팹")]
    [SerializeField] private GameObject nodeMonsterPrefab;
    [SerializeField] private GameObject[] randomMonsterPrefabs;
    [SerializeField] private GameObject bossPrefab;

    [Header("방해요소")]
    [SerializeField] private StageObstacleType obstacleType;

    [Header("CSV 패턴")]
    [SerializeField] private CsvSpawnPatternSO csvSpawnPatternSO;
    [SerializeField] private MonsterCatalogSO monsterCatalogSO;

    public CsvSpawnPatternSO CsvSpawnPatternSO => csvSpawnPatternSO;
    public MonsterCatalogSO MonsterCatalogSO => monsterCatalogSO;

    public int StageIndex => stageIndex;
    public string StageName => stageName;
    public Sprite NodeSprite => nodeSprite;
    public AudioClip MusicClip => musicClip;
    public float Bpm => bpm;
    public float FirstBeatOffset => firstBeatOffset;
    public float ForcedSongLength => forcedSongLength;
    public int SpawnEveryNBeats => spawnEveryNBeats <= 0 ? 1 : spawnEveryNBeats;
    public string SpawnPattern => spawnPattern;

    public RhythmSpawnPatternSO SpawnPatternSO => spawnPatternSO;

    public GameObject[] RandomMonsterPrefabs => randomMonsterPrefabs;
    public GameObject NodeMonsterPrefab => nodeMonsterPrefab;
    public GameObject BossPrefab => bossPrefab;

    public StageObstacleType ObstacleType => obstacleType;
}