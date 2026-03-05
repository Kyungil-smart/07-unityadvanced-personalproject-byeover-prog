using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GnalIhu.Rhythm;
using _Game.Scripts.Rhythm;

using PatternSO = _Game.Scripts.Rhythm.RhythmSpawnPatternSO;

public sealed class NoteSpawner : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RhythmConductor conductor;

    private GameManager gameManager;
    private PatternSO patternSO;
    private Transform[] laneSpawnPoints;
    private Transform activeRoot;
    private Transform poolRoot;
    private Coroutine spawnCo;

    public void Initialize(GameManager manager) { gameManager = manager; }

    public void Configure(StageData data, Transform[] lanes, Transform activeRoot, Transform poolRoot)
    {
        patternSO = data.SpawnPatternSO;
        laneSpawnPoints = lanes;
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
            ReturnToPool(child.gameObject);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (conductor.SongTime < 0) yield return null;

        var cues = GetCues();
        if (cues == null || cues.Count == 0) yield break;

        var sortedCues = new List<PatternSO.SpawnCue>(cues);
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

    private IList<PatternSO.SpawnCue> GetCues()
    {
        return patternSO != null ? patternSO.cues : null;
    }

    private void SpawnSpecificNote(GameObject prefab, string laneIdStr)
    {
        if (prefab == null || laneSpawnPoints == null || laneSpawnPoints.Length == 0) return;
        if (activeRoot == null) return;

        int laneIdx = ResolveLaneIndex(laneIdStr, laneSpawnPoints.Length);
        Transform sp = laneSpawnPoints[laneIdx];

        GameObject go = GetFromPool(prefab);
        go.transform.SetParent(activeRoot, true);
        go.transform.SetPositionAndRotation(sp.position, sp.rotation);
        go.SetActive(true);

        if (go.TryGetComponent(out NodeMonster node))
        {
            node.Initialize(gameManager, (n) => ReturnToPool(n.gameObject));
            node.Spawn(sp.position, null);
        }
    }

    private int ResolveLaneIndex(string laneIdStr, int laneCount)
    {
        if (laneCount <= 0) return 0;

        int idx = 0;

        if (!string.IsNullOrEmpty(laneIdStr))
        {
            int parsed = -1;

            if (int.TryParse(laneIdStr, out int direct))
            {
                parsed = direct;
            }
            else
            {
                int value = 0;
                bool hasDigit = false;

                for (int i = 0; i < laneIdStr.Length; i++)
                {
                    char c = laneIdStr[i];
                    if (c < '0' || c > '9') continue;
                    hasDigit = true;
                    value = (value * 10) + (c - '0');
                }

                if (hasDigit) parsed = value;
            }

            if (parsed >= 0) idx = parsed;
        }

        return Mathf.Clamp(idx, 0, laneCount - 1);
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (poolRoot != null)
        {
            for (int i = poolRoot.childCount - 1; i >= 0; i--)
            {
                var child = poolRoot.GetChild(i);
                if (child != null && child.name == prefab.name)
                    return child.gameObject;
            }
        }

        var go = Instantiate(prefab);
        go.name = prefab.name;
        go.SetActive(false);
        return go;
    }

    private void ReturnToPool(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        if (poolRoot != null) go.transform.SetParent(poolRoot, false);
        else go.transform.SetParent(null, false);
    }
}