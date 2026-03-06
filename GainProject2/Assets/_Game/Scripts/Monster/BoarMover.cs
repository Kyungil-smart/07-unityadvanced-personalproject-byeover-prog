using UnityEngine;

namespace _Game.Scripts.Monster
{
    /// <summary>
    /// 멧돼지: 2박 주기로 크게 돌진.
    /// 짝수박에만 3유닛씩 확 이동 = 평균 1.5유닛/박
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoarMover : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.left;
        [SerializeField] private float dashDistance = 3f;
        [SerializeField] private float smoothTime = 0.03f;

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
                    // 짝수박에만 크게 돌진
                    if (i % 2 == 0)
                        targetPosition += moveDirection.normalized * dashDistance;
                }
                lastStep = currentStep;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}