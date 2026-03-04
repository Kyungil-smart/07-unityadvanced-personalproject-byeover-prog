using System;
using UnityEngine;

public sealed class NodeMonster : MonoBehaviour
{
    [Header("비주얼")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string missTrigger = "Miss";

    [Header("수명 및 판정")]
    [SerializeField, Tooltip("이 X 보다 더 왼쪽이면 자동 파괴")] private float despawnX = -30f;
    [SerializeField, Tooltip("히트 후 파괴 지연(초)")] private float hitReturnDelay = 0.05f;

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
        StopMovement();

        if (animator != null && HasParameter(hitTrigger))
            animator.SetTrigger(hitTrigger);

        if (gameManager != null)
            gameManager.Events.RaiseNodeSuccess();
    }

    public void Miss()
    {
        if (IsJudged) return;

        judgedMiss = true;
        StopMovement();

        if (animator != null && HasParameter(missTrigger))
            animator.SetTrigger(missTrigger);

        if (gameManager != null)
            gameManager.Events.RaiseNodeMiss();
    }

    private void StopMovement()
    {
        if (cachedMovers == null) return;
        
        foreach (var mover in cachedMovers)
        {
            if (mover != this) mover.enabled = false;
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

        // 판정선을 너무 많이 지나쳤을 때 자동 파괴
        if (transform.position.x <= despawnX)
        {
            Return();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsJudged) return;

        if (other.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.ApplyDamage(missDamage);
            Miss();
            Return();
        }
    }

    private void Return()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        
        if (returnToPool != null)
        {
            returnToPool.Invoke(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private bool HasParameter(string paramName)
    {
        if (string.IsNullOrEmpty(paramName) || animator == null || !animator.gameObject.activeInHierarchy || animator.runtimeAnimatorController == null) 
            return false;

        try
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName) return true;
            }
        }
        catch { return false; }
        return false;
    }
}