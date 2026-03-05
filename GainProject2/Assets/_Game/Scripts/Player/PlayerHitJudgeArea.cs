using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerHitJudgeArea : MonoBehaviour
{
    [Header("입력")]
    [SerializeField, Tooltip("판정 키")] private KeyCode hitKey = KeyCode.Space;

    private readonly List<NodeMonster> nodesInArea = new List<NodeMonster>(16);
    private readonly Dictionary<NodeMonster, int> contactCount = new Dictionary<NodeMonster, int>(32);

    private Transform self;

    private void Awake()
    {
        self = transform;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(hitKey)) return;

        CleanupNulls();
        if (nodesInArea.Count == 0) return;

        NodeMonster best = null;
        float bestDist = float.MaxValue;

        float centerX = self.position.x;

        for (int i = 0; i < nodesInArea.Count; i++)
        {
            var n = nodesInArea[i];
            if (n == null) continue;
            if (n.IsJudged) continue;

            float d = Mathf.Abs(n.transform.position.x - centerX);
            if (d < bestDist)
            {
                bestDist = d;
                best = n;
            }
        }

        if (best == null) return;

        RemoveNode(best);
        best.Hit();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var node = other.GetComponentInParent<NodeMonster>();
        if (node == null) return;
        if (node.IsJudged) return;

        if (contactCount.TryGetValue(node, out int c)) contactCount[node] = c + 1;
        else contactCount[node] = 1;

        if (!nodesInArea.Contains(node))
            nodesInArea.Add(node);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var node = other.GetComponentInParent<NodeMonster>();
        if (node == null) return;

        if (!contactCount.TryGetValue(node, out int c)) return;

        c -= 1;
        if (c > 0)
        {
            contactCount[node] = c;
            return;
        }

        contactCount.Remove(node);
        nodesInArea.Remove(node);

        if (!node.IsJudged)
            node.Miss();
    }

    private void RemoveNode(NodeMonster node)
    {
        nodesInArea.Remove(node);
        contactCount.Remove(node);
    }

    private void CleanupNulls()
    {
        for (int i = nodesInArea.Count - 1; i >= 0; i--)
        {
            var n = nodesInArea[i];
            if (n == null || !n.gameObject.activeInHierarchy)
            {
                nodesInArea.RemoveAt(i);
                if (n != null) contactCount.Remove(n);
            }
        }
    }
}