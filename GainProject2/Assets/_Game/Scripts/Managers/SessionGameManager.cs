using UnityEngine;

public sealed class SessionGameManager : MonoBehaviour
{
    [Header("세션")]
    // 기존 1을 0(튜토리얼)으로 변경, 최소값을 0으로 설정
    [SerializeField, Tooltip("현재 스테이지 인덱스(0=튜토리얼, 1~8)")] 
    private int currentStageIndex = 0; 

    private GameManager gameManager;

    public int CurrentStageIndex => currentStageIndex;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        // Clamp의 최소값을 1에서 0으로 수정
        currentStageIndex = Mathf.Clamp(currentStageIndex, 0, 8); 
    }

    public void SetStageIndex(int stageIndex)
    {
        currentStageIndex = Mathf.Clamp(stageIndex, 0, 8);
        gameManager.Events.RaiseStageRequested(currentStageIndex);
    }

    public void AdvanceToNextStage()
    {
        SetStageIndex(Mathf.Clamp(currentStageIndex + 1, 0, 8));
    }
}