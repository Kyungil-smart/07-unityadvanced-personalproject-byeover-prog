using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class ChartPlayer : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private RhythmConductor conductor;
        [SerializeField] private TextAsset chartJson;
        [SerializeField] private NotePool notePool;
        [SerializeField] private JudgeSystem judgeSystem;

        [Header("레인")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform hitLine;

        [Header("타이밍")]
        [Tooltip("스폰 지점에서 히트 라인까지 도달하는 시간(초)")]
        [Min(0.1f)]
        [SerializeField] private float travelTimeSeconds = 1.2f;

        [Header("차트 단위")]
        [Tooltip("ON=noteTimes를 비트(Beat)로 해석합니다.\nOFF=noteTimes를 초(Seconds)로 해석합니다.")]
        [SerializeField] private bool chartTimesAreBeats = true;

        [Header("디버그")]
        [SerializeField] private bool autoPlay = true;

        private ChartData _chart;
        private int _spawnIndex;

        private void Awake()
        {
            if (conductor == null)
                conductor = FindFirstObjectByType<RhythmConductor>();

            if (judgeSystem == null)
                judgeSystem = FindFirstObjectByType<JudgeSystem>();

            if (!ChartDataLoader.TryLoad(chartJson, out _chart))
                Debug.LogError("[ChartPlayer] 차트 JSON 로드 실패", this);

            if (notePool != null)
                notePool.Prewarm();
        }

        private void Start()
        {
            if (autoPlay && conductor != null)
                conductor.Play();
        }

        private void Update()
        {
            if (_chart == null || conductor == null || !conductor.IsRunning) return;
            if (_chart.noteTimes == null || _chart.noteTimes.Length == 0) return;
            if (notePool == null || spawnPoint == null || hitLine == null) return;

            double songTimeSec = conductor.SongTime;

            while (_spawnIndex < _chart.noteTimes.Length)
            {
                float chartValue = _chart.noteTimes[_spawnIndex];

                // 차트 값 -> 히트 타임(초)
                double hitTimeSec = chartTimesAreBeats
                    ? (double)chartValue * conductor.BeatDuration
                    : (double)chartValue;

                // 스폰 시점(초)
                double travelSec = travelTimeSeconds;
                double spawnTimeSec = hitTimeSec - travelSec;

                if (songTimeSec < spawnTimeSec) break;

                SpawnNote(hitTimeSec);
                _spawnIndex++;
            }
        }

        private void SpawnNote(double targetHitTimeSec)
        {
            var note = notePool.Rent();
            if (note == null) return;

            note.transform.SetParent(null, true);

            note.SetBaseTravelTime(travelTimeSeconds);
            note.Init(conductor, spawnPoint, hitLine, targetHitTimeSec);

            if (judgeSystem != null)
                judgeSystem.Register(note);
        }
    }
}