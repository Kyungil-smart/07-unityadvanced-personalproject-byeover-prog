using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Monster
{
    [DisallowMultipleComponent]
    public sealed class GhostMover : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.left;
        [SerializeField] private float stepDistance = 1f;
        [SerializeField] private float smoothTime = 0.1f;

        private RhythmConductor conductor;
        private double spawnBeat;
        private int lastStep = -1;
        private Vector3 targetPosition;
        private Vector3 velocity;

        public void Initialize(RhythmConductor syncConductor)
        {
            conductor = syncConductor;
            targetPosition = transform.position;
            spawnBeat = conductor.CurrentBeat; // 자신이 태어난 정확한 엇박 타이밍을 기억!
            lastStep = 0;
        }

        private void Update()
        {
            if (conductor == null || !conductor.IsRunning) return;

            // 태어난 지 몇 박자가 지났는지 계산
            float ageInBeats = (float)(conductor.CurrentBeat - spawnBeat);
            int currentStep = Mathf.FloorToInt(ageInBeats);

            if (currentStep > lastStep)
            {
                int stepsToTake = currentStep - lastStep;
                lastStep = currentStep;
                targetPosition += moveDirection * (stepDistance * stepsToTake);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}