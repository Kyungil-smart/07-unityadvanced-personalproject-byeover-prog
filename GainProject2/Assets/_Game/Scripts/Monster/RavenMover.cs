using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Monster
{
    [DisallowMultipleComponent]
    public sealed class RavenMover : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.left;
        [SerializeField] private float dashDistance = 2f;
        [SerializeField] private float dashSmoothTime = 0.02f;

        private RhythmConductor conductor;
        private double spawnBeat;
        private int lastStep = -1;
        private Vector3 targetPosition;
        private Vector3 velocity;

        public void Initialize(RhythmConductor syncConductor)
        {
            conductor = syncConductor;
            targetPosition = transform.position;
            spawnBeat = conductor.CurrentBeat;
            lastStep = 0;
        }

        private void Update()
        {
            if (conductor == null || !conductor.IsRunning) return;

            float ageInBeats = (float)(conductor.CurrentBeat - spawnBeat);
            int currentStep = Mathf.FloorToInt(ageInBeats);

            if (currentStep > lastStep)
            {
                for (int i = lastStep + 1; i <= currentStep; i++)
                {
                    // 짝수 박자에만 2칸씩 확 덮칩니다
                    if (i % 2 == 0) targetPosition += moveDirection * dashDistance;
                }
                lastStep = currentStep;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dashSmoothTime);
        }
    }
}