using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class HitBarEffect : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("바 SpriteRenderer")]
        [SerializeField] private SpriteRenderer barRenderer;

        [Header("플래시")]
        [Tooltip("반짝 유지 시간(초)")]
        [Min(0.01f)]
        [SerializeField] private float holdDuration = 0.05f;

        [Tooltip("사라지는 시간(초)")]
        [Min(0.01f)]
        [SerializeField] private float fadeDuration = 0.12f;

        [Tooltip("unscaled 시간 사용")]
        [SerializeField] private bool useUnscaledTime = true;

        private HitBarPool _pool;
        private float _t;
        private float _end;
        private float _holdEnd;
        private Color _baseColor;
        private bool _running;

        public void SetPool(HitBarPool pool)
        {
            _pool = pool;
        }

        private void Awake()
        {
            if (barRenderer == null) barRenderer = GetComponentInChildren<SpriteRenderer>();
            if (barRenderer != null) _baseColor = barRenderer.color;
        }

        public void PlayFlash(Vector3 position, Color color, float widthScale, float heightScale, int sortingOrder)
        {
            transform.position = position;
            transform.localScale = new Vector3(widthScale, heightScale, 1f);

            _t = 0f;
            _holdEnd = Mathf.Max(0.01f, holdDuration);
            _end = _holdEnd + Mathf.Max(0.01f, fadeDuration);

            _baseColor = color;

            if (barRenderer != null)
            {
                barRenderer.color = color;
                barRenderer.sortingOrder = sortingOrder;
            }

            _running = true;
        }

        private void Update()
        {
            if (!_running) return;

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _t += dt;

            if (_t <= _holdEnd)
                return;

            float u = Mathf.Clamp01((_t - _holdEnd) / Mathf.Max(0.01f, fadeDuration));

            if (barRenderer != null)
            {
                Color c = _baseColor;
                c.a = Mathf.Lerp(_baseColor.a, 0f, u);
                barRenderer.color = c;
            }

            if (_t >= _end)
                Despawn();
        }

        private void Despawn()
        {
            _running = false;

            if (barRenderer != null)
            {
                Color c = _baseColor;
                c.a = 1f;
                barRenderer.color = c;
            }

            if (_pool != null) _pool.Return(this);
            else gameObject.SetActive(false);
        }
    }
}