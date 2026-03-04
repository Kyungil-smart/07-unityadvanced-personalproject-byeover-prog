using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI
{
    [DisallowMultipleComponent]
    public sealed class BossHpBarUI : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private Image fillImage;

        [Header("옵션")]
        [Tooltip("0~1 범위를 자동으로 고정")]
        [SerializeField] private bool clamp01 = true;

        public void SetNormalized(float value)
        {
            if (fillImage == null) return;
            if (clamp01) value = Mathf.Clamp01(value);
            fillImage.fillAmount = value;
        }

        public void SetValue(float current, float max)
        {
            if (max <= 0f) { SetNormalized(0f); return; }
            SetNormalized(current / max);
        }
    }
}