using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class StoryManager : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField, Tooltip("현재 재생할 스토리 데이터(비어있으면 StoryFlowService에서 가져옴)")]
    private StoryDataSO currentStory;

    [Header("UI 참조")]
    [SerializeField] private Image illustrationImage;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("버튼")]
    [SerializeField] private Button autoButton;
    [SerializeField] private Button logButton;
    [SerializeField] private Button skipButton;

    [Header("로그")]
    [SerializeField] private GameObject logPanel;
    [SerializeField] private TMP_Text logText;

    [Header("연출 설정")]
    [SerializeField, Min(1f), Tooltip("초당 표시할 글자 수")]
    private float charactersPerSecond = 40f;

    [SerializeField, Min(0f), Tooltip("Auto 모드에서 다음 대사로 넘어가기 전 대기 시간")]
    private float autoDelay = 1.0f;

    [Header("입력")]
    [SerializeField, Tooltip("스페이스로 진행할지")]
    private bool useSpaceToAdvance = true;

    [SerializeField, Tooltip("마우스 좌클릭으로 진행할지")]
    private bool useClickToAdvance = true;

    private int currentIndex;
    private bool isTyping;
    private bool isAutoMode;

    private Coroutine typingCoroutine;
    private int currentTotalCharacters;

    private readonly List<string> history = new List<string>(128);
    private readonly StringBuilder logBuilder = new StringBuilder(4096);

    private void Awake()
    {
        if (autoButton != null) autoButton.onClick.AddListener(ToggleAutoMode);
        if (logButton != null) logButton.onClick.AddListener(ToggleLogPanel);
        if (skipButton != null) skipButton.onClick.AddListener(SkipStory);

        if (logPanel != null) logPanel.SetActive(false);

        if (currentStory == null && StoryFlowService.Instance != null && StoryFlowService.Instance.HasPendingStory)
        {
            currentStory = StoryFlowService.Instance.ConsumePendingStory();
        }
    }

    private void Start()
    {
        StartStory();
    }

    private void Update()
    {
        if (logPanel != null && logPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleLogPanel();
            }
            return;
        }

        if (useSpaceToAdvance && Input.GetKeyDown(KeyCode.Space))
        {
            HandleAdvanceInput();
            return;
        }

        if (useClickToAdvance && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HandleAdvanceInput();
            return;
        }
    }

    private void StartStory()
    {
        if (currentStory == null || currentStory.lines == null || currentStory.lines.Length == 0)
        {
            FinishStory();
            return;
        }

        currentIndex = 0;
        history.Clear();
        if (logText != null) logText.text = string.Empty;

        PlayLine(currentIndex);
    }

    private void HandleAdvanceInput()
    {
        if (isTyping)
        {
            CompleteTypingInstantly();
            return;
        }

        NextLine();
    }

    private void PlayLine(int index)
    {
        if (currentStory == null || currentStory.lines == null)
        {
            FinishStory();
            return;
        }

        if (index < 0 || index >= currentStory.lines.Length)
        {
            FinishStory();
            return;
        }

        var line = currentStory.lines[index];

        if (speakerNameText != null) speakerNameText.text = line.speakerName ?? string.Empty;

        if (illustrationImage != null && line.illustration != null)
        {
            illustrationImage.sprite = line.illustration;
            illustrationImage.enabled = true;
        }
        else if (illustrationImage != null && illustrationImage.sprite == null)
        {
            illustrationImage.enabled = false;
        }

        AddHistory(line.speakerName, line.dialogueText);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;

        if (dialogueText == null)
        {
            isTyping = false;
            yield break;
        }

        dialogueText.text = text ?? string.Empty;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        currentTotalCharacters = dialogueText.textInfo.characterCount;
        int visible = 0;
        float progress = 0f;

        while (visible < currentTotalCharacters)
        {
            progress += Time.unscaledDeltaTime * charactersPerSecond;
            visible = Mathf.Min(currentTotalCharacters, Mathf.FloorToInt(progress));
            dialogueText.maxVisibleCharacters = visible;
            yield return null;
        }

        dialogueText.maxVisibleCharacters = currentTotalCharacters;
        isTyping = false;
        typingCoroutine = null;

        if (isAutoMode)
        {
            yield return new WaitForSecondsRealtime(autoDelay);
            if (!isTyping)
            {
                NextLine();
            }
        }
    }

    private void CompleteTypingInstantly()
    {
        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = currentTotalCharacters > 0 ? currentTotalCharacters : int.MaxValue;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    private void NextLine()
    {
        currentIndex++;
        PlayLine(currentIndex);
    }

    private void ToggleAutoMode()
    {
        isAutoMode = !isAutoMode;

        if (isAutoMode && !isTyping)
        {
            StartCoroutine(AutoAdvanceAfterDelay());
        }
    }

    private IEnumerator AutoAdvanceAfterDelay()
    {
        yield return new WaitForSecondsRealtime(autoDelay);
        if (isAutoMode && !isTyping)
        {
            NextLine();
        }
    }

    private void ToggleLogPanel()
    {
        if (logPanel == null) return;

        bool willShow = !logPanel.activeSelf;
        logPanel.SetActive(willShow);

        if (willShow)
        {
            RefreshLogText();
        }
    }

    private void RefreshLogText()
    {
        if (logText == null) return;

        logBuilder.Clear();
        for (int i = 0; i < history.Count; i++)
        {
            logBuilder.Append(history[i]);
            if (i + 1 < history.Count) logBuilder.Append("\n\n");
        }

        logText.text = logBuilder.ToString();
    }

    private void AddHistory(string speaker, string text)
    {
        string s = speaker ?? string.Empty;
        string t = text ?? string.Empty;
        history.Add($"[{s}] {t}");
    }

    private void SkipStory()
    {
        FinishStory();
    }

    private void FinishStory()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        isAutoMode = false;

        if (currentStory == null)
            return;

        if (currentStory.exitAction == StoryExitAction.RequestStage)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RequestStage(currentStory.nextStageIndex);
                return;
            }
        }

        if (currentStory.exitAction == StoryExitAction.LoadScene)
        {
            if (!string.IsNullOrWhiteSpace(currentStory.nextSceneName))
            {
                SceneManager.LoadScene(currentStory.nextSceneName);
                return;
            }
        }
    }
}