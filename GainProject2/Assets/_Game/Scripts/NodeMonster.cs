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
    [SerializeField, Tooltip("메인 콜라이더 (판정 + 미스 후 데미지 겸용)")]
    private BoxCollider2D mainCollider;

    [Header("수명")]
    [SerializeField] private float despawnX = -30f;

    [Header("히트 회수")]
    [SerializeField] private float hitReturnDelay = 0.05f;

    [Header("미스 후 이동")]
    [SerializeField] private float missSpeed = 8f;
    [SerializeField] private Vector3 missDirection = Vector3.left;

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
        if (mainCollider == null) mainCollider = GetComponent<BoxCollider2D>();
        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();
        judgedHit = false;
        judgedMiss = false;
        returnTimer = 0f;
        
        if (mainCollider != null) mainCollider.enabled = true;
    }

    public void Initialize(GameManager manager, Action<NodeMonster> onReturn)
    {
        gameManager = manager;
        returnToPool = onReturn;
        missDamage = manager != null && manager.Settings != null ? manager.Settings.MissDamage : 1;

        judgedHit = false;
        judgedMiss = false;
        returnTimer = 0f;
        
        if (mainCollider != null) mainCollider.enabled = true;

        cachedMovers ??= GetComponents<MonoBehaviour>();
    }

    public void Spawn(Vector3 position, Sprite sprite)
    {
        transform.position = position;

        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;

        judgedHit = false;
        judgedMiss = false;
        returnTimer = 0f;
        
        if (mainCollider != null) mainCollider.enabled = true;

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

        Debug.Log($"[NodeMonster] HIT! {gameObject.name}");
    }

    public void Miss()
    {
        if (IsJudged) return;
        EnsureInitialized();

        judgedMiss = true;

        // Mover 정지 → 미스 후 플레이어 방향으로 이동
        StopMovement();

        AnimatorParamUtil.TrySetTrigger(animator, missTrigger);

        if (gameManager != null)
            gameManager.Events.RaiseNodeMiss();

        Debug.Log($"[NodeMonster] MISS! {gameObject.name}");
    }

    private void EnsureInitialized()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (missDamage <= 0)
            missDamage = gameManager != null && gameManager.Settings != null ? gameManager.Settings.MissDamage : 1;
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

        if (judgedMiss)
        {
            // 미스 후 플레이어 방향으로 이동
            transform.position += missDirection.normalized * (missSpeed * Time.deltaTime);

            if (transform.position.x <= despawnX)
                Return();
            return;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 미스 상태가 아니면 데미지 안 줌
        if (!judgedMiss) return;

        if (other.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.ApplyDamage(missDamage);
            Debug.Log($"[NodeMonster] 플레이어 데미지! dmg={missDamage}");
            Return();
        }
    }

    private void Return()
    {
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false);

        if (returnToPool != null) returnToPool.Invoke(this);
        else Destroy(gameObject);
    }
}