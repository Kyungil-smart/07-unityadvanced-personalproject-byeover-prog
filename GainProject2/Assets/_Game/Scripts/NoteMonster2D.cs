using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class NoteMonster2D : MonoBehaviour
    {
        [Header("홉 이동")]
        [Tooltip("스폰→히트라인까지 몇 비트에 걸쳐 이동할지")]
        [Min(1)]
        [SerializeField] private int beatsToTravel = 4;

        [Tooltip("홉 높이 (월드 유닛). 비트마다 뛰어오르는 높이")]
        [Min(0f)]
        [SerializeField] private float hopHeight = 0.35f;

        [Header("스쿼시 & 스트레치")]
        [Tooltip("착지 시 스쿼시 (Y 스케일 배수, 1=변형 없음)")]
        [SerializeField] private float squashY = 0.75f;

        [Tooltip("점프 정점 스트레치 (Y 스케일 배수)")]
        [SerializeField] private float stretchY = 1.2f;

        [Tooltip("스쿼시 복원 속도")]
        [SerializeField] private float squashRecoverSpeed = 12f;

        [Header("디버그")]
        [SerializeField] private bool autoReturnOnLate = true;
        
        private NotePool2D _pool;
        private RhythmConductor2D _conductor;

        private Vector3 _spawnPos;
        private Vector3 _hitPos;

        private double _targetHitBeat;   // 히트라인 도착 비트 (float형 비트 인덱스)
        private double _spawnBeat;       // 스폰 비트
        private bool _initialized;

        private Vector3 _baseScale;
        private float _currentScaleY;
        
        public double TargetHitTime { get; private set; }  // 초 단위 (JudgeSystem 호환)

        public void SetPool(NotePool2D pool) => _pool = pool;

        // ChartPlayer2D 호환용 (기존 인터페이스 유지)
        public void SetBaseTravelTime(float seconds)
        {
            // beatsToTravel로 대체되므로 무시하거나, 
            // seconds 기반으로 비트 수를 역산할 수도 있음
        }

        
        // 비트 기반 초기화. ChartPlayer2D에서 호출
        public void Init(RhythmConductor2D conductor, Transform spawnPoint, Transform hitLine, double targetHitTimeSec)
        {
            _conductor = conductor;

            _spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            _hitPos = hitLine != null ? hitLine.position : _spawnPos;

            // 초 → 비트 변환
            _targetHitBeat = conductor.SecondsToBeat(targetHitTimeSec);
            _spawnBeat = _targetHitBeat - beatsToTravel;

            TargetHitTime = targetHitTimeSec;

            _baseScale = transform.localScale;
            _currentScaleY = 1f;

            _initialized = true;
            transform.position = _spawnPos;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_initialized || _conductor == null) return;

            double currentBeat = _conductor.SongTimeBeat;

            // ── 비트 진행률 계산 ──
            // beatProgress: 0 = 스폰 비트, beatsToTravel = 히트라인 도착
            double beatProgress = currentBeat - _spawnBeat;

            if (beatProgress < 0)
            {
                // 아직 스폰 타이밍 전 → 스폰 위치에 대기
                transform.position = _spawnPos;
                return;
            }

            // 전체 비트 중 몇 번째 홉인지
            int hopIndex = Mathf.FloorToInt((float)beatProgress);
            float hopFraction = (float)(beatProgress - hopIndex); // 현재 홉 내 진행률 (0~1)

            // beatsToTravel을 넘어서면 히트라인 고정
            if (hopIndex >= beatsToTravel)
            {
                transform.position = _hitPos;

                // 늦게 도착한 노트 자동 회수
                if (autoReturnOnLate && currentBeat > _targetHitBeat + 0.5)
                    Despawn();
                return;
            }

            // ── 수평 이동 (비트마다 균등 분할) ──
            float startT = (float)hopIndex / beatsToTravel;
            float endT = (float)(hopIndex + 1) / beatsToTravel;
            float horizontalT = Mathf.Lerp(startT, endT, hopFraction);

            Vector3 flatPos = Vector3.Lerp(_spawnPos, _hitPos, horizontalT);

            // ── 수직 홉 (포물선) ──
            // hopFraction 0→0.5→1 에서 높이 0→max→0
            float parabola = 4f * hopHeight * hopFraction * (1f - hopFraction);
            flatPos.y += parabola;

            transform.position = flatPos;

            // ── 스쿼시 & 스트레치 ──
            float targetScaleY;
            if (hopFraction < 0.1f)
            {
                // 착지 직후 → 스쿼시
                targetScaleY = squashY;
            }
            else if (hopFraction > 0.3f && hopFraction < 0.7f)
            {
                // 공중 → 스트레치
                targetScaleY = stretchY;
            }
            else
            {
                targetScaleY = 1f;
            }

            _currentScaleY = Mathf.Lerp(_currentScaleY, targetScaleY, Time.deltaTime * squashRecoverSpeed);

            // 부피 보존: Y가 줄면 X가 늘어남
            float scaleX = 1f / Mathf.Max(0.5f, _currentScaleY);
            transform.localScale = new Vector3(
                _baseScale.x * scaleX,
                _baseScale.y * _currentScaleY,
                _baseScale.z
            );
        }

        public void Despawn()
        {
            _initialized = false;
            transform.localScale = _baseScale;

            if (_pool != null)
                _pool.Return(this);
            else
                gameObject.SetActive(false);
        }
    }
}
