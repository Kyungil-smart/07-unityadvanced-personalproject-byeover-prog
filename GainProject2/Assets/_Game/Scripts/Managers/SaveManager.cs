using System.IO;
using UnityEngine;

public sealed class SaveManager : MonoBehaviour
{
    [Header("세이브")]
    [SerializeField, Tooltip("세이브 파일명")] private string saveFileName = "save.json";

    private SaveData saveData;
    private string savePath;

    public int ClearedStageIndex => saveData != null ? saveData.ClearedStageIndex : 0;

    public void Initialize(GameManager manager)
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        LoadOrCreate();
    }

    public void LoadOrCreate()
    {
        if (!JsonManager.TryLoad(savePath, out saveData))
        {
            saveData = new SaveData { ClearedStageIndex = 0 };
            JsonManager.TrySave(savePath, saveData);
        }
    }

    public void MarkStageCleared(int stageIndex)
    {
        if (saveData == null) return;

        if (stageIndex > saveData.ClearedStageIndex)
            saveData.ClearedStageIndex = stageIndex;

        JsonManager.TrySave(savePath, saveData);
    }
}