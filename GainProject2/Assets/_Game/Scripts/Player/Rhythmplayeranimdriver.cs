using UnityEngine;

[DisallowMultipleComponent]
public sealed class RhythmPlayerAnimDriver : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private KeyCode rhythmKey = KeyCode.Space;
    [SerializeField] private string attackTriggerName = "Attack";

    private int attackHash;
    private bool hasParam;

    private void Awake()
    {
        if (playerAnimator == null) playerAnimator = GetComponentInChildren<Animator>();
        attackHash = Animator.StringToHash(attackTriggerName);
        hasParam = HasParameter(attackTriggerName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(rhythmKey) && playerAnimator != null && hasParam)
            playerAnimator.SetTrigger(attackHash);
    }

    private bool HasParameter(string paramName)
    {
        if (string.IsNullOrEmpty(paramName) || playerAnimator == null || playerAnimator.runtimeAnimatorController == null) return false;
        foreach (var p in playerAnimator.parameters) { if (p.name == paramName) return true; }
        return false;
    }
}