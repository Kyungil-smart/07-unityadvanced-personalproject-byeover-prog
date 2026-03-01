using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameEventHub Events { get; private set; }

    [Header("기본 SO")]
    [Tooltip("전역 설정 SO")]
    [SerializeField] private GameSettingsSO settings;

    [Header("매니저")]
    [Tooltip("오디오 매니저(선택)")]
    [SerializeField] private AudioManager audioManager;

    [Tooltip("세이브 매니저(선택)")]
    [SerializeField] private SaveManager saveManager;

    [Tooltip("세션 게임 매니저(선택)")]
    [SerializeField] private SessionGameManager sessionGameManager;

    [Header("디버그")]
    [SerializeField] private bool debugLog = true;

    public GameSettingsSO Settings => settings;
    public AudioManager Audio => audioManager;
    public SaveManager Save => saveManager;
    public SessionGameManager Session => sessionGameManager;

    public GameState CurrentState { get; private set; } = GameState.None;
    public bool IsPaused { get; private set; }

    private GameState stateBeforePause = GameState.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Events ??= new GameEventHub();

        if (audioManager != null) audioManager.Initialize(this);
        if (saveManager != null) saveManager.Initialize(this);
        if (sessionGameManager != null) sessionGameManager.Initialize(this);

        SetState(GameState.Boot);
    }

    public void SetState(GameState state)
    {
        CurrentState = state;
        Events.RaiseGameStateChanged(state);

        if (debugLog)
            Debug.Log($"[GameManager] State: {state}", this);
    }

    public void RequestStage(int stageIndex)
    {
        SetPaused(false);

        Events.RaiseStageRequested(stageIndex);
        SetState(GameState.StageLoading);

        if (debugLog)
            Debug.Log($"[GameManager] RequestStage: {stageIndex}", this);
    }

    public void RestartCurrentStage()
    {
        if (sessionGameManager == null)
        {
            if (debugLog) Debug.Log("[GameManager] RestartCurrentStage: SessionGameManager missing", this);
            return;
        }

        RequestStage(sessionGameManager.CurrentStageIndex);
    }

    public void SetPaused(bool paused)
    {
        if (paused == IsPaused) return;

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (audioManager != null)
            audioManager.SetPaused(paused);

        if (paused)
        {
            stateBeforePause = CurrentState;
            SetState(GameState.Pause);
        }
        else
        {
            var restore = stateBeforePause != GameState.None ? stateBeforePause : GameState.StagePlaying;
            SetState(restore);
            stateBeforePause = GameState.None;
        }

        if (debugLog)
            Debug.Log($"[GameManager] Pause: {paused}", this);
    }

    public void FailStageAndRestart()
    {
        if (sessionGameManager == null)
        {
            if (debugLog) Debug.Log("[GameManager] FailStageAndRestart: SessionGameManager missing", this);
            return;
        }

        if (audioManager != null)
            audioManager.StopStageMusic();

        SetPaused(false);
        SetState(GameState.StageFailed);

        Events.RaiseStageEnded(sessionGameManager.CurrentStageIndex);

        if (debugLog)
            Debug.Log($"[GameManager] StageFailed => Restart: {sessionGameManager.CurrentStageIndex}", this);

        RequestStage(sessionGameManager.CurrentStageIndex);
    }

    public void QuitGame()
    {
        if (debugLog)
            Debug.Log("[GameManager] QuitGame", this);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}