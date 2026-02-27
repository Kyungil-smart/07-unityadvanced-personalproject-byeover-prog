// UTF-8
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class NoteMonster2D : MonoBehaviour
    {
        [Header("이동")]
        [Tooltip("기본 이동 시간(초). 스크롤 배율이 1일 때 스폰→히트까지 걸리는 시간")]
        [Min(0.05f)]
        [SerializeField] private float baseTravelTime = 1.2f;

        [Header("디버그")]
        [SerializeField] private bool autoReturnOnLate = true;

        private NotePool2D _pool;
        private RhythmConductor2D _conductor;

        private Transform _spawnPoint;
        private Transform _hitLine;

        private Vector3 _spawnPos;
        private Vector3 _hitPos;

        private double _targetHitTime;
        private double _startTime;
        private float _travelTimeFixed;

        private bool _initialized;

        public double TargetHitTime => _targetHitTime;

        public void SetPool(NotePool2D pool)
        {
            _pool = pool;
        }

        public void SetBaseTravelTime(float seconds)
        {
            baseTravelTime = Mathf.Max(0.05f, seconds);
        }

        public void Init(RhythmConductor2D conductor, Transform spawnPoint, Transform hitLine, double targetHitTimeSec)
        {
            _conductor = conductor;
            _spawnPoint = spawnPoint;
            _hitLine = hitLine;

            _spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
            _hitPos = _hitLine != null ? _hitLine.position : _spawnPos;

            _targetHitTime = targetHitTimeSec;

            float speedAtHit = _conductor != null ? _conductor.GetScrollSpeed(_targetHitTime) : 1f;
            _travelTimeFixed = baseTravelTime / Mathf.Max(0.01f, speedAtHit);
            _startTime = _targetHitTime - _travelTimeFixed;

            _initialized = true;

            transform.position = _spawnPos;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_initialized) return;
            if (_conductor == null) return;

            double now = _conductor.SongTimeSeconds;

            float t = Mathf.InverseLerp((float)_startTime, (float)_targetHitTime, (float)now);
            t = Mathf.Clamp01(t);

            transform.position = Vector3.LerpUnclamped(_spawnPos, _hitPos, t);

            if (autoReturnOnLate && now > _targetHitTime + 0.25f)
                Despawn();
        }

        public void Despawn()
        {
            _initialized = false;

            if (_pool != null)
                _pool.Return(this);
            else
                gameObject.SetActive(false);
        }
    }
}