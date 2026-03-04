using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GnalIhu.Rhythm;

public sealed class NoteSpawner : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RhythmConductor conductor;

    private GameManager gameManager;
    private RhythmSpawnPatternSO patternSO;
    private Transform[] laneSpawnPoints;
    private Transform activeRoot;
    private Transform poolRoot;
    private Coroutine spawnCo;

    public void Initialize(GameManager manager) { gameManager = manager; }

    public void Configure(StageData data, Transform[] lanes, Transform activeRoot, Transform poolRoot)
    {
        patternSO = data.SpawnPatternSO; 
        this.laneSpawnPoints = lanes;
        this.activeRoot = activeRoot;
        this.poolRoot = poolRoot;
    }

    public void StartSpawning()
    {
        StopSpawning();
        if (conductor != null && patternSO != null) spawnCo = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCo != null) { StopCoroutine(spawnCo); spawnCo = null; }
    }

    public void ClearAll()
    {
        if (activeRoot == null) return;
        for (int i = activeRoot.childCount - 1; i >= 0; i--)
        {
            var child = activeRoot.GetChild(i);
            child.gameObject.SetActive(false);
            if (poolRoot != null) child.SetParent(poolRoot);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (conductor.SongTime < 0) yield return null;

        if (patternSO.cues.Count == 0) yield break;
        
        var sortedCues = new List<RhythmSpawnPatternSO.SpawnCue>(patternSO.cues);
        sortedCues.Sort((a, b) => (a.beat + a.subBeat).CompareTo(b.beat + b.subBeat));

        int eventIndex = 0;
        double cycleOffset = 0;

        while (true)
        {
            if (eventIndex >= sortedCues.Count)
            {
                if (patternSO.loop) { eventIndex = 0; cycleOffset += patternSO.lengthBeats; }
                else break;
            }

            var cue = sortedCues[eventIndex];
            double baseTargetBeat = cycleOffset + cue.beat + cue.subBeat;
            
            while (conductor.CurrentBeat < baseTargetBeat) yield return null;

            for (int i = 0; i < cue.count; i++)
            {
                if (i > 0)
                {
                    double nextBeat = baseTargetBeat + (cue.withinCueSpacingBeats * i);
                    while (conductor.CurrentBeat < nextBeat) yield return null;
                }
                SpawnSpecificNote(cue.prefab, cue.laneId);
            }
            eventIndex++;
        }
    }

    private void SpawnSpecificNote(GameObject prefab, string laneIdStr)
    {
        if (prefab == null || laneSpawnPoints == null || laneSpawnPoints.Length == 0) return;

        // SO에 적힌 laneId를 읽어서 해당 레인에서 스폰합니다.
        int laneIdx = 0;
        if (!string.IsNullOrEmpty(laneIdStr) && int.TryParse(laneIdStr, out int parsed))
        {
            laneIdx = Mathf.Clamp(parsed, 0, laneSpawnPoints.Length - 1);
        }
        Transform sp = laneSpawnPoints[laneIdx];

        GameObject go = Instantiate(prefab, sp.position, sp.rotation, activeRoot);
        if (go.TryGetComponent(out NodeMonster node))
        {
            node.Initialize(gameManager, (n) =>
            {
                n.gameObject.SetActive(false);
                if (poolRoot != null) n.transform.SetParent(poolRoot);
            });
            node.Spawn(sp.position, null);
        }
    }
}