using UnityEngine;

[CreateAssetMenu(menuName = "리듬 도사/게임 설정")]
public sealed class GameSettingsSO : ScriptableObject
{
    [Header("노드 이동")]
    [SerializeField, Tooltip("한 박자마다 이동할 거리")] private float noteStepDistance = 1.2f;
    [SerializeField, Tooltip("한 박자 중 이동에 쓰는 비율(나머지는 정지)")] private float noteMoveFraction = 0.8f;

    [Header("데미지")]
    [SerializeField, Tooltip("미스 노드가 플레이어에 닿을 때 데미지")] private int missDamage = 1;

    [Header("피니셔")]
    [SerializeField, Tooltip("곡 종료 후 연타 번개 횟수")] private int finisherHitCount = 24;
    [SerializeField, Tooltip("연타 번개 간격(초)")] private float finisherInterval = 0.05f;
    [SerializeField, Tooltip("마지막 데미지(보스 즉사 연출)")] private int finisherFinalDamage = 99999;

    [Header("클리어 연출")]
    [SerializeField, Tooltip("보스 사라짐까지 대기(초)")] private float bossVanishDelay = 0.35f;
    [SerializeField, Tooltip("하율 자동 RUN 이동 속도")] private float playerAutoRunSpeed = 6f;

    public float NoteStepDistance => noteStepDistance;
    public float NoteMoveFraction => noteMoveFraction;
    public int MissDamage => missDamage;
    public int FinisherHitCount => finisherHitCount;
    public float FinisherInterval => finisherInterval;
    public int FinisherFinalDamage => finisherFinalDamage;
    public float BossVanishDelay => bossVanishDelay;
    public float PlayerAutoRunSpeed => playerAutoRunSpeed;
}