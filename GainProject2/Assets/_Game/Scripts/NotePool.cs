// UTF-8
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class NotePool : MonoBehaviour
    {
        [Header("프리팹")]
        [Tooltip("NoteMonster가 붙은 프리팹")]
        [SerializeField] private NoteMonster notePrefab;

        [Header("풀 설정")]
        [Tooltip("초기 풀 크기(노트 개수)")]
        [Min(8)]
        [SerializeField] private int initialSize = 64;

        [Tooltip("부족하면 자동 확장")]
        [SerializeField] private bool allowExpand = true;

        private NoteMonster[] _items;
        private int _count;

        public void Prewarm()
        {
            if (notePrefab == null) return;

            _items = new NoteMonster[initialSize];
            _count = initialSize;

            for (int i = 0; i < initialSize; i++)
            {
                var n = Instantiate(notePrefab, transform);
                n.gameObject.SetActive(false);
                n.SetPool(this);
                _items[i] = n;
            }
        }

        public NoteMonster Rent()
        {
            if (_items == null || _items.Length == 0) Prewarm();
            if (_items == null) return null;

            for (int i = 0; i < _items.Length; i++)
            {
                var n = _items[i];
                if (n != null && !n.gameObject.activeSelf)
                {
                    n.gameObject.SetActive(true);
                    return n;
                }
            }

            if (!allowExpand || notePrefab == null) return null;

            int newSize = Mathf.Max(_items.Length + 16, _items.Length * 2);
            var next = new NoteMonster[newSize];
            for (int i = 0; i < _items.Length; i++) next[i] = _items[i];

            for (int i = _items.Length; i < newSize; i++)
            {
                var n = Instantiate(notePrefab, transform);
                n.gameObject.SetActive(false);
                n.SetPool(this);
                next[i] = n;
            }

            _items = next;

            var rented = _items[_count];
            rented.gameObject.SetActive(true);
            _count = _items.Length;
            return rented;
        }

        public void Return(NoteMonster note)
        {
            if (note == null) return;
            note.gameObject.SetActive(false);
            note.transform.SetParent(transform, false);
        }
    }
}