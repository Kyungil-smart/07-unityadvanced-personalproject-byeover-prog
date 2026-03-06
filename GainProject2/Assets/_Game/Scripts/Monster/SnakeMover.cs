using UnityEngine;

namespace _Game.Scripts.Monster
{
    /// <summary>
    /// 뱀: 앞으로 크게 + 가끔 뒤로 살짝.
    /// 3박 주기: +2.5, +2.5, -0.5 = net 4.5유닛/3박 = 1.5유닛/박
    /// Ghost(1유닛/박)보다 1.5배 빠름.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SnakeMover : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.left;
        [SerializeField] private float forwardDistance = 2.5f;
        [SerializeField] private float backwardDistance = 0.5f;
        [SerializeField] private float smoothTime = 0.15f;

        private RhythmConductor conductor;
        private double spawnBeat;
        private int lastStep = -1;
        private Vector3 targetPosition;
        private Vector3 velocity;
        private bool initialized;

        public void Initialize(RhythmConductor syncConductor)
        {
            if (syncConductor == null) return;
            conductor = syncConductor;
            targetPosition = transform.position;
            spawnBeat = conductor.CurrentBeat;
            lastStep = 0;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized || conductor == null) return;

            float ageInBeats = (float)(conductor.CurrentBeat - spawnBeat);
            int currentStep = Mathf.FloorToInt(ageInBeats);

            if (currentStep > lastStep)
            {
                for (int i = lastStep + 1; i <= currentStep; i++)
                {
                    // 3박 주기: 0,1번째=전진 / 2번째=후퇴
                    if (i % 3 == 2)
                        targetPosition -= moveDirection.normalized * backwardDistance;
                    else
                        targetPosition += moveDirection.normalized * forwardDistance;
                }
                lastStep = currentStep;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}