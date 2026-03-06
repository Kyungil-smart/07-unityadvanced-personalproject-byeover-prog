using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class RhythmFeedbackHub : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BossHitReact bossHitReact;

        [Header("설정")]
        [SerializeField] private bool reactOnHit = true;

        private void Awake()
        {
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
            if (bossHitReact == null) bossHitReact = FindFirstObjectByType<BossHitReact>();
        }

        private void OnEnable()
        {
            if (gameManager != null && gameManager.Events != null)
                gameManager.Events.NodeSuccess += HandleHit;
        }

        private void OnDisable()
        {
            if (gameManager != null && gameManager.Events != null)
                gameManager.Events.NodeSuccess -= HandleHit;
        }

        private void HandleHit()
        {
            if (reactOnHit && bossHitReact != null) bossHitReact.Hit();
        }
    }
}