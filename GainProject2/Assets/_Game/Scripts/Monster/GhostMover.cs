using UnityEngine;

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
                int stepsToTake = currentStep - lastStep;
                lastStep = currentStep;
                targetPosition += moveDirection.normalized * (stepDistance * stepsToTake);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}