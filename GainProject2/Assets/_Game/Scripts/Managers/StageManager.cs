using System.Collections;
using UnityEngine;
using _Game.Scripts.UI;
using _Game.Scripts.Rhythm;

public sealed class StageManager : MonoBehaviour
{
    [SerializeField] private StageCatalogSO stageCatalog;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform bossRoot;
    [SerializeField] private RhythmConductor conductor;
    [SerializeField] private LightningVfxPool lightningVfxPool;
    [SerializeField] private SimpleBeatSpawner beatSpawner;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerHeartUI heartUI;
    [SerializeField] private PlayerAutoRunner playerAutoRunner;
    [SerializeField] private BossHpBarUI bossHpBarUI;
    [SerializeField] private CanvasGroup clearUiGroup;
    [SerializeField] private Camera mainCamera;

    [SerializeField] private int baseDamage = 100;
    [SerializeField] private int comboScaling = 10;
    [SerializeField] private float musicLeadIn = 3f;
    [SerializeField] private int finisherHitCount = 24;
    [SerializeField] private float finisherInterval = 0.05f;
    [SerializeField] private float bossVanishDelay = 0.35f;
    [SerializeField] private float runOutSpeed = 8f;

    private GameManager gameManager;
    private StageData currentStage;
    private BossController boss;
    private bool stageRunning;
    private bool clearing;
    private int combo;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        gameManager.Events.StageRequested += OnStageRequested;
        gameManager.Events.SongEnded += OnSongEnded;
        gameManager.Events.NodeSuccess += OnNodeSuccess;
        gameManager.Events.NodeMiss += OnNodeMiss;
        if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();
        if (beatSpawner == null) beatSpawner = FindFirstObjectByType<SimpleBeatSpawner>();
        if (heartUI == null) heartUI = FindFirstObjectByType<PlayerHeartUI>();
        if (bossHpBarUI == null) bossHpBarUI = FindFirstObjectByType<BossHpBarUI>(FindObjectsInactive.Include);
        if (playerAutoRunner == null) playerAutoRunner = FindFirstObjectByType<PlayerAutoRunner>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;
        gameManager.Events.StageRequested -= OnStageRequested;
        gameManager.Events.SongEnded -= OnSongEnded;
        gameManager.Events.NodeSuccess -= OnNodeSuccess;
        gameManager.Events.NodeMiss -= OnNodeMiss;
    }

    private void OnStageRequested(int stageIndex) => StartStage(stageIndex);

    public void StartStage(int stageIndex)
    {
        if (stageCatalog == null || !stageCatalog.TryGetStage(stageIndex, out currentStage)) return;
        StopAllCoroutines();
        stageRunning = false;
        clearing = false;
        combo = 0;
        if (clearUiGroup != null) clearUiGroup.alpha = 0f;
        if (playerHealth != null) playerHealth.ResetHp();
        if (heartUI != null) heartUI.RefreshAll();
        if (playerAutoRunner != null) playerAutoRunner.StopAutoRun();
        SpawnBoss();
        if (lightningVfxPool != null) lightningVfxPool.Initialize(gameManager, boss != null ? boss.transform : null);
        if (conductor != null && currentStage.MusicClip != null) conductor.SetStageMusic(currentStage.MusicClip, currentStage.Bpm, musicLeadIn);
        if (beatSpawner != null) { beatSpawner.LoadStage(stageIndex); beatSpawner.StartSpawning(); }
        if (conductor != null) conductor.Play();
        stageRunning = true;
        if (gameManager.Events != null) { gameManager.Events.RaiseStageStarted(stageIndex); gameManager.Events.RaiseGameStateChanged(GameState.StagePlaying); }
    }

    private void SpawnBoss()
    {
        if (bossRoot != null) for (int i = bossRoot.childCount - 1; i >= 0; i--) Destroy(bossRoot.GetChild(i).gameObject);
        boss = null;
        if (currentStage.BossPrefab == null || bossSpawnPoint == null) return;
        BossSpawnContext.IsStageManagerSpawning = true;
        BossSpawnContext.LastReason = "StageManager.SpawnBoss";
        var go = Instantiate(currentStage.BossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, bossRoot);
        BossSpawnContext.IsStageManagerSpawning = false;
        boss = go.GetComponent<BossController>();
        if (boss != null && bossHpBarUI != null) { boss.SetHpBarUI(bossHpBarUI); bossHpBarUI.Show(); bossHpBarUI.SetNormalized(1f); }
    }

    private void Update()
    {
        if (!stageRunning || clearing) return;
        if (playerHealth != null && playerHealth.CurrentHp <= 0) { stageRunning = false; StartCoroutine(FailSequence()); }
    }

    private void OnNodeSuccess()
    {
        if (!stageRunning || clearing || boss == null) return;
        combo++;
        int damage = baseDamage + (combo * comboScaling);
        boss.ApplyDamage(damage);
    }

    private void OnNodeMiss()
    {
        combo = 0;
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
        if (beatSpawner != null) beatSpawner.StopSpawning();
        gameManager.Events.RaiseGameStateChanged(GameState.StageClearing);

        for (int i = 0; i < finisherHitCount; i++)
        {
            if (boss != null) boss.LightningHit();
            yield return new WaitForSeconds(finisherInterval);
        }

        if (boss != null) boss.FinishKill();
        yield return new WaitForSeconds(bossVanishDelay);
        if (bossHpBarUI != null) bossHpBarUI.Hide();
        if (boss != null) boss.gameObject.SetActive(false);

        if (playerAutoRunner != null)
        {
            playerAutoRunner.ConfigureSpeed(runOutSpeed);
            playerAutoRunner.StartAutoRun();
            yield return WaitUntilOffScreen(playerAutoRunner.transform);
            playerAutoRunner.StopAutoRun();
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        if (clearUiGroup != null) clearUiGroup.alpha = 1f;
        if (gameManager.Save != null) gameManager.Save.MarkStageCleared(currentStage.StageIndex);
        gameManager.Events.RaiseGameStateChanged(GameState.StageClear);
    }

    private IEnumerator WaitUntilOffScreen(Transform target)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) { yield return new WaitForSeconds(2f); yield break; }
        float elapsed = 0f;
        while (elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            Vector3 vp = mainCamera.WorldToViewportPoint(target.position);
            if (vp.x > 1.1f) yield break;
            yield return null;
        }
    }

    private IEnumerator FailSequence()
    {
        clearing = true;
        if (conductor != null) conductor.Stop();
        if (beatSpawner != null) beatSpawner.StopSpawning();
        yield return new WaitForSeconds(1f);
        gameManager.Events.RaiseGameStateChanged(GameState.StageFailed);
    }
}