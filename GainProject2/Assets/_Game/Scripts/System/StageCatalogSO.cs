using UnityEngine;

[CreateAssetMenu(menuName = "리듬 도사/스테이지 카탈로그")]
public sealed class StageCatalogSO : ScriptableObject
{
    [Header("스테이지")]
    [SerializeField] private StageData[] stages;

    public bool TryGetStage(int stageIndex, out StageData stageData)
    {
        stageData = default;

        if (stages == null) return false;

        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].StageIndex == stageIndex)
            {
                stageData = stages[i];
                return true;
            }
        }

        if (stageIndex >= 0 && stageIndex < stages.Length)
        {
            stageData = stages[stageIndex];
            return true;
        }

        return false;
    }
}