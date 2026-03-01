// UTF-8
using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class NoteMonster : MonoBehaviour
    {
        [Header("홉 이동")]
        [Tooltip("스폰→히트라인까지 몇 비트에 걸쳐 이동할지")]
        [Min(1)]
        [SerializeField] private int beatsToTravel = 4;

        [Tooltip("홉 높이 (월드 유닛)")]
        [Min(0f)]
        [SerializeField] private float hopHeight = 0.35f;

        [Header("스쿼시 & 스트레치")]
        [Tooltip("착지 시 스쿼시 (Y 스케일 배수, 1=변형 없음)")]
        [SerializeField] private float squashY = 0.75f;

        [Tooltip("점프 정점 스트레치 (Y 스케일 배수)")]
        [SerializeField] private float stretchY = 1.2f;

        [Tooltip("스쿼시 복원 속도")]
        [SerializeField] private float squashRecoverSpeed = 12f;

        [Header("미스 처리")]
        [Tooltip("미스 후 플레이어까지 몇 비트에 걸쳐 이동할지")]
        [Min(1)]
        [SerializeField] private int beatsToPlayerAfterMiss = 2;

        [Tooltip("미스 후 플레이어에게 적용할 데미지")]
        [Min(1)]
        [SerializeField] private int missDamage = 1;

        [Tooltip("미스 후 플레이어 충돌 트리거(없으면 루트 콜라이더를 트리거로 사용)")]
        [SerializeField] private Collider2D damageTrigger;

        [Tooltip("플레이어 태그(비우면 태그 체크 안 함)")]
        [SerializeField] private string playerTag = "Player";

        [Header("디버그")]
        [Tooltip("너무 늦었을 때 강제 회수(비트). 0 이하면 사용 안 함")]
        [SerializeField] private float despawnIfLateBeats = 0f;

        private NotePool pool;
        private RhythmConductor conductor;

        private Vector3 spawnPos;
        private Vector3 hitPos;

        private double targetHitBeat;
        private double spawnBeat;

        private bool initialized;
        private bool judged;
        private bool missed;

        private Vector3 baseScale;
        private float currentScaleY;

        private Transform playerTarget;
        private IDamageable playerDamageable;

        public bool IsJudged => judged;
        public bool IsMissed => missed;
        public double TargetHitTime { get; private set; }

        public void SetPool(NotePool p) => pool = p;
        public void SetBaseTravelTime(float seconds) { }

        public void Init(RhythmConductor c, Transform spawnPoint, Transform hitLine, double targetHitTimeSec)
        {
            conductor = c;

            spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            hitPos = hitLine != null ? hitLine.position : spawnPos;

            TargetHitTime = targetHitTimeSec;

            targetHitBeat = TargetHitTime / conductor.BeatDuration;
            spawnBeat = targetHitBeat - beatsToTravel;

            baseScale = transform.localScale;
            currentScaleY = 1f;

            judged = false;
            missed = false;
            initialized = true;

            if (damageTrigger != null)
            {
                damageTrigger.isTrigger = true;
                damageTrigger.enabled = false;
            }

            transform.position = spawnPos;
            gameObject.SetActive(true);
        }

        public void BindPlayer(Transform player, IDamageable damageable)
        {
            playerTarget = player;
            playerDamageable = damageable;
        }

        public void Hit()
        {
            if (!initialized || judged) return;
            judged = true;
            Despawn();
        }

        public void Miss()
        {
            if (!initialized || judged || missed) return;
            missed = true;

            if (damageTrigger != null)
                damageTrigger.enabled = true;
        }

        private void Update()
        {
            if (!initialized || conductor == null) return;
            if (!conductor.IsRunning) return;

            double currentBeat = conductor.CurrentBeat;

            if (!missed)
            {
                double beatProgress = currentBeat - spawnBeat;

                if (beatProgress < 0)
                {
                    transform.position = spawnPos;
                    return;
                }

                int hopIndex = Mathf.FloorToInt((float)beatProgress);
                float hopFraction = (float)(beatProgress - hopIndex);

                if (hopIndex >= beatsToTravel)
                {
                    transform.position = hitPos;

                    if (!judged && despawnIfLateBeats > 0f && currentBeat > targetHitBeat + despawnIfLateBeats)
                        Despawn();

                    return;
                }

                float startT = (float)hopIndex / beatsToTravel;
                float endT = (float)(hopIndex + 1) / beatsToTravel;
                float horizontalT = Mathf.Lerp(startT, endT, hopFraction);

                Vector3 flatPos = Vector3.Lerp(spawnPos, hitPos, horizontalT);

                float parabola = 4f * hopHeight * hopFraction * (1f - hopFraction);
                flatPos.y += parabola;

                transform.position = flatPos;

                float targetScaleY;
                if (hopFraction < 0.1f) targetScaleY = squashY;
                else if (hopFraction > 0.3f && hopFraction < 0.7f) targetScaleY = stretchY;
                else targetScaleY = 1f;

                currentScaleY = Mathf.Lerp(currentScaleY, targetScaleY, Time.deltaTime * squashRecoverSpeed);

                float scaleX = 1f / Mathf.Max(0.5f, currentScaleY);
                transform.localScale = new Vector3(baseScale.x * scaleX, baseScale.y * currentScaleY, baseScale.z);
                return;
            }

            if (playerTarget == null)
            {
                Despawn();
                return;
            }

            double missStartBeat = targetHitBeat;
            double missProgress = currentBeat - missStartBeat;

            if (missProgress < 0)
            {
                transform.position = hitPos;
                transform.localScale = baseScale;
                return;
            }

            float t = Mathf.Clamp01((float)(missProgress / beatsToPlayerAfterMiss));
            transform.position = Vector3.Lerp(hitPos, playerTarget.position, t);
            transform.localScale = baseScale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!initialized || !missed || judged) return;
            if (playerTarget == null || playerDamageable == null) return;

            var otherRoot = other.transform.root;
            var playerRoot = playerTarget.root;

            bool sameRoot = otherRoot == playerRoot;
            bool sameTransform = other.transform == playerTarget;

            bool tagOk = true;
            if (!string.IsNullOrWhiteSpace(playerTag))
                tagOk = otherRoot.CompareTag(playerTag) || other.CompareTag(playerTag);

            if (!tagOk) return;
            if (!sameRoot && !sameTransform) return;

            playerDamageable.ApplyDamage(missDamage);
            Despawn();
        }

        public void Despawn()
        {
            initialized = false;
            judged = false;
            missed = false;

            transform.localScale = baseScale;

            if (damageTrigger != null)
                damageTrigger.enabled = false;

            if (pool != null) pool.Return(this);
            else gameObject.SetActive(false);
        }
    }
}