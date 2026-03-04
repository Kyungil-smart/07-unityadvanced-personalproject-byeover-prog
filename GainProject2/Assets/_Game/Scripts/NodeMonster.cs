using System;
using UnityEngine;

public sealed class NodeMonster : MonoBehaviour
{
    [Header("비주얼")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string missTrigger = "Miss";

    [Header("콜라이더")]
    [SerializeField, Tooltip("미스 후 데미지용 트리거 콜라이더")] private BoxCollider2D damageTriggerCollider;

    [Header("수명")]
    [SerializeField, Tooltip("이 X 보다 더 왼쪽이면 자동 회수")] private float despawnX = -30f;

    [Header("히트 회수")]
    [SerializeField, Tooltip("히트 후 회수 지연(초)")] private float hitReturnDelay = 0.05f;

    private GameManager gameManager;
    private Action<NodeMonster> returnToPool;
    private int missDamage;

    private bool judgedHit;
    private bool judgedMiss;
    private float returnTimer;
    private MonoBehaviour[] cachedMovers;

    public bool IsJudged => judgedHit || judgedMiss;

    public void Initialize(GameManager manager, Action<NodeMonster> onReturn)
    {
        gameManager = manager;
        returnToPool = onReturn;
        missDamage = manager != null && manager.Settings != null ? manager.Settings.MissDamage : 1;

        if (damageTriggerCollider != null)
            damageTriggerCollider.enabled = false;

        judgedHit = false;
        judgedMiss = false;
        returnTimer = 0f;
        
        cachedMovers = GetComponents<MonoBehaviour>();
    }

    public void Hit()
    {
        if (IsJudged) return;

        judgedHit = true;
        returnTimer = hitReturnDelay;
        StopMovement(); // 타격 시 즉시 멈춤

        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);

        if (gameManager != null) gameManager.Events.RaiseNodeSuccess();
    }

    public void Miss()
    {
        if (IsJudged) return;

        judgedMiss = true;
        StopMovement(); // 미스 시 즉시 멈춤

        if (damageTriggerCollider != null)
            damageTriggerCollider.enabled = true;

        if (animator != null && !string.IsNullOrEmpty(missTrigger))
            animator.SetTrigger(missTrigger);

        if (gameManager != null) gameManager.Events.RaiseNodeMiss();
    }

    private void StopMovement()
    {
        if (cachedMovers == null) return;
        foreach (var mover in cachedMovers)
        {
            if (mover != this) mover.enabled = false; // 다른 이동 스크립트를 전부 끕니다
        }
    }

    private void Update()
    {
        if (judgedHit)
        {
            returnTimer -= Time.deltaTime;
            if (returnTimer <= 0f) Return();
            return;
        }

        if (!judgedMiss) return;

        if (transform.position.x <= despawnX)
            Return();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!judgedMiss) return;
        if (damageTriggerCollider == null || !damageTriggerCollider.enabled) return;

        if (other.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.ApplyDamage(missDamage);
            Return();
        }
    }

    private void Return()
    {
        if (!gameObject.activeSelf) return;

        if (damageTriggerCollider != null)
            damageTriggerCollider.enabled = false;

        gameObject.SetActive(false);

        // 풀링이 없으면 완전히 삭제하여 좀비 몬스터 방지
        if (returnToPool != null)
            returnToPool.Invoke(this);
        else
            Destroy(gameObject);
    }
}