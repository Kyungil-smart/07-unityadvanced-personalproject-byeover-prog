using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class HitLineMarker : MonoBehaviour
    {
        [Header("표시")]
        [Tooltip("마커 색상")]
        [SerializeField] private Color color = new Color(1f, 0.2f, 0.2f, 1f);

        [Tooltip("마커 반지름(유닛)")]
        [Min(0.01f)]
        [SerializeField] private float radius = 0.25f;

        [Tooltip("십자선 길이(유닛)")]
        [Min(0f)]
        [SerializeField] private float crossSize = 0.35f;

        private void OnDrawGizmos()
        {
            Gizmos.color = color;

            Vector3 p = transform.position;
            Gizmos.DrawWireSphere(p, radius);

            if (crossSize > 0f)
            {
                Gizmos.DrawLine(p + Vector3.left * crossSize, p + Vector3.right * crossSize);
                Gizmos.DrawLine(p + Vector3.up * crossSize, p + Vector3.down * crossSize);
            }
        }
    }
}
//이게 왜 되는거지?