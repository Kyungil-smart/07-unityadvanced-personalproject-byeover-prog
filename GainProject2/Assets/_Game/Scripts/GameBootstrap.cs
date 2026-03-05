using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Monster;

public class GameBootstrap : MonoBehaviour
{
    [Header("매니저")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SessionGameManager sessionManager;
    [SerializeField] private StageManager stageManager;

    [Header("시스템")]
    [SerializeField] private RhythmConductor conductor;

    [Header("시작 스테이지")]
    [SerializeField] private int startStageIndex = 0;

    [Header("튜토리얼")]
    [SerializeField] private bool isTutorialStage = true;

    [Header("SoloUI 연결")]
    [SerializeField] private GameObject soloUIRoot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("버튼")]
    [SerializeField] private Button autoButton;
    [SerializeField] private Button logButton;
    [SerializeField] private Button skipButton;

    [Header("로그 패널")]
    [SerializeField] private GameObject logPanel;
    [SerializeField] private TMP_Text logText;

    [Header("교육 몬스터 프리팹")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject boarPrefab;
    [SerializeField] private GameObject ravenPrefab;
    [SerializeField] private GameObject snakePrefab;

    [Header("스폰 포인트")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform spawnPoint3;
    [SerializeField] private Transform spawnPoint4;

    [Header("교육 설정")]
    [SerializeField] private float practiceSeconds = 10f;

    [Header("대사 설정")]
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private float autoDelay = 1.5f;

    private struct DialogueLine
    {
        public string speaker;
        public string text;
    }

    private DialogueLine[][] tutorialDialogues;
    private bool waitingForInput;
    private bool isAutoMode;
    private bool skipRequested;
    private bool isTyping;
    private readonly List<string> history = new List<string>(64);
    private readonly StringBuilder logBuilder = new StringBuilder(2048);

    private void Awake()
    {
        BuildDialogueData();

        if (autoButton != null) autoButton.onClick.AddListener(ToggleAuto);
        if (logButton != null) logButton.onClick.AddListener(ToggleLog);
        if (skipButton != null) skipButton.onClick.AddListener(OnSkip);

        if (logPanel != null) logPanel.SetActive(false);
    }

    private void BuildDialogueData()
    {
        tutorialDialogues = new[]
        {
            // Phase 1: 유령
            new[]
            {
                new DialogueLine { speaker = "하율", text = "안녕! 나는 하율이야.\n안타깝지만 너한테 직접 말하는건 이번이 처음이자 마지막 일거야~" },
                new DialogueLine { speaker = "하율", text = "어쨌든! 내가 요괴들을 퇴치 하는 법을 보여줄게"},
                new DialogueLine { speaker = "하율", text = "저기 봐, <color=#00FFFF>유령</color>이 다가오고 있어.\n느리지만 꾸준히 오니까 방심하면 안돼." },
                new DialogueLine { speaker = "하율", text = "유령이 부적에 딱 닿는 순간!\n<color=#FFD700>스페이스</color>를 눌러서 퇴치해봐!" },
            },
            // Phase 2: 멧돼지
            new[]
            {
                new DialogueLine { speaker = "하율", text = "잘했어! 이번엔 <color=#FF4444>멧돼지</color>야." },
                new DialogueLine { speaker = "하율", text = "멧돼지는 잠깐 멈췄다가 갑자기 <color=#FF4444>돌진</color>해!\n2박자 마다 확 달려오니까 타이밍에 주의해." },
            },
            // Phase 3: 까마귀
            new[]
            {
                new DialogueLine { speaker = "하율", text = "다음은 <color=#CC66FF>까마귀</color>야." },
                new DialogueLine { speaker = "하율", text = "까마귀는 매 박자마다 빠르게 날아와.\n유령보다 <color=#CC66FF>2배 빠르니까</color> 긴장해야 해!" },
            },
            // Phase 4: 뱀
            new[]
            {
                new DialogueLine { speaker = "하율", text = "마지막! <color=#44FF44>뱀</color>이야." },
                new DialogueLine { speaker = "하율", text = "뱀은 앞으로 갔다가 살짝 뒤로 물러나.\n<color=#44FF44>요동치는 리듬</color>에 집중하면 맞출 수 있어!" },
            },
            // 마무리
            new[]
            {
                new DialogueLine { speaker = "하율", text = "좋아, 식은 죽 먹기지?" },
                new DialogueLine { speaker = "하율", text = "실전은 이것보다 훨씬 어려울거야.\n당황하지 말고 <color=#FFD700>리듬에 집중</color>해!" },
                new DialogueLine { speaker = "하율", text = "준비됐지? 안됐어도 어쩔수 없어 \n그럼... <color=#FFD700>시작!</color>" },
            },
        };
    }

    private void Start()
    {
        if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();

        if (sessionManager != null && gameManager != null)
            sessionManager.Initialize(gameManager);

        if (stageManager != null && gameManager != null)
            stageManager.Initialize(gameManager);

        if (isTutorialStage && startStageIndex == 0)
            StartCoroutine(TutorialSequence());
        else
            stageManager.StartStage(startStageIndex);
    }

    private void Update()
    {
        if (!waitingForInput) return;

        if (logPanel != null && logPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ToggleLog();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            waitingForInput = false;
    }

    // ===================== 튜토리얼 시퀀스 =====================

    private IEnumerator TutorialSequence()
    {
        if (conductor != null)
        {
            conductor.SetStageMusic(null, 130f, 0f);
            conductor.Play();
        }

        ShowUI(true);

        yield return PlayDialoguePhase(0);
        if (skipRequested) { yield return FinishTutorial(); yield break; }
        yield return PracticePhase(ghostPrefab, spawnPoint3);

        yield return PlayDialoguePhase(1);
        if (skipRequested) { yield return FinishTutorial(); yield break; }
        yield return PracticePhase(boarPrefab, spawnPoint2);

        yield return PlayDialoguePhase(2);
        if (skipRequested) { yield return FinishTutorial(); yield break; }
        yield return PracticePhase(ravenPrefab, spawnPoint1);

        yield return PlayDialoguePhase(3);
        if (skipRequested) { yield return FinishTutorial(); yield break; }
        yield return PracticePhase(snakePrefab, spawnPoint4);

        yield return PlayDialoguePhase(4);

        yield return FinishTutorial();
    }

    private IEnumerator FinishTutorial()
    {
        if (conductor != null) conductor.Stop();

        ShowUI(false);
        Time.timeScale = 1f;

        yield return new WaitForSeconds(0.5f);

        if (stageManager != null)
            stageManager.StartStage(startStageIndex);
    }

    // ===================== 대사 재생 =====================

    private IEnumerator PlayDialoguePhase(int phaseIndex)
    {
        if (skipRequested) yield break;
        if (phaseIndex < 0 || phaseIndex >= tutorialDialogues.Length) yield break;

        var lines = tutorialDialogues[phaseIndex];

        for (int i = 0; i < lines.Length; i++)
        {
            if (skipRequested) yield break;
            yield return ShowDialogue(lines[i].speaker, lines[i].text);
        }
    }

    private IEnumerator ShowDialogue(string speaker, string text)
    {
        Time.timeScale = 0f;

        // SoloUI 켜기 (대사 모드)
        ShowUI(true);

        if (nameText != null) nameText.text = speaker;

        AddHistory(speaker, text);

        // 타이핑 연출
        if (dialogueText != null)
        {
            dialogueText.text = text;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();

            int totalChars = dialogueText.textInfo.characterCount;
            float progress = 0f;
            isTyping = true;

            yield return null;

            while (progress < totalChars)
            {
                progress += Time.unscaledDeltaTime * charactersPerSecond;
                dialogueText.maxVisibleCharacters = Mathf.Min(totalChars, Mathf.FloorToInt(progress));

                if (skipRequested) { dialogueText.maxVisibleCharacters = totalChars; break; }
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    dialogueText.maxVisibleCharacters = totalChars;
                    break;
                }

                yield return null;
            }

            dialogueText.maxVisibleCharacters = totalChars;
            isTyping = false;
        }

        if (skipRequested) yield break;

        if (isAutoMode)
        {
            float timer = 0f;
            while (timer < autoDelay)
            {
                timer += Time.unscaledDeltaTime;
                if (skipRequested) yield break;
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) break;
                yield return null;
            }
        }
        else
        {
            yield return null;
            waitingForInput = true;
            while (waitingForInput)
            {
                if (skipRequested) { waitingForInput = false; yield break; }
                yield return null;
            }
        }

        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(0.1f);
    }

    // ===================== 연습 Phase =====================

    private IEnumerator PracticePhase(GameObject prefab, Transform spawnPoint)
    {
        ShowUI(false);

        if (prefab == null || spawnPoint == null)
        {
            yield return new WaitForSeconds(practiceSeconds);
            yield break;
        }

        // 0초: 첫 번째
        if (!skipRequested) SpawnTutorialMonster(prefab, spawnPoint);

        // 5초 대기
        float elapsed = 0f;
        while (elapsed < 5f)
        {
            if (skipRequested) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5초: 두 번째
        if (!skipRequested) SpawnTutorialMonster(prefab, spawnPoint);

        // 나머지 시간 대기
        elapsed = 0f;
        while (elapsed < 5f)
        {
            if (skipRequested) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void SpawnTutorialMonster(GameObject prefab, Transform sp)
    {
        var go = Instantiate(prefab, sp.position, sp.rotation);

        if (go.TryGetComponent(out NodeMonster node))
        {
            var gm = GameManager.Instance;
            node.Initialize(gm, n => Destroy(n.gameObject));
            node.Spawn(sp.position, null);
        }

        var ghost = go.GetComponent<GhostMover>();
        if (ghost != null) ghost.Initialize(conductor);

        var boar = go.GetComponent<BoarMover>();
        if (boar != null) boar.Initialize(conductor);

        var snake = go.GetComponent<SnakeMover>();
        if (snake != null) snake.Initialize(conductor);

        var raven = go.GetComponent<RavenMover>();
        if (raven != null) raven.Initialize(conductor);
    }

    // ===================== 버튼 =====================

    private void ToggleAuto()
    {
        isAutoMode = !isAutoMode;
        if (isAutoMode && waitingForInput && !isTyping)
            waitingForInput = false;
    }

    private void ToggleLog()
    {
        if (logPanel == null) return;
        bool willShow = !logPanel.activeSelf;
        logPanel.SetActive(willShow);
        if (willShow) RefreshLog();
    }

    private void OnSkip()
    {
        skipRequested = true;
        waitingForInput = false;
    }

    // ===================== 로그 =====================

    private void AddHistory(string speaker, string text)
    {
        string clean = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", "");
        history.Add($"[{speaker}] {clean}");
    }

    private void RefreshLog()
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

    // ===================== UI =====================

    private void ShowUI(bool show)
    {
        if (soloUIRoot != null) soloUIRoot.SetActive(show);
    }
}