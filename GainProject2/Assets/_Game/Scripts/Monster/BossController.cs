using UnityEngine;
using UnityEngine.UI;

public sealed class BossController : MonoBehaviour
{
    [Header("능력치")]
    [SerializeField, Tooltip("실제 체력 설정")] private int realHp = 99999;

    [Header("시각 효과")]
    [SerializeField, Tooltip("보스 애니메이터")] private Animator animator;
    [SerializeField, Tooltip("피격 트리거")] private string hitTrigger = "Hit";
    [SerializeField, Tooltip("사망 트리거")] private string dieTrigger = "Die";

    [Header("UI 연동")]
    [SerializeField, Tooltip("체력바 이미지")] private Image fakeHpFill;
    [SerializeField, Tooltip("최소 잔여 체력 비율")] private float minFillBeforeFinish = 0.02f;

    private int currentHp;

    private void Awake()
    {
        // 경고 해결: realHp 변수 사용
        currentHp = realHp;
    }

    public void SetSurvivalFill01(float value01, bool clampToMin)
    {
        float v = Mathf.Clamp01(value01);
        if (clampToMin) v = Mathf.Max(v, minFillBeforeFinish);
        if (fakeHpFill != null) fakeHpFill.fillAmount = v;
    }

    public void LightningHit()
    {
        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);
    }

    public void FinishKill()
    {
        if (fakeHpFill != null) fakeHpFill.fillAmount = 0f;
        if (animator != null && !string.IsNullOrEmpty(dieTrigger))
            animator.SetTrigger(dieTrigger);
    }
}