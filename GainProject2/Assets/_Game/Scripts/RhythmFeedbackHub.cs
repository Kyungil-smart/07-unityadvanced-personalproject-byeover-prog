using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class RhythmFeedbackHub : MonoBehaviour
    {
        [Header("시스템 연결")]
        [Tooltip("이벤트를 수신할 게임 매니저")]
        [SerializeField] private GameManager gameManager;

        [Header("피드백 참조")]
        [Tooltip("보스 피격 반응(선택)")]
        [SerializeField] private BossHitReact bossHitReact;

        [Tooltip("히트바 이펙트(선택)")]
        [SerializeField] private HitBarSpawner hitBarSpawner;

        [Header("반응 설정")]
        [Tooltip("성공 시 보스 반응 여부")]
        [SerializeField] private bool reactOnHit = true;

        [Tooltip("성공 시 히트바 생성 여부")]
        [SerializeField] private bool spawnBarOnHit = true;

        [Tooltip("실패(Miss) 시 히트바 생성 여부")]
        [SerializeField] private bool spawnBarOnMiss = false;

        private void Awake()
        {
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
            if (bossHitReact == null) bossHitReact = FindFirstObjectByType<BossHitReact>();
            if (hitBarSpawner == null) hitBarSpawner = FindFirstObjectByType<HitBarSpawner>();
        }

        private void OnEnable()
        {
            if (gameManager != null && gameManager.Events != null)
            {
                gameManager.Events.NodeSuccess += HandleHit;
                gameManager.Events.NodeMiss += HandleMiss;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null && gameManager.Events != null)
            {
                gameManager.Events.NodeSuccess -= HandleHit;
                gameManager.Events.NodeMiss -= HandleMiss;
            }
        }

        private void HandleHit()
        {
            if (reactOnHit && bossHitReact != null) bossHitReact.Hit();
            if (spawnBarOnHit && hitBarSpawner != null) hitBarSpawner.SpawnFlash();
        }

        private void HandleMiss()
        {
            if (spawnBarOnMiss && hitBarSpawner != null) hitBarSpawner.SpawnFlash();
        }
    }
}