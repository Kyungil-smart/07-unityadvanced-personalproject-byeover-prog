using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour
{
    [Header("체력")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int absoluteMaxHp = 14;

    [Header("이벤트")]
    [SerializeField] private UnityEvent onDamaged;
    [SerializeField] private UnityEvent onHealed;
    [SerializeField] private UnityEvent onDead;

    private int currentHp;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    private void Awake()
    {
        currentHp = Mathf.Max(1, maxHp);
    }

    public void ResetHp()
    {
        currentHp = Mathf.Max(1, maxHp);
        onHealed?.Invoke();
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0 || currentHp <= 0) return;

        currentHp = Mathf.Max(0, currentHp - amount);
        onDamaged?.Invoke();

        if (currentHp <= 0)
        {
            onDead?.Invoke();
        }
    }

    public void Heal(int amount, bool allowBonus = false)
    {
        if (amount <= 0 || currentHp <= 0) return;

        int limit = allowBonus ? absoluteMaxHp : maxHp;
        if (currentHp >= limit) return;

        currentHp = Mathf.Min(limit, currentHp + amount);
        onHealed?.Invoke();
    }
}