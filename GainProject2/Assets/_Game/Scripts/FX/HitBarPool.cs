using UnityEngine;

//풀링 안쓰는게임 만들고 싶다.
//이 앞 지겨운 풀링의 시간이다.

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class HitBarPool : MonoBehaviour
    {
        [Header("프리팹")]
        [Tooltip("HitBarEffect가 붙은 프리팹")]
        [SerializeField] private HitBarEffect barPrefab;

        [Header("풀 설정")]
        [Tooltip("초기 풀 크기")]
        [Min(8)]
        [SerializeField] private int initialSize = 24;

        [Tooltip("부족하면 자동 확장")]
        [SerializeField] private bool allowExpand = true;

        private HitBarEffect[] _items;

        public void Prewarm()
        {
            if (barPrefab == null) return;

            _items = new HitBarEffect[initialSize];
            for (int i = 0; i < initialSize; i++)
            {
                var it = Instantiate(barPrefab, transform);
                it.gameObject.SetActive(false);
                it.SetPool(this);
                _items[i] = it;
            }
        }

        public HitBarEffect Rent()
        {
            if (_items == null || _items.Length == 0) Prewarm();
            if (_items == null) return null;

            for (int i = 0; i < _items.Length; i++)
            {
                var it = _items[i];
                if (it != null && !it.gameObject.activeSelf)
                {
                    it.gameObject.SetActive(true);
                    return it;
                }
            }

            if (!allowExpand || barPrefab == null) return null;

            int newSize = Mathf.Max(_items.Length + 8, _items.Length * 2);
            var next = new HitBarEffect[newSize];

            for (int i = 0; i < _items.Length; i++) next[i] = _items[i];

            for (int i = _items.Length; i < newSize; i++)
            {
                var it = Instantiate(barPrefab, transform);
                it.gameObject.SetActive(false);
                it.SetPool(this);
                next[i] = it;
            }

            _items = next;

            var rented = _items[_items.Length - 1];
            rented.gameObject.SetActive(true);
            return rented;
        }

        public void Return(HitBarEffect bar)
        {
            if (bar == null) return;
            bar.transform.SetParent(transform, false);
            bar.gameObject.SetActive(false);
        }
    }
}