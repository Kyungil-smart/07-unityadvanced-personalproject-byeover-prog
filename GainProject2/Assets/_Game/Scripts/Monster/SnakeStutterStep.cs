using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Monster
{
    [DisallowMultipleComponent]
    public sealed class SnakeStutterStep : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float forwardDistance = 2f;
        [SerializeField] private float backwardDistance = 1f;
        [SerializeField] private float moveSmoothTime = 0.1f;

        private RhythmConductor conductor;
        private int stepCounter = 0;
        private Vector3 targetPosition;
        private Vector3 moveVelocity;

        public void Initialize(RhythmConductor syncConductor)
        {
            conductor = syncConductor;
            targetPosition = transform.position;

            if (conductor != null)
            {
                conductor.OnBeat += HandleBeat;
            }
        }

        private void OnDisable()
        {
            if (conductor != null)
            {
                conductor.OnBeat -= HandleBeat;
            }
        }

        private void HandleBeat(int beatIndex)
        {
            stepCounter++;

            if (stepCounter % 3 == 0)
            {
                targetPosition += transform.up * backwardDistance; 
            }
            else
            {
                targetPosition -= transform.up * forwardDistance;
            }
        }

        private void Update()
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref moveVelocity, 
                moveSmoothTime
            );
        }
    }
}