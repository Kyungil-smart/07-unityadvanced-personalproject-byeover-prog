using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class HitBarSpawner : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private HitBarPool barPool;
        [SerializeField] private Transform hitPoint;
        [SerializeField] private Camera targetCamera;

        [Header("히트 사운드")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip hitSound;
        [SerializeField, Range(0f, 1f)] private float hitSoundVolume = 0.7f;

        [Header("외형")]
        [SerializeField, Tooltip("바 색상")]
        private Color barColor = new Color(1f, 1f, 1f, 0.35f);

        [SerializeField, Tooltip("바 두께"), Min(0.01f)]
        private float barWidth = 0.04f;

        [SerializeField, Tooltip("세로 길이 배수"), Min(0.5f)]
        private float heightMultiplier = 1.1f;

        [SerializeField, Tooltip("앞으로 나오게")]
        private int sortingOrder = 200;

        private GameManager gameManager;

        private void Awake()
        {
            if (barPool == null) barPool = FindFirstObjectByType<HitBarPool>();
            if (targetCamera == null) targetCamera = Camera.main;
            if (barPool != null) barPool.Prewarm();
        }

        private void Start()
        {
            gameManager = GameManager.Instance;

            // NodeSuccess 이벤트 구독 — 히트 성공할 때만 바 생성
            if (gameManager != null && gameManager.Events != null)
                gameManager.Events.NodeSuccess += OnNodeSuccess;
        }

        private void OnDestroy()
        {
            if (gameManager != null && gameManager.Events != null)
                gameManager.Events.NodeSuccess -= OnNodeSuccess;
        }

        private void OnNodeSuccess()
        {
            SpawnFlash();
            PlayHitSound();
        }

        public void SpawnFlash()
        {
            if (barPool == null || hitPoint == null) return;

            float heightScale = ComputeHeightScale();
            Vector3 basePos = hitPoint.position;

            SpawnOne(basePos, heightScale);
        }

        private void SpawnOne(Vector3 pos, float heightScale)
        {
            var bar = barPool.Rent();
            if (bar == null) return;

            bar.transform.SetParent(null, true);
            bar.PlayFlash(pos, barColor, barWidth, heightScale, sortingOrder);
        }

        private void PlayHitSound()
        {
            if (hitSound == null) return;

            if (sfxSource != null)
                sfxSource.PlayOneShot(hitSound, hitSoundVolume);
            else
                AudioSource.PlayClipAtPoint(hitSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero, hitSoundVolume);
        }

        private float ComputeHeightScale()
        {
            if (targetCamera == null || !targetCamera.orthographic)
                return 10f;

            float worldHeight = targetCamera.orthographicSize * 2f;
            return worldHeight * heightMultiplier;
        }
    }
}