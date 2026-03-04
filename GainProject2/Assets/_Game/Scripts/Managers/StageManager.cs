using System.Collections;
using UnityEngine;
using _Game.Scripts.Rhythm;
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
    [SerializeField] private NoteSpawner noteSpawner;
    [SerializeField] private CsvStageSpawner csvStageSpawner;
    [SerializeField] private RhythmConductor conductor;
    [SerializeField] private LightningVfxPool lightningVfxPool;

    [Header("플레이어 및 UI")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAutoRunner playerAutoRunner;
    [SerializeField] private Transform clearLine;
    [SerializeField] private CanvasGroup clearUiGroup;

    private GameManager gameManager;
    private StageData currentStage;
    private BossController boss;
    private bool stageRunning;
    private bool clearing;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;

        gameManager.Events.StageRequested += OnStageRequested;
        gameManager.Events.SongEnded += OnSongEnded;
        gameManager.Events.NodeSuccess += OnNodeSuccess;

        if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;
        gameManager.Events.StageRequested -= OnStageRequested;
        gameManager.Events.SongEnded -= OnSongEnded;
        gameManager.Events.NodeSuccess -= OnNodeSuccess;
    }

    private void OnStageRequested(int stageIndex) => StartStage(stageIndex);

    public void StartStage(int stageIndex)
    {
        if (stageCatalog == null || !stageCatalog.TryGetStage(stageIndex, out currentStage)) return;

        StopAllCoroutines();
        stageRunning = false;
        clearing = false;

        if (clearUiGroup != null) clearUiGroup.alpha = 0f;
        if (playerHealth != null) playerHealth.ResetHp();

        if (playerAutoRunner != null && gameManager.Settings != null)
        {
            playerAutoRunner.StopAutoRun();
            playerAutoRunner.ConfigureSpeed(gameManager.Settings.PlayerAutoRunSpeed);
        }

        SpawnBoss();

        if (lightningVfxPool != null)
            lightningVfxPool.Initialize(gameManager, boss != null ? boss.transform : null);

        if (currentStage.MusicClip != null && gameManager.Audio != null)
            gameManager.Audio.PlayStageMusic(currentStage.MusicClip, currentStage.Bpm, currentStage.FirstBeatOffset, currentStage.ForcedSongLength);

        bool useCsv = currentStage.CsvSpawnPatternSO != null && currentStage.MonsterCatalogSO != null && csvStageSpawner != null;

        if (useCsv)
        {
            csvStageSpawner.Configure(currentStage.CsvSpawnPatternSO, currentStage.MonsterCatalogSO, currentStage.Bpm, 1.2f);

            if (noteSpawner != null)
                noteSpawner.StopSpawning();
        }
        else
        {
            if (noteSpawner != null)
            {
                noteSpawner.Configure(currentStage, laneSpawnPoints, nodeActiveRoot, nodePoolRoot);
                noteSpawner.StartSpawning();
            }
        }

        if (conductor != null) conductor.Play();

        stageRunning = true;
        gameManager.Events.RaiseStageStarted(stageIndex);
        gameManager.Events.RaiseGameStateChanged(GameState.StagePlaying);
    }

    private void SpawnBoss()
    {
        if (bossRoot != null)
            for (int i = bossRoot.childCount - 1; i >= 0; i--) Destroy(bossRoot.GetChild(i).gameObject);

        boss = null;
        if (currentStage.BossPrefab == null || bossSpawnPoint == null) return;
        var go = Instantiate(currentStage.BossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, bossRoot);
        boss = go.GetComponent<BossController>();
    }

    private void Update()
    {
        if (!stageRunning || clearing) return;
        if (boss != null && conductor != null && conductor.IsRunning)
        {
            float len = currentStage.ForcedSongLength > 0 ? currentStage.ForcedSongLength : (currentStage.MusicClip ? currentStage.MusicClip.length : 100f);
            float t = Mathf.Clamp01((float)conductor.SongTime / len);
            boss.SetSurvivalFill01(1f - t, true);
        }
    }

    private void OnNodeSuccess()
    {
        if (stageRunning && !clearing && boss != null) boss.LightningHit();
    }

    private void OnSongEnded()
    {
        if (stageRunning && !clearing) StartCoroutine(ClearSequence());
    }

    private IEnumerator ClearSequence()
    {
        clearing = true;
        stageRunning = false;

        if (conductor != null) conductor.Stop();
        if (noteSpawner != null) noteSpawner.StopSpawning();

        gameManager.Events.RaiseGameStateChanged(GameState.StageClearing);
        var hitCount = gameManager.Settings != null ? gameManager.Settings.FinisherHitCount : 24;

        for (int i = 0; i < hitCount; i++)
        {
            gameManager.Events.RaiseNodeSuccess();
            if (boss != null) boss.LightningHit();
            yield return new WaitForSeconds(0.05f);
        }

        if (boss != null) boss.FinishKill();
        yield return new WaitForSeconds(0.35f);
        if (boss != null) boss.gameObject.SetActive(false);
        if (playerAutoRunner != null) playerAutoRunner.StartAutoRun();

        if (clearLine != null && playerAutoRunner != null)
            while (playerAutoRunner.transform.position.x < clearLine.position.x) yield return null;
        else
            yield return new WaitForSeconds(1f);

        if (clearUiGroup != null) clearUiGroup.alpha = 1f;
        if (gameManager.Save != null) gameManager.Save.MarkStageCleared(currentStage.StageIndex);

        gameManager.Events.RaiseGameStateChanged(GameState.StageClear);
    }
}