using UnityEngine;

namespace GnalIhu.Rhythm
{
    public class RhythmLaneNode : MonoBehaviour
    {
        [Min(0.01f)]
        [Tooltip("이 노드에 도착하기까지 걸리는 박자 수(이전 노드 기준). Node_00(첫 노드)은 의미 없음.")]
        public float beatsFromPrevious = 1f;
    }
}