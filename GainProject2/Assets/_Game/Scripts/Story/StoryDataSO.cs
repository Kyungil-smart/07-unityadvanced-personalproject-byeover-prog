using System;
using UnityEngine;

[Serializable]
public struct StoryLine
{
    [Header("대화 정보")]
    public string speakerName;
    [TextArea(3, 6)] public string dialogueText;
    public Sprite illustration;
}

public enum StoryExitAction
{
    RequestStage,
    LoadScene
}

[CreateAssetMenu(menuName = "리듬 도사/스토리 데이터")]
public sealed class StoryDataSO : ScriptableObject
{
    public StoryLine[] lines;
    public StoryExitAction exitAction = StoryExitAction.RequestStage;
    public int nextStageIndex;
    public string nextSceneName;
}