using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerHitJudgeArea : MonoBehaviour
{
    [Header("입력")]
    [SerializeField, Tooltip("판정 키")] private KeyCode hitKey = KeyCode.Space;

    private readonly List<NodeMonster> nodesInArea = new List<NodeMonster>(16);
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

        var centerX = self.position.x;

        for (int i = 0; i < nodesInArea.Count; i++)
        {
            var n = nodesInArea[i];
            if (n == null) continue;
            if (n.IsJudged) continue;

            var d = Mathf.Abs(n.transform.position.x - centerX);
            if (d < bestDist)
            {
                bestDist = d;
                best = n;
            }
        }

        if (best == null) return;

        nodesInArea.Remove(best);
        best.Hit();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out NodeMonster node)) return;
        if (node.IsJudged) return;

        if (!nodesInArea.Contains(node))
            nodesInArea.Add(node);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out NodeMonster node)) return;

        nodesInArea.Remove(node);

        if (!node.IsJudged)
            node.Miss();
    }

    private void CleanupNulls()
    {
        for (int i = nodesInArea.Count - 1; i >= 0; i--)
        {
            var n = nodesInArea[i];
            if (n == null || !n.gameObject.activeInHierarchy)
                nodesInArea.RemoveAt(i);
        }
    }
}