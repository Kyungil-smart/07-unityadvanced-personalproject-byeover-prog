using System.Collections;
using UnityEngine;
using _Game.Scripts.Rhythm;
using _Game.Scripts.UI;
using GnalIhu.Rhythm;

public sealed class StageManager : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private StageCatalogSO stageCatalog;

    [Header("스폰 포인트")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform[] laneSpawnPoints;

    [Header("루트")]
    [SerializeField] private Transform bossRoot;
    [SerializeField] private Transform nodeActiveRoot;
    [SerializeField] private Transform nodePoolRoot;

    [Header("시스템")]
    [SerializeField] private CsvStageSpawner csvStageSpawner;
    [SerializeField] private RhythmConductor conductor;
    [SerializeField] private LightningVfxPool lightningVfxPool;

    [Header("플레이어 및 UI")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAutoRunner playerAutoRunner;
    [SerializeField] private Transform clearLine;
    [SerializeField] private CanvasGroup clearUiGroup;

    [Header("보스 UI")]
    [SerializeField, Tooltip("보스 체력바 UI (Canvas WorldSpace 또는 Overlay)")]
    private BossHpBarUI bossHpBarUI;
    [SerializeField, Tooltip("보스 머리 위 오프셋 (월드 좌표)")]
    private Vector3 bossHpBarOffset = new Vector3(0f, 2.5f, 0f);

    [Header("보스")]
    [SerializeField, Tooltip("노드 성공 1회당 보스 데미지")] private int damagePerNodeSuccess = 5000;
    [SerializeField, Tooltip("클리어 연출 중 번개 타격 간격(초)")] private float finisherInterval = 0.05f;

    [Header("디버그")]
    [SerializeField, Tooltip("초기화 후 자동으로 1스테이지 시작(제출용)")] private bool autoStartStage1;

    private static StageManager instance;

    private GameManager gameManager;
    private StageData currentStage;
    private BossController boss;
    private bool stageRunning;
    private bool clearing;

    private bool initialized;
    private bool stageStartRequested;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void Initialize(GameManager manager)
    {
        if (initialized)
            UnbindEvents();

        gameManager = manager;

        if (gameManager != null)
        {
            gameManager.Events.StageRequested += OnStageRequested;
            gameManager.Events.SongEnded += OnSongEnded;
            gameManager.Events.NodeSuccess += OnNodeSuccess;
        }

        if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();

        initialized = true;

        if (autoStartStage1)
            RequestStartStage(1);
    }

    private void OnDestroy()
    {
        UnbindEvents();

        if (instance == this)
            instance = null;
    }

    private void UnbindEvents()
    {
        if (gameManager == null) return;

        gameManager.Events.StageRequested -= OnStageRequested;
        gameManager.Events.SongEnded -= OnSongEnded;
        gameManager.Events.NodeSuccess -= OnNodeSuccess;
    }

    private void OnStageRequested(int stageIndex)
    {
        RequestStartStage(stageIndex);
    }

    private void RequestStartStage(int stageIndex)
    {
        if (!initialized) return;
        if (stageStartRequested) return;

        stageStartRequested = true;
        StartStage(stageIndex);
    }

    public void StartStage(int stageIndex)
    {
        if (!initialized) return;

        if (stageCatalog == null || !stageCatalog.TryGetStage(stageIndex, out currentStage))
        {
            stageStartRequested = false;
            return;
        }

        StopAllCoroutines();

        if (csvStageSpawner != null) csvStageSpawner.StopSpawning();
        if (conductor != null) conductor.Stop();

        stageRunning = false;
        clearing = false;

        if (clearUiGroup != null) clearUiGroup.alpha = 0f;
        if (playerHealth != null) playerHealth.ResetHp();

        if (playerAutoRunner != null && gameManager != null && gameManager.Settings != null)
        {
            playerAutoRunner.StopAutoRun();
            playerAutoRunner.ConfigureSpeed(gameManager.Settings.PlayerAutoRunSpeed);
        }

        SpawnBoss();

        if (lightningVfxPool != null)
            lightningVfxPool.Initialize(gameManager, boss != null ? boss.transform : null);

        if (currentStage.MusicClip != null && gameManager != null && gameManager.Audio != null)
            gameManager.Audio.PlayStageMusic(currentStage.MusicClip, currentStage.Bpm, currentStage.FirstBeatOffset, currentStage.ForcedSongLength);

        bool useCsv = currentStage.CsvSpawnPatternSO != null
                      && currentStage.MonsterCatalogSO != null
                      && csvStageSpawner != null;

        if (!useCsv)
        {
            stageStartRequested = false;
            return;
        }

        csvStageSpawner.SetLaneSpawnPoints(laneSpawnPoints);

        csvStageSpawner.Configure(
            currentStage.CsvSpawnPatternSO,
            currentStage.MonsterCatalogSO,
            currentStage.Bpm,
            1.2f,
            currentStage.ForcedSongLength
        );

        if (conductor != null) conductor.Play();

        stageRunning = true;

        if (gameManager != null)
        {
            gameManager.Events.RaiseStageStarted(stageIndex);
            gameManager.Events.RaiseGameStateChanged(GameState.StagePlaying);
        }
    }

    private void SpawnBoss()
    {
        if (bossRoot != null)
            for (int i = bossRoot.childCount - 1; i >= 0; i--) Destroy(bossRoot.GetChild(i).gameObject);

        boss = null;
        if (currentStage.BossPrefab == null || bossSpawnPoint == null) return;

        using (_Game.Scripts.Rhythm.BossSpawnContext.Enter("StageManager.SpawnBoss"))
        {
            var go = Instantiate(currentStage.BossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, bossRoot);
            boss = go.GetComponent<BossController>();
        }

        if (boss != null && bossHpBarUI != null)
        {
            boss.SetHpBarUI(bossHpBarUI);
            bossHpBarUI.Show();
            bossHpBarUI.SetNormalized(1f);
        }
    }

    private void LateUpdate()
    {
        if (boss != null && bossHpBarUI != null && bossHpBarUI.FollowTarget)
            bossHpBarUI.transform.position = boss.transform.position + bossHpBarOffset;
    }

    private void OnNodeSuccess()
    {
        if (!stageRunning || clearing || boss == null) return;
        boss.ApplyDamage(damagePerNodeSuccess);
    }

    private void OnSongEnded()
    {
        stageStartRequested = false;

        if (csvStageSpawner != null) csvStageSpawner.StopSpawning();

        if (stageRunning && !clearing) StartCoroutine(ClearSequence());
    }

    private IEnumerator ClearSequence()
    {
        clearing = true;
        stageRunning = false;

        if (conductor != null) conductor.Stop();

        if (gameManager != null)
            gameManager.Events.RaiseGameStateChanged(GameState.StageClearing);

        int hitCount = gameManager != null && gameManager.Settings != null ? gameManager.Settings.FinisherHitCount : 24;

        for (int i = 0; i < hitCount; i++)
        {
            if (boss != null) boss.LightningHit();
            yield return new WaitForSeconds(finisherInterval);
        }

        if (boss != null) boss.FinishKill();
        yield return new WaitForSeconds(0.35f);

        if (bossHpBarUI != null) bossHpBarUI.Hide();

        if (boss != null) boss.gameObject.SetActive(false);
        if (playerAutoRunner != null) playerAutoRunner.StartAutoRun();

        if (clearLine != null && playerAutoRunner != null)
            while (playerAutoRunner.transform.position.x < clearLine.position.x) yield return null;
        else
            yield return new WaitForSeconds(1f);

        if (clearUiGroup != null) clearUiGroup.alpha = 1f;
        if (gameManager != null && gameManager.Save != null)
            gameManager.Save.MarkStageCleared(currentStage.StageIndex);

        if (gameManager != null)
            gameManager.Events.RaiseGameStateChanged(GameState.StageClear);
    }
}