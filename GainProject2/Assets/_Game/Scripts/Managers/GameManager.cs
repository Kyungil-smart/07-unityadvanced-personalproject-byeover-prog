using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameEventHub Events { get; private set; }

    [Header("기본 SO")]
    [SerializeField] private GameSettingsSO settings;

    [Header("매니저")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SaveManager saveManager;
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

        // 최적화: 부모가 있다면 해제하여 DontDestroyOnLoad 에러 방지
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
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
        if (debugLog) Debug.Log($"[GameManager] State: {state}");
    }

    public void RequestStage(int stageIndex)
    {
        SetPaused(false);
        Events.RaiseStageRequested(stageIndex);
        SetState(GameState.StageLoading);
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

    public void FailStageAndRestart()
    {
        if (sessionGameManager == null) return;
        if (audioManager != null) audioManager.StopStageMusic();
        SetPaused(false);
        SetState(GameState.StageFailed);
        Events.RaiseStageEnded(sessionGameManager.CurrentStageIndex);
        RequestStage(sessionGameManager.CurrentStageIndex);
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