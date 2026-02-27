using System.Collections;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class StagePatternRunner2D : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("리듬 시간 기준(필수)")]
        [SerializeField] private RhythmConductor2D conductor;

        [Tooltip("보스 트랜스폼(연출용)")]
        [SerializeField] private Transform bossTransform;

        [Header("연출")]
        [Tooltip("보스 점프 높이(로컬 Y)")]
        [SerializeField] private float bossPulseHeight = 0.18f;

        [Tooltip("보스 점프 유지 시간(초)")]
        [SerializeField] private float bossPulseHold = 0.08f;

        [Header("실행")]
        [Tooltip("시작 시 자동 실행")]
        [SerializeField] private bool autoStart = true;

        private Vector3 _bossBaseLocalPos;

        private void Awake()
        {
            if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor2D>();

            if (bossTransform != null)
                _bossBaseLocalPos = bossTransform.localPosition;
        }

        private void Start()
        {
            if (!autoStart) return;
            if (conductor == null) return;

            if (!conductor.IsPlaying)
                conductor.Play();

            StartCoroutine(Stage1Routine());
        }

        private IEnumerator Stage1Routine()
        {
            yield return RhythmWait2D.After(conductor, 0.65);
            yield return BossPulse();

            yield return RhythmWait2D.After(conductor, 1.30);
            yield return BossPulse();

            yield return RhythmWait2D.After(conductor, 0.75);
            yield return BossPulse();
        }

        private IEnumerator BossPulse()
        {
            if (bossTransform == null) yield break;

            bossTransform.localPosition = _bossBaseLocalPos + new Vector3(0f, bossPulseHeight, 0f);
            yield return RhythmWait2D.After(conductor, bossPulseHold);
            bossTransform.localPosition = _bossBaseLocalPos;
        }
    }
}