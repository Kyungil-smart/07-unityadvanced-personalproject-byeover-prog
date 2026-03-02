using System;
using UnityEngine;

public sealed class NodeMonster : MonoBehaviour
{
    [Header("비주얼")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string missTrigger = "Miss";

    [Header("이동")]
    [SerializeField] private BeatStepMover stepMover;

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

    public bool IsJudged => judgedHit || judgedMiss;

    public void Initialize(GameManager manager, Action<NodeMonster> onReturn)
    {
        gameManager = manager;
        returnToPool = onReturn;
        missDamage = manager != null && manager.Settings != null ? manager.Settings.MissDamage : 1;

        if (stepMover != null && manager != null)
            stepMover.Initialize(manager.Events, manager.Settings);

        judgedHit = false;
        judgedMiss = false;
        returnTimer = 0f;
    }

    public void Spawn(Vector3 position, Sprite nodeSprite)
    {
        transform.position = position;

        if (spriteRenderer != null && nodeSprite != null)
            spriteRenderer.sprite = nodeSprite;

        judgedHit = false;
        judgedMiss = false;
        returnTimer = 0f;
        
        gameObject.SetActive(true);
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void Hit()
    {
        if (IsJudged) return;

        judgedHit = true;
        returnTimer = hitReturnDelay;

        if (animator != null && HasParameter(hitTrigger))
            animator.SetTrigger(hitTrigger);

        if (gameManager != null)
            gameManager.Events.RaiseNodeSuccess();
    }

    public void Miss()
    {
        if (IsJudged) return;

        judgedMiss = true;

        if (animator != null && HasParameter(missTrigger))
            animator.SetTrigger(missTrigger);

        if (gameManager != null)
            gameManager.Events.RaiseNodeMiss();
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

        if (other.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.ApplyDamage(missDamage);
            Return();
        }
    }

    private void Return()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        returnToPool?.Invoke(this);
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