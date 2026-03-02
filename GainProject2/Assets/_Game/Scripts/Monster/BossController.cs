using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Rhythm;

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

    private BossHitReact hitReact;

    private void Awake()
    {
        hitReact = GetComponent<BossHitReact>();
    }

    public void SetSurvivalFill01(float value01, bool clampToMin)
    {
        float v = Mathf.Clamp01(value01);
        if (clampToMin) v = Mathf.Max(v, minFillBeforeFinish);
        if (fakeHpFill != null) fakeHpFill.fillAmount = v;
    }

    public void LightningHit()
    {
        if (animator != null && HasParameter(hitTrigger))
            animator.SetTrigger(hitTrigger);

        if (hitReact != null)
            hitReact.Hit();
    }

    public void FinishKill()
    {
        if (fakeHpFill != null) fakeHpFill.fillAmount = 0f;

        if (animator != null && HasParameter(dieTrigger))
            animator.SetTrigger(dieTrigger);
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