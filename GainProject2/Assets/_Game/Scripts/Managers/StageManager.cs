using System.Collections;
using UnityEngine;
using _Game.Scripts.Rhythm;

public sealed class StageManager : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField, Tooltip("스테이지 목록 데이터")] private StageCatalogSO stageCatalog;

    [Header("스폰 포인트")]
    [SerializeField, Tooltip("보스 생성 위치")] private Transform bossSpawnPoint;
    [SerializeField, Tooltip("노드 생성 시작 위치")] private Transform nodeSpawnPoint;

    [Header("루트")]
    [SerializeField, Tooltip("생성된 보스의 부모 트랜스폼")] private Transform bossRoot;
    [SerializeField, Tooltip("활성화된 노드들의 부모")] private Transform nodeActiveRoot;
    [SerializeField, Tooltip("비활성 노드 풀의 부모")] private Transform nodePoolRoot;

    [Header("시스템")]
    [SerializeField, Tooltip("노드 스폰 관리자")] private NoteSpawner noteSpawner;
    [SerializeField, Tooltip("번개 효과 풀")] private LightningVfxPool lightningVfxPool;

    [Header("플레이어")]
    [SerializeField, Tooltip("플레이어 체력 컴포넌트")] private PlayerHealth playerHealth;
    [SerializeField, Tooltip("플레이어 자동 이동 컴포넌트")] private PlayerAutoRunner playerAutoRunner;
    [SerializeField, Tooltip("클리어 판정 지점 (X 좌표)")] private Transform clearLine;

    [Header("UI")]
    [SerializeField, Tooltip("클리어 UI 그룹")] private CanvasGroup clearUiGroup;

    private GameManager gameManager;
    private StageData currentStage;
    private BossController boss;
    private bool stageRunning;
    private bool clearing;

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null) return;

        gameManager.Events.StageRequested += OnStageRequested;
        gameManager.Events.SongEnded += OnSongEnded;
        gameManager.Events.NodeSuccess += OnNodeSuccess;

        if (noteSpawner != null) noteSpawner.Initialize(gameManager);

        StartStage(gameManager.Session.CurrentStageIndex);
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;

        gameManager.Events.StageRequested -= OnStageRequested;
        gameManager.Events.SongEnded -= OnSongEnded;
        gameManager.Events.NodeSuccess -= OnNodeSuccess;
    }

    private void OnStageRequested(int stageIndex)
    {
        StartStage(stageIndex);
    }

    public void StartStage(int stageIndex)
    {
        if (stageCatalog == null) return;
        if (!stageCatalog.TryGetStage(stageIndex, out currentStage)) return;
        if (gameManager == null) return;

        StopAllCoroutines();

        stageRunning = false;
        clearing = false;

        if (clearUiGroup != null)
        {
            clearUiGroup.alpha = 0f;
            clearUiGroup.interactable = false;
            clearUiGroup.blocksRaycasts = false;
        }

        if (playerHealth != null)
            playerHealth.ResetHp();

        if (playerAutoRunner != null && gameManager.Settings != null)
        {
            playerAutoRunner.StopAutoRun();
            playerAutoRunner.ConfigureSpeed(gameManager.Settings.PlayerAutoRunSpeed);
        }

        if (noteSpawner != null)
        {
            noteSpawner.StopSpawning();
            noteSpawner.ClearAll();
            noteSpawner.Configure(currentStage, nodeSpawnPoint, nodeActiveRoot, nodePoolRoot);
        }

        SpawnBoss();

        if (lightningVfxPool != null)
            lightningVfxPool.Initialize(gameManager, boss != null ? boss.transform : null);

        if (currentStage.MusicClip != null)
            gameManager.Audio.PlayStageMusic(currentStage.MusicClip, currentStage.Bpm, currentStage.FirstBeatOffset, currentStage.ForcedSongLength);

        if (noteSpawner != null)
        {
            noteSpawner.StartSpawning();
        }

        stageRunning = true;
        gameManager.Events.RaiseStageStarted(stageIndex);
        gameManager.Events.RaiseGameStateChanged(GameState.StagePlaying);
    }

    private void SpawnBoss()
    {
        if (bossRoot != null)
        {
            for (int i = bossRoot.childCount - 1; i >= 0; i--)
                Destroy(bossRoot.GetChild(i).gameObject);
        }

        boss = null;

        if (currentStage.BossPrefab == null || bossSpawnPoint == null) return;

        var go = Instantiate(currentStage.BossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, bossRoot);
        boss = go.GetComponent<BossController>();
    }

    private void Update()
    {
        if (!stageRunning) return;
        if (clearing) return;

        if (boss != null && gameManager != null && gameManager.Audio != null)
        {
            var len = Mathf.Max(0.01f, gameManager.Audio.CurrentSongLength);
            var t = Mathf.Clamp01(gameManager.Audio.CurrentSongTime / len);
            boss.SetSurvivalFill01(1f - t, true);
        }
    }

    private void OnNodeSuccess()
    {
        if (!stageRunning || clearing) return;
        if (boss != null) boss.LightningHit();
    }

    private void OnSongEnded()
    {
        if (!stageRunning || clearing) return;

        // 핵심 추가 1: 노래가 끝났을 때 플레이어가 죽은 상태라면 클리어 시퀀스(번개 연타 등)를 실행하지 않음
        if (playerHealth != null && playerHealth.CurrentHp <= 0) return;

        StartCoroutine(ClearSequence());
    }

    private IEnumerator ClearSequence()
    {
        clearing = true;
        stageRunning = false;

        if (noteSpawner != null)
            noteSpawner.StopSpawning();

        gameManager.Events.RaiseGameStateChanged(GameState.StageClearing);

        var hitCount = gameManager.Settings != null ? gameManager.Settings.FinisherHitCount : 24;
        var interval = gameManager.Settings != null ? gameManager.Settings.FinisherInterval : 0.05f;

        // 번개 연타 시퀀스 (플레이어가 살아있을 때만 진입함)
        for (int i = 0; i < hitCount; i++)
        {
            gameManager.Events.RaiseNodeSuccess();
            if (boss != null) boss.LightningHit();

            yield return new WaitForSeconds(interval);
        }

        if (boss != null)
            boss.FinishKill();

        var vanishDelay = gameManager.Settings != null ? gameManager.Settings.BossVanishDelay : 0.35f;
        yield return new WaitForSeconds(vanishDelay);

        if (boss != null)
            boss.gameObject.SetActive(false);

        // 핵심 추가 2: 달리기 애니메이터 관련 에러가 터져도 코루틴이 멈추지 않도록 try-catch로 방어
        try
        {
            if (playerAutoRunner != null)
                playerAutoRunner.StartAutoRun();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[StageManager] 플레이어 달리기 실행 중 에러 발생 (무시됨): {e.Message}");
        }

        if (clearLine != null && playerAutoRunner != null)
        {
            // 플레이어가 클리어 라인을 넘을 때까지 대기
            while (playerAutoRunner.transform.position.x < clearLine.position.x)
                yield return null;
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        if (clearUiGroup != null)
        {
            clearUiGroup.alpha = 1f;
            clearUiGroup.interactable = true;
            clearUiGroup.blocksRaycasts = true;
        }

        if (gameManager.Save != null)
            gameManager.Save.MarkStageCleared(currentStage.StageIndex);

        gameManager.Events.RaiseStageCleared(currentStage.StageIndex);
        gameManager.Events.RaiseGameStateChanged(GameState.StageClear);
    }
}