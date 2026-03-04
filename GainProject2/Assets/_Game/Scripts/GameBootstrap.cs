using UnityEngine;
using GnalIhu.Rhythm;

public class GameBootstrap : MonoBehaviour
{
    [Header("코어 시스템 연결")]
    [Tooltip("음악과 박자를 관리하는 컨덕터")]
    [SerializeField] private RhythmConductor rhythmConductor;

    [Header("매니저 연결")]
    [Tooltip("전체 게임 상태를 관리하는 게임 매니저")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("인게임 스테이지를 관리하는 세션 매니저")]
    [SerializeField] private SessionGameManager sessionManager;

    [Tooltip("스테이지 데이터를 로드할 스테이지 매니저")]
    [SerializeField] private StageManager stageManager;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (sessionManager != null && gameManager != null)
        {
            sessionManager.Initialize(gameManager);
        }
        
        if (stageManager != null)
        {
            stageManager.StartStage(0); 
        }
        
        if (rhythmConductor != null && !rhythmConductor.IsRunning)
        {
            rhythmConductor.Play();
        }
    }
}