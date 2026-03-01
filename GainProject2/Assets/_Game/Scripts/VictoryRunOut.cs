using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class VictoryRunOut : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private PlayerAutoRunner autoRunner;
        [SerializeField] private Camera targetCamera;

        [Header("이동")]
        [Min(0f)]
        [SerializeField] private float runSpeed = 3.5f;

        [Tooltip("화면 밖 판정 여유(0~0.5 권장). 0.1이면 화면 바깥 10% 지점까지 나가면 종료")]
        [Range(0f, 0.5f)]
        [SerializeField] private float offscreenPadding = 0.1f;

        [Header("디버그")]
        [SerializeField] private bool debugLog = true;

        private bool isActive;

        private void Reset()
        {
            autoRunner = GetComponent<PlayerAutoRunner>();
            targetCamera = Camera.main;
        }

        private void Awake()
        {
            if (autoRunner == null) autoRunner = GetComponent<PlayerAutoRunner>();
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void Update()
        {
            if (!isActive) return;
            if (autoRunner == null || targetCamera == null) return;

            if (IsOffscreenToRight(transform.position))
            {
                isActive = false;
                autoRunner.StopAndZeroVelocity();
                if (debugLog) Debug.Log("[VictoryRunOut] Finish (offscreen)", this);
            }
        }

        public void StartRunOut()
        {
            if (autoRunner == null)
            {
                if (debugLog) Debug.LogWarning("[VictoryRunOut] autoRunner missing", this);
                return;
            }

            autoRunner.RunSpeed = runSpeed;
            autoRunner.SetRunning(true);
            isActive = true;

            if (debugLog) Debug.Log($"[VictoryRunOut] StartRunOut speed={runSpeed}", this);
        }

        private bool IsOffscreenToRight(Vector3 worldPos)
        {
            Vector3 vp = targetCamera.WorldToViewportPoint(worldPos);
            if (vp.z < 0f) return false;
            return vp.x > (1f + offscreenPadding);
        }
    }
}