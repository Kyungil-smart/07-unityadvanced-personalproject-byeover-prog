using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Monster
{
    [DisallowMultipleComponent]
    public sealed class SnakeMover : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.left;
        [SerializeField] private float forwardDistance = 2f;
        [SerializeField] private float backwardDistance = 1f;
        [SerializeField] private float smoothTime = 0.15f;

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
                    if (i % 3 == 0) targetPosition -= moveDirection * backwardDistance;
                    else targetPosition += moveDirection * forwardDistance;
                }
                lastStep = currentStep;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}