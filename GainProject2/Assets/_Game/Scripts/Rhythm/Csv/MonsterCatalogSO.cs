using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "리듬 도사/리듬/CSV/몬스터 카탈로그", fileName = "SO_MonsterCatalog")]
public sealed class MonsterCatalogSO : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        [Tooltip("CSV에서 쓰는 몬스터 ID")] public string id;
        [Tooltip("스폰할 프리팹")] public GameObject prefab;
    }

    [Header("매핑")]
    [SerializeField] private List<Entry> entries = new List<Entry>(16);

    private Dictionary<string, GameObject> _map;

    public bool TryGetPrefab(string id, out GameObject prefab)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            prefab = null;
            return false;
        }

        EnsureIndex();
        return _map.TryGetValue(id.Trim(), out prefab) && prefab != null;
    }

    private void EnsureIndex()
    {
        if (_map != null) return;

        _map = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (string.IsNullOrWhiteSpace(e.id)) continue;
            if (e.prefab == null) continue;

            string key = e.id.Trim();
            _map[key] = e.prefab;
        }
    }

    private void OnValidate()
    {
        _map = null;
    }
}