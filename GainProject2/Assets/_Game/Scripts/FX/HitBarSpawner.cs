using UnityEngine;

//툴팁을 안적으면 죽는 병에 걸렸습니다.

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class HitBarSpawner : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("바 풀")]
        [SerializeField] private HitBarPool barPool;

        [Tooltip("히트 기준 위치(히트라인)")]
        [SerializeField] private Transform hitPoint;

        [Tooltip("기준 카메라(없으면 Main Camera)")]
        [SerializeField] private Camera targetCamera;

        [Header("입력")]
        [Tooltip("입력 시 바 생성(테스트/기본)")]
        [SerializeField] private bool spawnOnKey = true;

        [Tooltip("리듬 입력 키(1키)")]
        [SerializeField] private KeyCode rhythmKey = KeyCode.Space;

        [Header("외형")]
        [Tooltip("바 색상(회색)")]
        [SerializeField] private Color barColor = new Color(0.7f, 0.7f, 0.7f, 0.55f);

        [Tooltip("세로 길이(카메라 높이 배수). 1.2면 화면 위/아래 약간 넘김")]
        [Min(0.5f)]
        [SerializeField] private float heightMultiplier = 1.2f;

        [Tooltip("바 두께(유닛)")]
        [Min(0.01f)]
        [SerializeField] private float barWidth = 0.08f;

        [Tooltip("두 줄로 보이게 할지")]
        [SerializeField] private bool doubleLine = true;

        [Tooltip("두 줄 간격(유닛)")]
        [Min(0f)]
        [SerializeField] private float doubleLineGap = 0.14f;

        [Tooltip("살짝 랜덤 오프셋(유닛)")]
        [Min(0f)]
        [SerializeField] private float randomXJitter = 0.04f;

        [Tooltip("앞으로 나오게 SortingOrder")]
        [SerializeField] private int sortingOrder = 200;

        private void Awake()
        {
            if (barPool == null) barPool = FindFirstObjectByType<HitBarPool>();
            if (targetCamera == null) targetCamera = Camera.main;
            if (barPool != null) barPool.Prewarm();
        }

        private void Update()
        {
            if (!spawnOnKey) return;
            if (Input.GetKeyDown(rhythmKey)) SpawnFlash();
        }

        public void SpawnFlash()
        {
            if (barPool == null || hitPoint == null) return;

            float heightScale = ComputeHeightScale();

            float jitter = (Random.value * 2f - 1f) * randomXJitter;
            Vector3 basePos = hitPoint.position + new Vector3(jitter, 0f, 0f);

            SpawnOne(basePos, heightScale);

            if (doubleLine)
                SpawnOne(basePos + Vector3.right * doubleLineGap, heightScale);
        }

        private void SpawnOne(Vector3 pos, float heightScale)
        {
            var bar = barPool.Rent();
            if (bar == null) return;

            bar.transform.SetParent(null, true);
            bar.PlayFlash(pos, barColor, barWidth, heightScale, sortingOrder);
        }

        private float ComputeHeightScale()
        {
            if (targetCamera == null || !targetCamera.orthographic)
                return 10f;

            float worldHeight = targetCamera.orthographicSize * 2f;
            return worldHeight * heightMultiplier;
        }
    }
}