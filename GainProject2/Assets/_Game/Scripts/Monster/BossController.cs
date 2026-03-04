using UnityEngine;
using _Game.Scripts.UI;

public sealed class BossController : MonoBehaviour
{
    [Header("능력치")]
    [SerializeField, Tooltip("실제 체력 설정")] private int realHp = 99999;

    [Header("시각 효과")]
    [SerializeField, Tooltip("보스 애니메이터")] private Animator animator;
    [SerializeField, Tooltip("피격 트리거")] private string hitTrigger = "Hit";
    [SerializeField, Tooltip("사망 트리거")] private string dieTrigger = "Die";

    [Header("UI 연동")]
    [SerializeField, Tooltip("보스 체력바 UI")] private BossHpBarUI hpBarUI;
    [SerializeField, Tooltip("최소 잔여 체력 비율")] private float minFillBeforeFinish = 0.02f;

    private int currentHp;

    public int CurrentHp => currentHp;
    public int RealHp => realHp;

    private void Awake()
    {
        currentHp = Mathf.Max(1, realHp);

        if (hpBarUI == null)
        {
            hpBarUI = FindObjectOfType<BossHpBarUI>(true);
        }

        SyncUI(true);
    }

    public void ApplyDamage(int damage)
    {
        if (damage <= 0) return;

        currentHp = Mathf.Max(0, currentHp - damage);

        LightningHit();

        if (currentHp <= 0)
        {
            FinishKill();
            return;
        }

        SyncUI(true);
    }

    private void SyncUI(bool clampToMin)
    {
        if (hpBarUI == null) return;

        float v = (float)currentHp / Mathf.Max(1, realHp);
        v = Mathf.Clamp01(v);
        if (clampToMin) v = Mathf.Max(v, minFillBeforeFinish);

        hpBarUI.SetNormalized(v);
    }

    public void LightningHit()
    {
        AnimatorParamUtil.TrySetTrigger(animator, hitTrigger);
    }

    public void FinishKill()
    {
        if (hpBarUI != null) hpBarUI.SetNormalized(0f);
        AnimatorParamUtil.TrySetTrigger(animator, dieTrigger);
    }
}