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
        
        // 루트에 SpriteRenderer가 있고, 자식 Visual에도 SpriteRenderer가 있으면
        // 루트의 SpriteRenderer를 끈다 (Visual만 보이게)
        FixDuplicateSpriteRenderers();

        if (hpBarUI == null)
            hpBarUI = FindObjectOfType<BossHpBarUI>(true);

        SyncUI(true);
    }
    
    private void FixDuplicateSpriteRenderers()
    {
        var rootSR = GetComponent<SpriteRenderer>();
        if (rootSR == null) return;

        // 자식에서 Visual SpriteRenderer 찾기
        SpriteRenderer childSR = null;
        var childSRs = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in childSRs)
        {
            if (sr != rootSR) // 루트가 아닌 자식
            {
                childSR = sr;
                break;
            }
        }

        if (childSR == null) return; // 자식에 SR이 없으면 건드리지 않음

        // 루트 SpriteRenderer 비활성화 (2마리 문제 해결)
        rootSR.enabled = false;

        // BossHitReact가 루트 SR을 참조하고 있으면 자식 SR로 교체
        var hitReact = GetComponent<_Game.Scripts.Rhythm.BossHitReact>();
        if (hitReact != null)
        {
            hitReact.OverrideBossSprite(childSR);
        }
    }

    // 런타임에서 HP바 UI 연결
    public void SetHpBarUI(BossHpBarUI ui)
    {
        hpBarUI = ui;
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