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
    
    // 문자열로 리듬 차트 만들기
    [SerializeField, Tooltip("패턴 예: 101100 (1=스폰, 0=휴식). 입력하면 우선 적용됩니다.")] 
    private string spawnPattern;

    [Header("프리팹")]
    [SerializeField] private GameObject nodeMonsterPrefab;
    
    // 몬스터 섞어서 스폰 시키기
    [SerializeField, Tooltip("여러 마리를 넣으면 랜덤 스폰 (기존 단일 Prefab 대신 사용)")] 
    private GameObject[] randomMonsterPrefabs;

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
    
    // 새로 추가된 프로퍼티
    public string SpawnPattern => spawnPattern;
    public GameObject[] RandomMonsterPrefabs => randomMonsterPrefabs;
    
    public GameObject NodeMonsterPrefab => nodeMonsterPrefab;
    public GameObject BossPrefab => bossPrefab;
    public StageObstacleType ObstacleType => obstacleType;
}