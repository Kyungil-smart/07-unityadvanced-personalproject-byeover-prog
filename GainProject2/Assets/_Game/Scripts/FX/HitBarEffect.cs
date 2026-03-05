using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class HitBarEffect : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField, Tooltip("월드 스프라이트용(선택)")] private SpriteRenderer spriteRenderer;
        [SerializeField, Tooltip("UI 이미지/텍스트 등(선택)")] private Graphic uiGraphic;

        [Header("플래시")]
        [SerializeField, Tooltip("최대 밝기 유지 시간")] private float holdDuration = 0.05f;
        [SerializeField, Tooltip("사라지는 시간")] private float fadeDuration = 0.12f;
        [SerializeField, Tooltip("타임스케일 무시 여부")] private bool useUnscaledTime = true;

        [Header("반환")]
        [SerializeField, Tooltip("플래시 종료 후 자동 반환 여부")] private bool autoReturnToPool = true;

        private HitBarPool pool;
        private Coroutine running;
        private Color baseColor;
        private bool initialized;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (uiGraphic == null) uiGraphic = GetComponent<Graphic>();

            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
                initialized = true;
                return;
            }

            if (uiGraphic != null)
            {
                baseColor = uiGraphic.color;
                initialized = true;
            }
        }

        public void SetPool(HitBarPool ownerPool)
        {
            pool = ownerPool;
        }

        public void PlayFlash(Vector3 position, Color color, float width, float heightScale, int order)
        {
            EnsureInit();

            transform.position = position;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
                spriteRenderer.sortingOrder = order;
                transform.localScale = new Vector3(Mathf.Max(0.0001f, width), Mathf.Max(0.0001f, heightScale), 1f);
            }
            else if (uiGraphic != null)
            {
                uiGraphic.color = color;
                ApplyUiSize(width, heightScale);
            }

            if (running != null) StopCoroutine(running);
            running = StartCoroutine(CoFlash());
        }

        public void Play()
        {
            EnsureInit();

            if (running != null) StopCoroutine(running);
            running = StartCoroutine(CoFlash());
        }

        private void EnsureInit()
        {
            if (initialized) return;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (uiGraphic == null) uiGraphic = GetComponent<Graphic>();

            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
                initialized = true;
                return;
            }

            if (uiGraphic != null)
            {
                baseColor = uiGraphic.color;
                initialized = true;
            }
        }

        private IEnumerator CoFlash()
        {
            SetAlpha(1f);

            float t = 0f;
            while (t < holdDuration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            t = 0f;
            float fd = Mathf.Max(0.0001f, fadeDuration);
            while (t < fd)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float a = 1f - Mathf.Clamp01(t / fd);
                SetAlpha(a);
                yield return null;
            }

            SetAlpha(0f);
            running = null;

            if (autoReturnToPool && pool != null)
                pool.Return(this);
            else
                gameObject.SetActive(false);
        }

        private void SetAlpha(float a)
        {
            var c = baseColor;
            c.a = a;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = c;
                return;
            }

            if (uiGraphic != null)
                uiGraphic.color = c;
        }

        private void ApplyUiSize(float width, float heightScale)
        {
            if (!TryGetComponent<RectTransform>(out var rt)) return;

            float pxPerUnit = 100f;

            float w = Mathf.Max(1f, width * pxPerUnit);
            float h = Mathf.Max(1f, heightScale * pxPerUnit);

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }
    }
}