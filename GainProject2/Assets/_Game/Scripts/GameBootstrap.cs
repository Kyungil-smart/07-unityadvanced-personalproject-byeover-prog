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
        // 1. 세션 매니저 초기화
        if (sessionManager != null && gameManager != null)
        {
            sessionManager.Initialize(gameManager);
        }
        
        // 💡 2. 해결의 핵심: StageManager에게 먼저 GameManager를 쥐여줘야(Initialize) Null 에러가 안 납니다!
        if (stageManager != null && gameManager != null)
        {
            stageManager.Initialize(gameManager); // 먼저 주입!
            stageManager.StartStage(0);           // 그 다음 시작!
        }
        
        // 3. 리듬 시작
        if (rhythmConductor != null && !rhythmConductor.IsRunning)
        {
            rhythmConductor.Play();
        }
    }
}