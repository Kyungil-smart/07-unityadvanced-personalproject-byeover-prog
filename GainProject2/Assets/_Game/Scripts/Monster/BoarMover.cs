using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Monster
{
    [DisallowMultipleComponent]
    public sealed class BoarMover : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.left;
        [SerializeField] private float stepDistance = 2f; // 유령의 2배 속도
        [SerializeField] private float smoothTime = 0.05f;

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
                int stepsToTake = currentStep - lastStep;
                lastStep = currentStep;
                targetPosition += moveDirection * (stepDistance * stepsToTake);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}