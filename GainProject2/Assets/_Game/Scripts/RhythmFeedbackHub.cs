using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class RhythmFeedbackHub : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("판정 시스템")]
        [SerializeField] private JudgeSystem judgeSystem;

        [Tooltip("보스 피격 반응(선택)")]
        [SerializeField] private BossHitReact bossHitReact;

        [Tooltip("히트바 이펙트(선택)")]
        [SerializeField] private HitBarSpawner hitBarSpawner;

        [Header("반응 설정")]
        [Tooltip("Perfect/Good일 때 보스 반응")]
        [SerializeField] private bool reactOnHit = true;

        [Tooltip("Perfect/Good일 때 히트바 생성")]
        [SerializeField] private bool spawnBarOnHit = true;

        [Tooltip("Miss일 때도 히트바 생성")]
        [SerializeField] private bool spawnBarOnMiss = false;

        private void Awake()
        {
            if (judgeSystem == null) judgeSystem = FindFirstObjectByType<JudgeSystem>();
            if (bossHitReact == null) bossHitReact = FindFirstObjectByType<BossHitReact>();
            if (hitBarSpawner == null) hitBarSpawner = FindFirstObjectByType<HitBarSpawner>();
        }

        private void OnEnable()
        {
            if (judgeSystem != null)
                judgeSystem.OnJudged += HandleJudged;
        }

        private void OnDisable()
        {
            if (judgeSystem != null)
                judgeSystem.OnJudged -= HandleJudged;
        }

        private void HandleJudged(JudgeResult result)
        {
            if (result == JudgeResult.Perfect || result == JudgeResult.Good)
            {
                if (reactOnHit && bossHitReact != null)
                    bossHitReact.Hit();

                if (spawnBarOnHit && hitBarSpawner != null)
                    hitBarSpawner.SpawnFlash();

                return;
            }

            if (result == JudgeResult.Miss)
            {
                if (spawnBarOnMiss && hitBarSpawner != null)
                    hitBarSpawner.SpawnFlash();
            }
        }
    }
}