using System.Collections;
using UnityEngine;
using _Game.Scripts.UI;

public sealed class StageManager : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private StageCatalogSO stageCatalog;

    [Header("스폰")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform bossRoot;

    [Header("시스템")]
    [SerializeField] private RhythmConductor conductor;
    [SerializeField] private LightningVfxPool lightningVfxPool;
    [SerializeField] private SimpleBeatSpawner beatSpawner;

    [Header("플레이어 및 UI")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerHeartUI heartUI;
    [SerializeField] private Transform clearLine;
    [SerializeField] private CanvasGroup clearUiGroup;

    [Header("보스 HP바 (상단 고정)")]
    [SerializeField] private BossHpBarUI bossHpBarUI;

    [Header("보스")]
    [SerializeField] private int damagePerNodeSuccess = 5000;
    [SerializeField] private float finisherInterval = 0.05f;

    [Header("음악")]
    [SerializeField] private float musicLeadIn = 3f;

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
        if (beatSpawner == null) beatSpawner = FindFirstObjectByType<SimpleBeatSpawner>();
        if (heartUI == null) heartUI = FindFirstObjectByType<PlayerHeartUI>();

        if (bossHpBarUI == null)
            bossHpBarUI = FindFirstObjectByType<BossHpBarUI>(FindObjectsInactive.Include);
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
        if (heartUI != null) heartUI.RefreshAll();

        SpawnBoss();

        if (lightningVfxPool != null)
            lightningVfxPool.Initialize(gameManager, boss != null ? boss.transform : null);

        if (conductor != null && currentStage.MusicClip != null)
            conductor.SetStageMusic(currentStage.MusicClip, currentStage.Bpm, musicLeadIn);

        if (beatSpawner != null)
        {
            beatSpawner.LoadStage(stageIndex);
            beatSpawner.StartSpawning();
        }

        if (conductor != null) conductor.Play();

        stageRunning = true;

        if (gameManager.Events != null)
        {
            gameManager.Events.RaiseStageStarted(stageIndex);
            gameManager.Events.RaiseGameStateChanged(GameState.StagePlaying);
        }
    }

    private void SpawnBoss()
    {
        if (bossRoot != null)
            for (int i = bossRoot.childCount - 1; i >= 0; i--)
                Destroy(bossRoot.GetChild(i).gameObject);

        boss = null;
        if (currentStage.BossPrefab == null || bossSpawnPoint == null) return;

        BossSpawnContext.IsStageManagerSpawning = true;
        BossSpawnContext.LastReason = "StageManager.SpawnBoss";

        var go = Instantiate(currentStage.BossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, bossRoot);

        BossSpawnContext.IsStageManagerSpawning = false;

        boss = go.GetComponent<BossController>();

        // 보스 HP바 연동 (상단 고정 - 위치 이동 없음)
        if (boss != null && bossHpBarUI != null)
        {
            boss.SetHpBarUI(bossHpBarUI);
            bossHpBarUI.Show();
            bossHpBarUI.SetNormalized(1f);
        }
    }

    // LateUpdate 보스 추적 코드 삭제 — 상단 고정이니까 필요 없음

    private void OnNodeSuccess()
    {
        if (!stageRunning || clearing || boss == null) return;
        boss.ApplyDamage(damagePerNodeSuccess);
    }

    private void OnSongEnded()
    {
        if (beatSpawner != null) beatSpawner.StopSpawning();
        if (stageRunning && !clearing) StartCoroutine(ClearSequence());
    }

    private IEnumerator ClearSequence()
    {
        clearing = true;
        stageRunning = false;

        if (conductor != null) conductor.Stop();

        gameManager.Events.RaiseGameStateChanged(GameState.StageClearing);
        int hitCount = gameManager.Settings != null ? gameManager.Settings.FinisherHitCount : 24;

        for (int i = 0; i < hitCount; i++)
        {
            if (boss != null) boss.LightningHit();
            yield return new WaitForSeconds(finisherInterval);
        }

        if (boss != null) boss.FinishKill();
        yield return new WaitForSeconds(0.35f);

        if (bossHpBarUI != null) bossHpBarUI.Hide();
        if (boss != null) boss.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        if (clearUiGroup != null) clearUiGroup.alpha = 1f;
        if (gameManager.Save != null) gameManager.Save.MarkStageCleared(currentStage.StageIndex);

        gameManager.Events.RaiseGameStateChanged(GameState.StageClear);
    }
}