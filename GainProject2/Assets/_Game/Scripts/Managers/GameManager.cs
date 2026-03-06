using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameEventHub Events { get; private set; }

    [Header("설정 참조")]
    [SerializeField, Tooltip("전역 게임 설정")] private GameSettingsSO settings;
    [SerializeField, Tooltip("오디오 매니저")] private _Game.Scripts.Managers.AudioManager audioManager;
    [SerializeField, Tooltip("세이브 매니저")] private SaveManager saveManager;
    [SerializeField, Tooltip("세션 매니저")] private SessionGameManager sessionGameManager;

    [Header("중복 방지")]
    [SerializeField, Tooltip("중복 인스턴스면 즉시 비활성화 후 제거")] private bool hardBlockDuplicate = true;

    [Header("시스템 상태")]
    [SerializeField, Tooltip("디버그 로그 출력 여부")] private bool debugLog = true;

    public GameSettingsSO Settings => settings;
    public _Game.Scripts.Managers.AudioManager Audio => audioManager;
    public SaveManager Save => saveManager;
    public SessionGameManager Session => sessionGameManager;

    public GameState CurrentState { get; private set; } = GameState.None;
    public bool IsPaused { get; private set; }

    private GameState stateBeforePause = GameState.None;
    private bool initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (hardBlockDuplicate)
                gameObject.SetActive(false);

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (Events == null)
            Events = new GameEventHub();

        if (audioManager == null) audioManager = GetComponentInChildren<_Game.Scripts.Managers.AudioManager>(true);
        if (saveManager == null) saveManager = GetComponentInChildren<SaveManager>(true);
        if (sessionGameManager == null) sessionGameManager = GetComponentInChildren<SessionGameManager>(true);

        InitializeSubSystemsOnce();
        SetState(GameState.Boot);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void InitializeSubSystemsOnce()
    {
        if (initialized) return;
        initialized = true;

        if (audioManager != null) audioManager.Initialize(this);
        if (saveManager != null) saveManager.Initialize(this);
        if (sessionGameManager != null) sessionGameManager.Initialize(this);
    }

    public void SetState(GameState state)
    {
        CurrentState = state;
        Events.RaiseGameStateChanged(state);
        if (debugLog) Debug.Log($"[GameManager] 상태 변경: {state}", this);
    }

    public void RequestStage(int stageIndex)
    {
        SetPaused(false);
        Events.RaiseStageRequested(stageIndex);
        SetState(GameState.StageLoading);
    }

    public void RestartCurrentStage()
    {
        if (sessionGameManager == null) return;
        RequestStage(sessionGameManager.CurrentStageIndex);
    }

    public void SetPaused(bool paused)
    {
        if (paused == IsPaused) return;

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (audioManager != null) audioManager.SetPaused(paused);

        if (paused)
        {
            stateBeforePause = CurrentState;
            SetState(GameState.Pause);
        }
        else
        {
            SetState(stateBeforePause != GameState.None ? stateBeforePause : GameState.StagePlaying);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}