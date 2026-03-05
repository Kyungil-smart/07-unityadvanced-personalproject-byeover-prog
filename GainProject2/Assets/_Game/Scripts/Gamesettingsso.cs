using UnityEngine;

[CreateAssetMenu(menuName = "리듬 도사/게임 설정")]
public sealed class GameSettingsSO : ScriptableObject
{
    [Header("노드 이동")]
    [SerializeField] private float noteStepDistance = 1.2f;
    [SerializeField] private float noteMoveFraction = 0.8f;

    [Header("데미지")]
    [SerializeField] private int missDamage = 1;

    [Header("피니셔")]
    [SerializeField] private int finisherHitCount = 24;
    [SerializeField] private float finisherInterval = 0.05f;
    [SerializeField] private int finisherFinalDamage = 99999;

    [Header("클리어 연출")]
    [SerializeField] private float bossVanishDelay = 0.35f;
    [SerializeField] private float playerAutoRunSpeed = 6f;

    public float NoteStepDistance => noteStepDistance;
    public float NoteMoveFraction => noteMoveFraction;
    public int MissDamage => missDamage;
    public int FinisherHitCount => finisherHitCount;
    public float FinisherInterval => finisherInterval;
    public int FinisherFinalDamage => finisherFinalDamage;
    public float BossVanishDelay => bossVanishDelay;
    public float PlayerAutoRunSpeed => playerAutoRunSpeed;
}