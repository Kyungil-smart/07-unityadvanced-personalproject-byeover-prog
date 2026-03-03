using UnityEngine;
using GnalIhu.Rhythm;           // RhythmConductor를 찾기 위한 네임스페이스
using _Game.Scripts.Rhythm;     // ChartPlayer를 찾기 위한 네임스페이스

public class GameBootstrap : MonoBehaviour
{
    [Header("코어 시스템 연결")]
    [Tooltip("음악과 박자를 관리하는 컨덕터")]
    [SerializeField] private RhythmConductor rhythmConductor;

    [Tooltip("차트 데이터를 읽어오는 플레이어")]
    [SerializeField] private ChartPlayer chartPlayer;

    [Header("매니저 연결")]
    [Tooltip("전체 게임 상태를 관리하는 게임 매니저")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("인게임 스테이지를 관리하는 세션 매니저")]
    [SerializeField] private SessionGameManager sessionManager;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // 1. 세션 매니저 초기화 (GameManager 주입)
        if (sessionManager != null && gameManager != null)
        {
            sessionManager.Initialize(gameManager);
        }

        // 2. ChartPlayer는 내부 Awake에서 스스로 차트를 로드하므로 별도 호출 생략

        // 3. 음악 및 리듬 시스템 시작
        if (rhythmConductor != null && !rhythmConductor.IsRunning)
        {
            rhythmConductor.Play();
        }
    }
}