using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerNoteDamage : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("피격 처리 대상(비우면 자기 자신에서 IDamageable을 찾습니다)")]
    [SerializeField] private MonoBehaviour damageTarget;

    [Header("필터")]
    [Tooltip("노트(몬스터) 레이어 마스크")]
    [SerializeField] private LayerMask noteMask;

    [Header("디버그")]
    [Tooltip("피격 로그 출력")]
    [SerializeField] private bool debugLog = true;

    private IDamageable damageable;

    private void Awake()
    {
        if (damageTarget != null)
            damageable = damageTarget as IDamageable;

        if (damageable == null)
            damageable = GetComponent<IDamageable>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryApplyDamage(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyDamage(other);
    }

    private void TryApplyDamage(Collider2D col)
    {
        if (damageable == null) return;
        if (((1 << col.gameObject.layer) & noteMask.value) == 0) return;

        if (!col.TryGetComponent<INoteDamageProvider>(out var provider))
            return;

        int dmg = provider.Damage;
        if (dmg <= 0) return;

        damageable.ApplyDamage(dmg);

        if (debugLog)
            Debug.Log($"[PlayerNoteDamage] HIT by {col.name}, dmg={dmg}", this);
    }
}