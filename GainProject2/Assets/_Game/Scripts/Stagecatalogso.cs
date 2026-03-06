using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "리듬 도사/스테이지 카탈로그")]
public sealed class StageCatalogSO : ScriptableObject
{
    [SerializeField] private StageData[] stages;

    public int StageCount => stages != null ? stages.Length : 0;

    public bool TryGetStage(int stageIndex, out StageData stage)
    {
        stage = default;
        if (stages == null) return false;

        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].StageIndex == stageIndex)
            {
                stage = stages[i];
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (stages == null || stages.Length == 0) return;
        var used = new HashSet<int>();
        for (int i = 0; i < stages.Length; i++)
        {
            if (!used.Add(stages[i].StageIndex))
                Debug.LogWarning($"[StageCatalogSO] stageIndex 중복: {stages[i].StageIndex}", this);
        }
    }
#endif
}