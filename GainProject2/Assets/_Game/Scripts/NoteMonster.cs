using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class NoteMonster : MonoBehaviour
    {
        [Header("이동 데이터")]
        [SerializeField, Min(1), Tooltip("도달 소요 박자")] private int beatsToTravel = 4;
        [SerializeField, Min(0f), Tooltip("홉 높이")] private float hopHeight = 0.35f;

        [Header("실패 데이터")]
        [SerializeField, Min(1), Tooltip("실패 후 플레이어까지 도달 박자")] private int beatsToPlayerAfterMiss = 2;
        [SerializeField, Min(1), Tooltip("실패 시 데미지")] private int missDamage = 1;
        [SerializeField, Tooltip("플레이어 태그")] private string playerTag = "Player";

        private NotePool pool;
        private RhythmConductor conductor;
        private Vector3 spawnPos;
        private Vector3 hitPos;
        private double targetHitBeat;
        private double spawnBeat;
        private bool initialized;
        private bool judged;
        private bool missed;
        private Transform playerTarget;
        private IDamageable playerDamageable;

        public bool IsJudged => judged;
        public double TargetHitTime { get; private set; }

        public void SetPool(NotePool p) => pool = p;

        public void Init(RhythmConductor c, Transform spawnPoint, Transform hitLine, double targetHitTimeSec)
        {
            conductor = c;
            spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            hitPos = hitLine != null ? hitLine.position : spawnPos;
            TargetHitTime = targetHitTimeSec;
            targetHitBeat = TargetHitTime / conductor.BeatDuration;
            spawnBeat = targetHitBeat - (double)beatsToTravel;

            judged = false;
            missed = false;
            initialized = true;

            transform.position = spawnPos;
            gameObject.SetActive(true);
        }

        public void BindPlayer(Transform player, IDamageable damageable)
        {
            playerTarget = player;
            playerDamageable = damageable;
        }

        private void Update()
        {
            if (!initialized || conductor == null || !conductor.IsRunning) return;

            double currentBeat = conductor.CurrentBeat;

            if (!missed)
            {
                double beatProgress = currentBeat - spawnBeat;
                if (beatProgress < 0) return;

                int hopIndex = Mathf.FloorToInt((float)beatProgress);
                float hopFraction = (float)(beatProgress - hopIndex);

                if (hopIndex >= beatsToTravel)
                {
                    transform.position = hitPos;
                    return;
                }

                float horizontalT = (float)(beatProgress / (double)beatsToTravel);
                Vector3 flatPos = Vector3.Lerp(spawnPos, hitPos, horizontalT);
                flatPos.y += 4f * hopHeight * hopFraction * (1f - hopFraction);
                transform.position = flatPos;
            }
            else
            {
                if (playerTarget == null) return;

                // 에러 해결: 명시적 float 형변환
                float missProgress = (float)(currentBeat - targetHitBeat); 
                float t = Mathf.Clamp01(missProgress / (float)beatsToPlayerAfterMiss);
                transform.position = Vector3.Lerp(hitPos, playerTarget.position, t);
            }
        }

        public void Despawn()
        {
            initialized = false;
            if (pool != null) pool.Return(this);
            else gameObject.SetActive(false);
        }
    }
}