using UnityEngine;
using GnalIhu.Rhythm;

[DisallowMultipleComponent]
public sealed class HitLineGate2D : MonoBehaviour
{
    [Header("게이트")]
    [SerializeField, Tooltip("레인 번호(1~4)")] private int lane = 1;
    [SerializeField, Tooltip("스포너 참조(비어있으면 자동 탐색)")] private CsvStageSpawner spawner;

    private void Awake()
    {
        lane = Mathf.Clamp(lane, 1, 4);
        if (spawner == null) spawner = FindFirstObjectByType<CsvStageSpawner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spawner == null) return;

        var node = other.GetComponentInParent<NodeMonster>();
        if (node == null) return;

        spawner.NotifyPassedHitLine(lane, node);
    }
}