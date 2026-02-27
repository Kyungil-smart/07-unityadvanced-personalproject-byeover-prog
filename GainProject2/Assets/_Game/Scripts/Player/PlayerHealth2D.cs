// UTF-8
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealth2D : MonoBehaviour
    {
        [Header("체력")]
        [Tooltip("최대 HP")]
        [Min(1)]
        [SerializeField] private int maxHp = 5;

        [Tooltip("현재 HP(런타임)")]
        [SerializeField] private int currentHp = 5;

        [Header("디버그")]
        [Tooltip("피해를 받을 때 콘솔 로그 출력")]
        [SerializeField] private bool logOnDamage = true;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;

        private void Awake()
        {
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        }

        public void Damage(int amount)
        {
            if (amount <= 0) return;
            if (currentHp <= 0) return;

            int before = currentHp;
            currentHp = Mathf.Max(0, currentHp - amount);

            if (logOnDamage)
                Debug.Log($"[PlayerHealth2D] Damage={amount}, HP {before} -> {currentHp}", this);

            if (currentHp == 0)
                Debug.Log("[PlayerHealth2D] Dead", this);
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;
            if (currentHp <= 0) return;

            int before = currentHp;
            currentHp = Mathf.Min(maxHp, currentHp + amount);

            Debug.Log($"[PlayerHealth2D] Heal={amount}, HP {before} -> {currentHp}", this);
        }
    }
}