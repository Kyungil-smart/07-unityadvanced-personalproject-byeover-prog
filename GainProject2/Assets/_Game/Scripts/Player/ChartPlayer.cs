using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class ChartPlayer : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField, Tooltip("리듬 지휘자")] private RhythmConductor conductor;
        [SerializeField, Tooltip("차트 데이터(JSON)")] private TextAsset chartJson;
        [SerializeField, Tooltip("노트 풀")] private NotePool notePool;
        [SerializeField, Tooltip("판정 시스템")] private JudgeSystem judgeSystem;

        [Header("레인 설정")]
        [SerializeField, Tooltip("노트 스폰 위치")] private Transform spawnPoint;
        [SerializeField, Tooltip("노트 판정선 위치")] private Transform hitLine;

        [Header("타이밍 설정")]
        [SerializeField, Min(0.1f), Tooltip("스폰 후 판정선까지 도달 시간(초)")] private float travelTimeSeconds = 1.2f;

        [Header("차트 설정")]
        [SerializeField, Tooltip("활성화 시 차트 데이터를 박자(Beat) 단위로 해석")] private bool chartTimesAreBeats = true;

        [Header("디버그")]
        [SerializeField, Tooltip("시작 시 자동 재생 여부")] private bool autoPlay = true;

        private ChartData _chart;
        private int _spawnIndex;

        private void Awake()
        {
            if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor>();
            if (judgeSystem == null) judgeSystem = FindFirstObjectByType<JudgeSystem>();

            if (!ChartDataLoader.TryLoad(chartJson, out _chart))
                Debug.LogError("[ChartPlayer] 차트 데이터 로드에 실패했습니다.", this);

            if (notePool != null) notePool.Prewarm();
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

                double hitTimeSec = chartTimesAreBeats
                    ? (double)chartValue * conductor.BeatDuration
                    : (double)chartValue;

                double spawnTimeSec = hitTimeSec - (double)travelTimeSeconds;

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
            
            // SetBaseTravelTime 호출 제거 (NoteMonster.cs 로직에 맞춰 수정)
            note.Init(conductor, spawnPoint, hitLine, targetHitTimeSec);

            if (judgeSystem != null)
                judgeSystem.Register(note);
        }
    }
}