using UnityEngine;
using UnityEngine.Events;

public sealed class PlayerHealth : MonoBehaviour
{
    [Header("체력")]
    [SerializeField] private int maxHp = 5;

    [Header("이벤트")]
    [SerializeField] private UnityEvent onDamaged;
    [SerializeField] private UnityEvent onDead;

    private int currentHp;

    public int CurrentHp => currentHp;

    private void Awake()
    {
        currentHp = Mathf.Max(1, maxHp);
    }

    public void ResetHp()
    {
        currentHp = Mathf.Max(1, maxHp);
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0) return;

        currentHp = Mathf.Max(0, currentHp - amount);
        onDamaged?.Invoke();

        if (currentHp <= 0)
            onDead?.Invoke();
    }
}