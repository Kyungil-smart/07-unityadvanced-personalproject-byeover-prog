// UTF-8
using UnityEngine;

namespace _Game.Scripts.Monster
{
    [DisallowMultipleComponent]
    public sealed class MonsterVisualOffset : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("스프라이트가 붙어있는 비주얼 트랜스폼(자식)")]
        [SerializeField] private Transform visual;

        [Header("보정")]
        [Tooltip("루트 기준점 대비 비주얼 로컬 위치 보정값")]
        [SerializeField] private Vector2 localOffset;

        [Header("옵션")]
        [Tooltip("Awake에서 항상 오프셋을 적용(실수로 비주얼 위치가 바뀌어도 복구)")]
        [SerializeField] private bool applyOnAwake = true;

        public void SetOffset(Vector2 offset)
        {
            localOffset = offset;
            ApplyOffset();
        }

        public void ApplyOffset()
        {
            if (visual == null) return;
            visual.localPosition = new Vector3(localOffset.x, localOffset.y, visual.localPosition.z);
        }

        private void Reset()
        {
            if (visual == null)
            {
                var child = transform.Find("Visual");
                if (child != null) visual = child;
            }

            ApplyOffset();
        }

        private void Awake()
        {
            if (applyOnAwake) ApplyOffset();
        }
    }
}