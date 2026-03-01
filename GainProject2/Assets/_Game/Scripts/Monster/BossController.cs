using UnityEngine;
using UnityEngine.UI;

public sealed class BossController : MonoBehaviour
{
    [Header("보스 스탯")]
    [SerializeField] private int realHp = 99999;

    [Header("연출")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string dieTrigger = "Die";

    [Header("UI")]
    [SerializeField] private Image fakeHpFill;
    [SerializeField, Tooltip("곡 종료 전 최소로 남겨둘 HP 비율")] private float minFillBeforeFinish = 0.02f;

    public void SetSurvivalFill01(float value01, bool clampToMin)
    {
        var v = Mathf.Clamp01(value01);
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