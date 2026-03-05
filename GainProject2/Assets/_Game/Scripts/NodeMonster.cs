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

    [Header("미스 후 이동")]
    [SerializeField, Tooltip("미스 후 플레이어 방향으로 이동 속도")] private float missSpeed = 8f;
    [SerializeField, Tooltip("미스 후 이동 방향 (보통 Vector3.left)")] private Vector3 missDirection = Vector3.left;

    private GameManager gameManager;
    private Action<NodeMonster> returnToPool;
    private int missDamage;

    private bool judgedHit;
    private bool judgedMiss;
    private float returnTimer;
    private MonoBehaviour[] cachedMovers;

    public bool IsJudged => judgedHit || judgedMiss;

    private void Awake()
    {
        cachedMovers = GetComponents<MonoBehaviour>();
        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();

        if (damageTriggerCollider != null)
            damageTriggerCollider.enabled = false;

        judgedHit = false;
        judgedMiss = false;
        returnTimer = 0f;
    }

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

        cachedMovers = cachedMovers != null ? cachedMovers : GetComponents<MonoBehaviour>();
    }

    public void Spawn(Vector3 position, Sprite sprite)
    {
        transform.position = position;

        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;

        if (damageTriggerCollider != null)
            damageTriggerCollider.enabled = false;

        judgedHit = false;
        judgedMiss = false;
        returnTimer = 0f;

        if (cachedMovers != null)
        {
            foreach (var mover in cachedMovers)
            {
                if (mover != this && mover != null)
                    mover.enabled = true;
            }
        }

        gameObject.SetActive(true);
    }

    public void Hit()
    {
        if (IsJudged) return;

        EnsureInitialized();

        judgedHit = true;
        returnTimer = hitReturnDelay;
        StopMovement();

        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
            AnimatorParamUtil.TrySetTrigger(animator, hitTrigger);

        if (gameManager != null)
            gameManager.Events.RaiseNodeSuccess();
    }

    public void Miss()
    {
        if (IsJudged) return;

        EnsureInitialized();

        judgedMiss = true;

        // 기존 Mover 정지 → 미스 전용 이동으로 전환
        StopMovement();

        // 데미지 콜라이더 활성화
        if (damageTriggerCollider != null)
            damageTriggerCollider.enabled = true;

        AnimatorParamUtil.TrySetTrigger(animator, missTrigger);

        // NodeMiss 이벤트 발생
        if (gameManager != null)
            gameManager.Events.RaiseNodeMiss();
    }

    private void EnsureInitialized()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (missDamage <= 0) missDamage = gameManager != null && gameManager.Settings != null ? gameManager.Settings.MissDamage : 1;
    }

    private void StopMovement()
    {
        if (cachedMovers == null) return;

        foreach (var mover in cachedMovers)
        {
            if (mover != this && mover != null)
                mover.enabled = false;
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

        // 미스 후 플레이어 방향으로 계속 이동
        transform.position += missDirection.normalized * (missSpeed * Time.deltaTime);

        if (transform.position.x <= despawnX)
            Return();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!judgedMiss) return;
        if (damageTriggerCollider == null || !damageTriggerCollider.enabled) return;

        // 플레이어 체력 감소
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

        if (returnToPool != null) returnToPool.Invoke(this);
        else Destroy(gameObject);
    }
}