using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerHeartUI : MonoBehaviour
{
    [Header("하트 이미지 배열")]
    [SerializeField] private Image[] heartImages;
    
    [Header("스프라이트")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("보너스 하트 색상")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color bonusColor = new Color(1f, 0.8f, 0.2f, 1f);

    [Header("참조")]
    [SerializeField] private PlayerHealth playerHealth;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            UpdateHearts();
        }
    }

    public void UpdateHearts()
    {
        if (playerHealth == null) return;

        int currentHp = playerHealth.CurrentHp;
        int maxHp = playerHealth.MaxHp;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHp)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].color = (i >= maxHp) ? bonusColor : normalColor;
                heartImages[i].gameObject.SetActive(true);
            }
            else if (i < maxHp)
            {
                heartImages[i].sprite = emptyHeartSprite;
                heartImages[i].color = normalColor;
                heartImages[i].gameObject.SetActive(true);
            }
            else
            {
                heartImages[i].gameObject.SetActive(false);
            }
        }
    }
}