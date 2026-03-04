using UnityEngine;

public sealed class StageManager : MonoBehaviour
{
    [Header("스테이지 데이터")]
    [SerializeField] private StageCatalogSO catalog;
    [SerializeField] private AudioSource musicSource;

    private GameManager gameManager;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void LoadStage(int stageIndex)
    {
        if (catalog == null || musicSource == null) return;

        if (!catalog.TryGetStage(stageIndex, out StageData stageData))
        {
            Debug.LogError($"[StageManager] 스테이지 데이터를 찾을 수 없습니다: {stageIndex}");
            return;
        }

        // 수정된 부분: private인 musicClip(소문자) 대신 public 프로퍼티인 MusicClip(대문자)을 호출합니다.
        musicSource.clip = stageData.MusicClip;
        
        if (gameManager != null && gameManager.Events != null)
        {
            gameManager.Events.RaiseStageStarted(stageIndex);
        }
    }
}