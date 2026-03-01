// UTF-8
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class PlayerAutoRunner : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Animator animator;

        [Header("애니메이터 파라미터")]
        [SerializeField] private string runBoolName = "IsRunning";

        [Header("이동")]
        [Min(0f)]
        [SerializeField] private float runSpeed = 6f;

        [Header("디버그")]
        [SerializeField] private bool debugLog;

        private bool isRunning;

        public float RunSpeed
        {
            get => runSpeed;
            set => runSpeed = Mathf.Max(0f, value);
        }

        public bool IsRunning => isRunning;

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
            StopAutoRun();
        }

        private void FixedUpdate()
        {
            if (!isRunning) return;
            if (rb == null) return;

            Vector2 v = rb.velocity;
            v.x = runSpeed;
            rb.velocity = v;
        }

        public void ConfigureSpeed(float speed)
        {
            runSpeed = Mathf.Max(0f, speed);
            if (debugLog) Debug.Log($"[PlayerAutoRunner] ConfigureSpeed={runSpeed}", this);
        }

        public void StartAutoRun()
        {
            SetRunning(true);
        }

        public void StopAutoRun()
        {
            SetRunning(false);
            ZeroXVelocity();
        }

        public void StopAndZeroVelocity()
        {
            StopAutoRun();
        }

        public void SetRunning(bool running)
        {
            if (isRunning == running) return;

            isRunning = running;

            if (animator != null && !string.IsNullOrEmpty(runBoolName))
                animator.SetBool(runBoolName, isRunning);

            if (debugLog) Debug.Log($"[PlayerAutoRunner] SetRunning={isRunning}, speed={runSpeed}", this);
        }

        private void ZeroXVelocity()
        {
            if (rb == null) return;
            Vector2 v = rb.velocity;
            v.x = 0f;
            rb.velocity = v;
        }
    }
}