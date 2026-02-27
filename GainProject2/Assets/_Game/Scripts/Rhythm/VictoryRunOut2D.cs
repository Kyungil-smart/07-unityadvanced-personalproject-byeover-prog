using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class VictoryRunOut2D : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("플레이어 Animator(없으면 자식에서 자동 탐색)")]
        [SerializeField] private Animator playerAnimator;

        [Tooltip("기준 카메라(없으면 Main Camera 사용)")]
        [SerializeField] private Camera targetCamera;

        [Header("Animator 파라미터")]
        [Tooltip("Run 제어용 Bool 파라미터 이름")]
        [SerializeField] private string runningBoolName = "IsRunning";

        [Header("이동")]
        [Tooltip("승리 달리기 속도(유닛/초)")]
        [Min(0f)]
        [SerializeField] private float runSpeed = 3.5f;

        [Tooltip("화면 밖 판정 여유(Viewport 기준)")]
        [Min(0f)]
        [SerializeField] private float offscreenPadding = 0.10f;

        [Header("디버그")]
        [SerializeField] private bool debugLog = true;

        private int _runningBoolHash;
        private bool _isRunningOut;

        private void Awake()
        {
            if (playerAnimator == null)
                playerAnimator = GetComponentInChildren<Animator>();

            if (targetCamera == null)
                targetCamera = Camera.main;

            _runningBoolHash = Animator.StringToHash(runningBoolName);
        }

        public void StartVictoryRunOut()
        {
            _isRunningOut = true;

            if (playerAnimator != null)
                playerAnimator.SetBool(_runningBoolHash, true);
        }

        private void Update()
        {
            if (!_isRunningOut) return;

            transform.position += Vector3.right * (runSpeed * Time.deltaTime);

            if (targetCamera == null) return;

            Vector3 vp = targetCamera.WorldToViewportPoint(transform.position);
            if (vp.x > 1f + offscreenPadding)
            {
                _isRunningOut = false;

                if (playerAnimator != null)
                    playerAnimator.SetBool(_runningBoolHash, false);

                OnCleared();
            }
        }

        private void OnCleared()
        {
            if (debugLog)
                Debug.Log("[VictoryRunOut2D] GAME CLEAR");

            // TODO: Hook your Clear UI here (e.g., UIManager.ShowClear()).
        }
    }
}