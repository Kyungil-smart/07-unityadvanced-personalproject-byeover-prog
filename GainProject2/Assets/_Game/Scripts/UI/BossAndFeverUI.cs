using UnityEngine;
using UnityEngine.UI;

public sealed class BossAndFeverUI : MonoBehaviour
{
    [Header("보스 체력바")]
    [SerializeField] private Image bossHpFill;
    [SerializeField, Tooltip("피니셔 전 남겨둘 최소 체력 비율")] private float minHpRatio = 0.02f;

    [Header("피버 타임")]
    [SerializeField] private CanvasGroup feverGroup;
    [SerializeField] private float feverFadeSpeed = 5f;

    private bool isFeverActive;

    public void UpdateBossHpByProgress(float songProgress)
    {
        if (bossHpFill == null) return;
        
        float targetFill = Mathf.Lerp(1f, minHpRatio, songProgress);
        bossHpFill.fillAmount = targetFill;
    }

    public void SetBossHpZero()
    {
        if (bossHpFill != null)
        {
            bossHpFill.fillAmount = 0f;
        }
    }

    public void SetFeverState(bool state)
    {
        isFeverActive = state;
    }

    private void Update()
    {
        if (feverGroup == null) return;

        float targetAlpha = isFeverActive ? 1f : 0f;
        feverGroup.alpha = Mathf.MoveTowards(feverGroup.alpha, targetAlpha, Time.deltaTime * feverFadeSpeed);
    }
}