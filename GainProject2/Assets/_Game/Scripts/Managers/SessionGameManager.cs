using UnityEngine;

public sealed class SessionGameManager : MonoBehaviour
{
    [Header("세션")]
    [SerializeField, Tooltip("현재 스테이지 인덱스(1~8)")] private int currentStageIndex = 1;

    private GameManager gameManager;

    public int CurrentStageIndex => currentStageIndex;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        currentStageIndex = Mathf.Clamp(currentStageIndex, 1, 8);
    }

    public void SetStageIndex(int stageIndex)
    {
        currentStageIndex = Mathf.Clamp(stageIndex, 1, 8);
        gameManager.Events.RaiseStageRequested(currentStageIndex);
    }

    public void AdvanceToNextStage()
    {
        SetStageIndex(Mathf.Clamp(currentStageIndex + 1, 1, 8));
    }
}