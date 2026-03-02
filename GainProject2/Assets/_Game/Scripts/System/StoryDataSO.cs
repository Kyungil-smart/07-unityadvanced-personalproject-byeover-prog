using UnityEngine;
using System;

[Serializable]
public struct DialogueData
{
    [Header("화자 이름")]
    public string speakerName;
    
    [Header("대사 내용")]
    [TextArea(3, 5)]
    public string dialogueText;
    
    [Header("일러스트")]
    public Sprite illustration;
}

[CreateAssetMenu(menuName = "리듬게임/스토리 데이터")]
public sealed class StoryDataSO : ScriptableObject
{
    [Header("대사 리스트")]
    public DialogueData[] dialogues;

    [Header("스토리 종료 후 이동할 스테이지")]
    public int nextStageIndex;
}