using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class StoryManager : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private StoryDataSO currentStory;

    [Header("UI 참조")]
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Text speakerNameText;
    [SerializeField] private Text dialogueText;
    
    [Header("버튼 참조")]
    [SerializeField] private Button autoButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button logButton;
    [SerializeField] private GameObject logPanel;
    [SerializeField] private Text logText;

    [Header("연출 설정")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float autoDelay = 1.5f;

    private int currentIndex;
    private bool isTyping;
    private bool isAutoMode;
    private Coroutine typingCoroutine;
    private readonly List<string> dialogueHistory = new List<string>();

    private void Start()
    {
        autoButton.onClick.AddListener(ToggleAutoMode);
        skipButton.onClick.AddListener(SkipStory);
        logButton.onClick.AddListener(ToggleLogPanel);
        
        logPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.Story);
        }

        StartStory();
    }

    private void Update()
    {
        if (logPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                CompleteTypingInstantly();
            }
            else
            {
                NextDialogue();
            }
        }
    }

    public void StartStory()
    {
        currentIndex = 0;
        dialogueHistory.Clear();
        logText.text = string.Empty;
        PlayDialogue(currentIndex);
    }

    private void PlayDialogue(int index)
    {
        if (index >= currentStory.dialogues.Length)
        {
            FinishStory();
            return;
        }

        var dialogue = currentStory.dialogues[index];
        speakerNameText.text = dialogue.speakerName;
        
        if (dialogue.illustration != null)
        {
            illustrationImage.sprite = dialogue.illustration;
        }

        dialogueHistory.Add($"[{dialogue.speakerName}] {dialogue.dialogueText}");

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeDialogue(dialogue.dialogueText));
    }

    private IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        if (isAutoMode)
        {
            yield return new WaitForSeconds(autoDelay);
            NextDialogue();
        }
    }

    private void CompleteTypingInstantly()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = currentStory.dialogues[currentIndex].dialogueText;
        isTyping = false;

        if (isAutoMode)
        {
            isAutoMode = false; 
        }
    }

    private void NextDialogue()
    {
        currentIndex++;
        PlayDialogue(currentIndex);
    }

    private void ToggleAutoMode()
    {
        isAutoMode = !isAutoMode;
        if (isAutoMode && !isTyping)
        {
            NextDialogue();
        }
    }

    private void SkipStory()
    {
        FinishStory();
    }

    private void ToggleLogPanel()
    {
        bool willShow = !logPanel.activeSelf;
        logPanel.SetActive(willShow);

        if (willShow)
        {
            logText.text = string.Join("\n\n", dialogueHistory);
        }
    }

    private void FinishStory()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RequestStage(currentStory.nextStageIndex);
        }
    }
}