using System;
using UnityEngine;

[Serializable]
public struct StoryLine
{
    [Header("대화 정보")]
    [Tooltip("화자 이름")]
    public string speakerName;

    [Tooltip("대사 내용")]
    [TextArea(3, 6)]
    public string dialogueText;

    [Tooltip("표시할 일러스트(없으면 이전 유지)")]
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
    [Header("스토리")]
    [Tooltip("순서대로 출력될 대사 목록")]
    public StoryLine[] lines;

    [Header("종료 동작")]
    [Tooltip("스토리 종료 후 어떤 동작을 할지")]
    public StoryExitAction exitAction = StoryExitAction.RequestStage;

    [Tooltip("RequestStage일 때 이동할 스테이지 인덱스(0=튜토리얼)")]
    public int nextStageIndex;

    [Tooltip("LoadScene일 때 로드할 씬 이름")]
    public string nextSceneName;
}