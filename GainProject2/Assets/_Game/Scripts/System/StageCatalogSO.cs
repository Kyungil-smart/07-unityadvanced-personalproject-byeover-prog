using System.Collections.Generic;
using UnityEngine;

public sealed class StageCatalogSO : ScriptableObject
{
    [SerializeField] private StageData[] stages;

    public bool TryGetStage(int stageIndex, out StageData stage)
    {
        stage = default;
        if (stages == null || stages.Length == 0) return false;

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
            var s = stages[i];

            if (!used.Add(s.StageIndex))
                Debug.LogWarning($"[StageCatalogSO] stageIndex 중복: {s.StageIndex} (중복 스테이지 존재)", this);

            bool hasCsv = s.CsvSpawnPatternSO != null && s.MonsterCatalogSO != null;
            bool hasPattern = s.SpawnPatternSO != null;

            if (hasCsv && hasPattern)
                Debug.LogWarning($"[StageCatalogSO] 스폰 소스가 2개입니다. stageIndex={s.StageIndex}. 한쪽을 None으로 비우세요.", this);
        }
    }
#endif
}