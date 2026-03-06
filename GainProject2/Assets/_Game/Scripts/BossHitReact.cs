using System.Collections;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class BossHitReact : MonoBehaviour
    {
        [Header("컴포넌트")]
        [SerializeField, Tooltip("보스 렌더러")] private SpriteRenderer bossSprite;

        [Header("피격 설정")]
        [SerializeField, Min(0.01f), Tooltip("반짝임 시간")] private float flashDuration = 0.08f;
        [SerializeField, Tooltip("반짝임 색상")] private Color flashColor = Color.white;

        [Header("흔들림 설정")]
        [SerializeField, Min(0.01f), Tooltip("흔들림 시간")] private float shakeDuration = 0.10f;
        [SerializeField, Min(0f), Tooltip("흔들림 강도")] private float shakeAmplitude = 0.06f;

        private Color originalColor;
        private Vector3 originalLocalPos;
        private Coroutine runningCoroutine;

        private void Awake()
        {
            if (bossSprite == null) bossSprite = GetComponentInChildren<SpriteRenderer>();
            if (bossSprite != null) originalColor = bossSprite.color;
            originalLocalPos = transform.localPosition;
        }

        /// <summary>BossController에서 루트 SR 대신 자식 Visual SR로 교체할 때 호출</summary>
        public void OverrideBossSprite(SpriteRenderer newSprite)
        {
            bossSprite = newSprite;
            if (bossSprite != null) originalColor = bossSprite.color;
        }

        public void Hit()
        {
            if (runningCoroutine != null) StopCoroutine(runningCoroutine);
            runningCoroutine = StartCoroutine(ProcessHitEffect());
        }

        private IEnumerator ProcessHitEffect()
        {
            if (bossSprite != null) bossSprite.color = flashColor;

            float timer = 0f;
            while (timer < flashDuration)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            float shakeTimer = 0f;
            while (shakeTimer < shakeDuration)
            {
                shakeTimer += Time.unscaledDeltaTime;
                Vector2 randomOffset = Random.insideUnitCircle * shakeAmplitude;
                transform.localPosition = originalLocalPos + new Vector3(randomOffset.x, randomOffset.y, 0f);
                yield return null;
            }

            transform.localPosition = originalLocalPos;
            if (bossSprite != null) bossSprite.color = originalColor;
            runningCoroutine = null;
        }
    }
}