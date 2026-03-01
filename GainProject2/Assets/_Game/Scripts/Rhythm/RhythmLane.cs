using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GnalIhu.Rhythm
{
    public class RhythmLane : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("패턴(SO)에서 참조할 레인 ID. 예: A / B / Left / Right")]
        private string laneId = "A";

        [SerializeField]
        [Tooltip("노드 목록(자동 수집). 순서는 Hierarchy에서 위→아래 순서입니다.")]
        private List<RhythmLaneNode> nodes = new();

        private readonly List<float> cumulativeBeats = new();

        public string LaneId => laneId;

        public float TotalBeats => (cumulativeBeats.Count > 0) ? cumulativeBeats[cumulativeBeats.Count - 1] : 0f;

        public Vector3 StartPosition => (nodes.Count > 0) ? nodes[0].transform.position : transform.position;

        public Vector3 EndPosition => (nodes.Count > 0) ? nodes[nodes.Count - 1].transform.position : transform.position;

        public bool HasValidNodes => nodes.Count >= 2;

        public void RefreshNodes()
        {
            nodes.Clear();

            // "직계 자식"만 노드로 본다(초보용 단순 규칙)
            for (int i = 0; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                var node = t.GetComponent<RhythmLaneNode>();
                if (node != null)
                    nodes.Add(node);
            }

            RebuildCumulative();
        }

        private void RebuildCumulative()
        {
            cumulativeBeats.Clear();

            if (nodes.Count == 0)
                return;

            cumulativeBeats.Add(0f); // Node_00 = 0

            for (int i = 1; i < nodes.Count; i++)
            {
                float seg = Mathf.Max(0.01f, nodes[i].beatsFromPrevious);
                cumulativeBeats.Add(cumulativeBeats[i - 1] + seg);
            }
        }

        /// <summary>
        /// localBeat(0=첫 노드 시점) 기준으로 레인 위 위치를 계산한다.
        /// </summary>
        public bool TryEvaluatePosition(float localBeat, out Vector3 position, out bool reachedEnd)
        {
            position = transform.position;
            reachedEnd = true;

            if (!HasValidNodes)
                return false;

            if (localBeat <= 0f)
            {
                position = StartPosition;
                reachedEnd = false;
                return true;
            }

            float total = TotalBeats;

            if (localBeat >= total)
            {
                position = EndPosition;
                reachedEnd = true;
                return true;
            }

            // 어느 구간(노드 i -> i+1)에 있는지 찾기
            int segIndex = 0;
            for (int i = 0; i < cumulativeBeats.Count - 1; i++)
            {
                if (localBeat < cumulativeBeats[i + 1])
                {
                    segIndex = i;
                    break;
                }
            }

            float aBeat = cumulativeBeats[segIndex];
            float bBeat = cumulativeBeats[segIndex + 1];
            float segLen = Mathf.Max(0.0001f, bBeat - aBeat);

            float t = (localBeat - aBeat) / segLen;

            Vector3 a = nodes[segIndex].transform.position;
            Vector3 b = nodes[segIndex + 1].transform.position;
            position = Vector3.Lerp(a, b, t);

            reachedEnd = false;
            return true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RefreshNodes();
        }

        private void OnDrawGizmos()
        {
            RefreshNodes();

            if (nodes.Count < 2)
                return;

            Gizmos.color = Color.cyan;

            for (int i = 0; i < nodes.Count; i++)
            {
                var p = nodes[i].transform.position;
                Gizmos.DrawSphere(p, 0.08f);

                if (i < nodes.Count - 1)
                {
                    Gizmos.DrawLine(p, nodes[i + 1].transform.position);
                }
            }

            // 라벨은 에디터에서만
            Handles.color = Color.white;
            for (int i = 0; i < nodes.Count; i++)
            {
                string label = (i == 0)
                    ? $"Node_00 (Start)"
                    : $"Node_{i:00} (+{nodes[i].beatsFromPrevious}박)";

                Handles.Label(nodes[i].transform.position + Vector3.up * 0.15f, label);
            }
        }
#endif
    }
}