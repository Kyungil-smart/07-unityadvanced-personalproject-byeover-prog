using System.Collections;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class HitBarEffect : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private SpriteRenderer barRenderer;

        [Header("플래시")]
        [SerializeField, Tooltip("최대 밝기 유지 시간"), Min(0.01f)]
        private float holdDuration = 0.03f;

        [SerializeField, Tooltip("페이드아웃 시간"), Min(0.01f)]
        private float fadeDuration = 0.2f;

        [SerializeField, Tooltip("타임스케일 무시")]
        private bool useUnscaledTime = true;

        private HitBarPool _pool;
        private Color _baseColor;
        private Coroutine _running;

        private void Awake()
        {
            if (barRenderer == null) barRenderer = GetComponentInChildren<SpriteRenderer>();
            if (barRenderer != null) _baseColor = barRenderer.color;
        }

        public void SetPool(HitBarPool pool) => _pool = pool;

        public void PlayFlash(Vector3 position, Color color, float widthScale, float heightScale, int sortingOrder)
        {
            transform.position = position;
            transform.localScale = new Vector3(
                Mathf.Max(0.001f, widthScale),
                Mathf.Max(0.001f, heightScale),
                1f
            );

            _baseColor = color;

            if (barRenderer != null)
            {
                barRenderer.color = color;
                barRenderer.sortingOrder = sortingOrder;
            }

            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(CoFlash());
        }

        private IEnumerator CoFlash()
        {
            // Hold: 최대 밝기 유지
            SetAlpha(_baseColor.a);

            float t = 0f;
            while (t < holdDuration)
            {
                t += Dt();
                yield return null;
            }

            // Fade: 부드럽게 사라짐 (EaseOut)
            t = 0f;
            float fd = Mathf.Max(0.001f, fadeDuration);
            while (t < fd)
            {
                t += Dt();
                float progress = Mathf.Clamp01(t / fd);
                // EaseOutQuad: 처음에 빨리 사라지고 끝에 천천히
                float eased = 1f - (1f - progress) * (1f - progress);
                float alpha = Mathf.Lerp(_baseColor.a, 0f, eased);
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(0f);
            _running = null;

            if (_pool != null) _pool.Return(this);
            else gameObject.SetActive(false);
        }

        private void SetAlpha(float a)
        {
            if (barRenderer == null) return;
            var c = _baseColor;
            c.a = a;
            barRenderer.color = c;
        }

        private float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }
}