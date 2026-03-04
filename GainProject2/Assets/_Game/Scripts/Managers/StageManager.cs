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

    [Header("루트")]
    [SerializeField] private Transform bossRoot;

    [Header("시스템")]
    [SerializeField, Tooltip("음악과 박자를 총괄하는 컨덕터")] 
    private RhythmConductor conductor;
    [SerializeField] private LightningVfxPool lightningVfxPool;

    [Header("플레이어")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAutoRunner playerAutoRunner;
    [SerializeField] private Transform clearLine;

    [Header("UI")]
    [SerializeField] private CanvasGroup clearUiGroup;

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

        if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();

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

        if (playerHealth != null) playerHealth.ResetHp();

        if (playerAutoRunner != null && gameManager.Settings != null)
        {
            playerAutoRunner.StopAutoRun();
            playerAutoRunner.ConfigureSpeed(gameManager.Settings.PlayerAutoRunSpeed);
        }

        SpawnBoss();

        if (lightningVfxPool != null)
            lightningVfxPool.Initialize(gameManager, boss != null ? boss.transform : null);
        
        if (currentStage.MusicClip != null)
            gameManager.Audio.PlayStageMusic(currentStage.MusicClip, currentStage.Bpm, currentStage.FirstBeatOffset, currentStage.ForcedSongLength);
        
        if (conductor != null)
        {
            conductor.Play();
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
        if (!stageRunning || clearing) return;

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
        StartCoroutine(ClearSequence());
    }

    private IEnumerator ClearSequence()
    {
        clearing = true;
        stageRunning = false;

        if (conductor != null) conductor.Stop();

        gameManager.Events.RaiseGameStateChanged(GameState.StageClearing);

        var hitCount = gameManager.Settings != null ? gameManager.Settings.FinisherHitCount : 24;
        var interval = gameManager.Settings != null ? gameManager.Settings.FinisherInterval : 0.05f;

        for (int i = 0; i < hitCount; i++)
        {
            gameManager.Events.RaiseNodeSuccess();
            if (boss != null) boss.LightningHit();

            var end = Time.time + interval;
            while (Time.time < end) yield return null;
        }

        if (boss != null) boss.FinishKill();

        var vanishDelay = gameManager.Settings != null ? gameManager.Settings.BossVanishDelay : 0.35f;
        var vanishEnd = Time.time + vanishDelay;
        while (Time.time < vanishEnd) yield return null;

        if (boss != null) boss.gameObject.SetActive(false);

        if (playerAutoRunner != null) playerAutoRunner.StartAutoRun();

        if (clearLine != null && playerAutoRunner != null)
        {
            while (playerAutoRunner.transform.position.x < clearLine.position.x) yield return null;
        }
        else
        {
            var tEnd = Time.time + 1f;
            while (Time.time < tEnd) yield return null;
        }

        if (clearUiGroup != null)
        {
            clearUiGroup.alpha = 1f;
            clearUiGroup.interactable = true;
            clearUiGroup.blocksRaycasts = true;
        }

        if (gameManager.Save != null) gameManager.Save.MarkStageCleared(currentStage.StageIndex);

        gameManager.Events.RaiseStageCleared(currentStage.StageIndex);
        gameManager.Events.RaiseGameStateChanged(GameState.StageClear);
    }
}