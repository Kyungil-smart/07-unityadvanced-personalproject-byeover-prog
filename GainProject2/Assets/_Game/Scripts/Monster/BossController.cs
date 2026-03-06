using UnityEngine;
using _Game.Scripts.UI;

public sealed class BossController : MonoBehaviour
{
    [SerializeField] private int maxHp = 99999;
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string dieTrigger = "Die";
    [SerializeField] private BossHpBarUI hpBarUI;

    private int currentHp;

    private void Awake()
    {
        currentHp = maxHp;
        FixDuplicateSpriteRenderers();
        if (hpBarUI == null) hpBarUI = FindFirstObjectByType<BossHpBarUI>(FindObjectsInactive.Include);
        SyncUI();
    }

    private void FixDuplicateSpriteRenderers()
    {
        var rootSR = GetComponent<SpriteRenderer>();
        if (rootSR == null) return;
        SpriteRenderer childSR = null;
        var all = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in all) { if (sr != rootSR) { childSR = sr; break; } }
        if (childSR == null) return;
        rootSR.enabled = false;
        var hitReact = GetComponent<_Game.Scripts.Rhythm.BossHitReact>();
        if (hitReact != null) hitReact.OverrideBossSprite(childSR);
    }

    public void SetHpBarUI(BossHpBarUI ui) { hpBarUI = ui; SyncUI(); }

    public void ApplyDamage(int damage)
    {
        currentHp = Mathf.Max(1, currentHp - damage);
        AnimatorParamUtil.TrySetTrigger(animator, hitTrigger);
        SyncUI();
    }

    private void SyncUI()
    {
        if (hpBarUI == null) return;
        hpBarUI.SetNormalized(Mathf.Clamp01((float)currentHp / maxHp));
    }

    public void LightningHit()
    {
        AnimatorParamUtil.TrySetTrigger(animator, hitTrigger);
    }

    public void FinishKill()
    {
        currentHp = 0;
        if (hpBarUI != null) hpBarUI.SetNormalized(0f);
        AnimatorParamUtil.TrySetTrigger(animator, dieTrigger);
    }
}