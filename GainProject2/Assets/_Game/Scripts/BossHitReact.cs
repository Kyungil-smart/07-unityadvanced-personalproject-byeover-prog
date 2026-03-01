using System.Collections;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class BossHitReact : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("보스 SpriteRenderer(없으면 자식에서 자동 탐색)")]
        [SerializeField] private SpriteRenderer bossSprite;

        [Header("반짝")]
        [Tooltip("반짝 지속 시간(초)")]
        [Min(0.01f)]
        [SerializeField] private float flashDuration = 0.08f;

        [Tooltip("반짝 색상")]
        [SerializeField] private Color flashColor = Color.white;

        [Header("흔들림")]
        [Tooltip("흔들림 지속 시간(초)")]
        [Min(0.01f)]
        [SerializeField] private float shakeDuration = 0.10f;

        [Tooltip("흔들림 강도(유닛)")]
        [Min(0f)]
        [SerializeField] private float shakeAmplitude = 0.06f;

        private Color _originalColor;
        private Vector3 _originalLocalPos;
        private Coroutine _runningCo;

        private void Awake()
        {
            if (bossSprite == null)
                bossSprite = GetComponentInChildren<SpriteRenderer>();

            if (bossSprite != null)
                _originalColor = bossSprite.color;

            _originalLocalPos = transform.localPosition;
        }

        public void Hit()
        {
            if (_runningCo != null) StopCoroutine(_runningCo);
            _runningCo = StartCoroutine(CoHit());
        }

        //아아 또 돌고돌아 코루틴인가?
        //이 앞 코루틴이다.
        
        private IEnumerator CoHit()
        {
            if (bossSprite != null)
                bossSprite.color = flashColor;

            float t = 0f;
            while (t < shakeDuration)
            {
                t += Time.unscaledDeltaTime;

                Vector2 r = Random.insideUnitCircle * shakeAmplitude;
                transform.localPosition = _originalLocalPos + new Vector3(r.x, r.y, 0f);

                yield return null;
            }

            transform.localPosition = _originalLocalPos;

            if (bossSprite != null)
                bossSprite.color = _originalColor;

            _runningCo = null;
        }
    }
}