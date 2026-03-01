using UnityEngine;

namespace GnalIhu.Rhythm
{
    /// <summary>
    /// 몹(또는 노트)이 레인의 노드를 따라 박에 맞춰 이동.
    /// - "시간/프레임"이 아니라 "현재 박(CurrentBeat)"으로 위치를 재계산하므로 리듬이 안 깨진다.
    /// </summary>
    public class RhythmLaneMover : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("레인 끝(마지막 노드)에 도달하면 Destroy. 풀링이면 false로 두고 외부에서 비활성 처리.")]
        private bool destroyOnEnd = true;

        private RhythmConductor conductor;
        private RhythmLane lane;
        private double spawnBeat; // 이 오브젝트가 레인 시작점에 있어야 하는 박(스폰 박)
        private bool initialized;

        private Rigidbody2D rb2D;

        private void Awake()
        {
            rb2D = GetComponent<Rigidbody2D>();
        }

        public void Initialize(RhythmConductor conductor, RhythmLane lane, double spawnBeat)
        {
            this.conductor = conductor;
            this.lane = lane;
            this.spawnBeat = spawnBeat;
            initialized = true;

            // 시작 프레임에도 즉시 위치 맞춤
            Tick();
        }

        private void Update()
        {
            Tick();
        }

        private void Tick()
        {
            if (!initialized || conductor == null || lane == null || !conductor.IsRunning)
                return;

            // leadIn 동안에는 움직이지 않음(원하면 여기 정책 변경 가능)
            if (conductor.SongTime < 0)
                return;

            float localBeat = (float)(conductor.CurrentBeat - spawnBeat);

            if (!lane.TryEvaluatePosition(localBeat, out Vector3 pos, out bool reachedEnd))
                return;

            if (rb2D != null)
                rb2D.position = pos;
            else
                transform.position = pos;

            if (reachedEnd)
            {
                // 여기에서 "플레이어 피격/판정" 같은 걸 연결하면 된다.
                if (destroyOnEnd) Destroy(gameObject);
                else gameObject.SetActive(false);
            }
        }
    }
}