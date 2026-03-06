using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerHeartUI : MonoBehaviour
{
    [Header("레이아웃")]
    [SerializeField] private int columns = 7;
    [SerializeField] private int rows = 2;
    [SerializeField] private float heartSize = 40f;
    [SerializeField] private float spacingX = 5f;
    [SerializeField] private float spacingY = 5f;

    [Header("기본 스프라이트")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("깨짐 애니메이션 (프레임 순서대로)")]
    [SerializeField] private Sprite[] breakFrames;

    [Header("회복 애니메이션 (프레임 순서대로)")]
    [SerializeField] private Sprite[] healFrames;

    [Header("애니메이션 속도")]
    [SerializeField] private float frameInterval = 0.08f;

    [Header("참조")]
    [SerializeField] private PlayerHealth playerHealth;

    private Image[] heartImages;
    private Coroutine[] heartAnims;
    private int lastKnownHp = -1;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        BuildHearts();
        RefreshAll();
    }

    private void OnEnable()
    {
        if (heartImages != null) RefreshAll();
    }

    private void BuildHearts()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        int total = columns * rows;
        heartImages = new Image[total];
        heartAnims = new Coroutine[total];

        for (int i = 0; i < total; i++)
        {
            int col = i % columns;
            int row = i / columns;

            GameObject go = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(heartSize, heartSize);
            rect.anchoredPosition = new Vector2(
                col * (heartSize + spacingX),
                -row * (heartSize + spacingY)
            );

            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = Color.white;
            if (fullHeartSprite != null) img.sprite = fullHeartSprite;

            heartImages[i] = img;
        }
    }

    private void Update()
    {
        if (playerHealth == null || heartImages == null) return;

        int hp = playerHealth.CurrentHp;
        if (hp == lastKnownHp) return;

        int prevHp = lastKnownHp;
        lastKnownHp = hp;

        // HP가 줄었으면 -> 깨짐 애니메이션
        if (hp < prevHp)
        {
            for (int i = prevHp - 1; i >= hp; i--)
            {
                if (i >= 0 && i < heartImages.Length)
                    PlayBreakAnim(i);
            }
        }
        // HP가 늘었으면 -> 회복 애니메이션
        else if (hp > prevHp)
        {
            for (int i = prevHp; i < hp; i++)
            {
                if (i >= 0 && i < heartImages.Length)
                    PlayHealAnim(i);
            }
        }
    }

    private void PlayBreakAnim(int index)
    {
        StopHeartAnim(index);
        heartAnims[index] = StartCoroutine(AnimateBreak(index));
    }

    private void PlayHealAnim(int index)
    {
        StopHeartAnim(index);
        heartAnims[index] = StartCoroutine(AnimateHeal(index));
    }

    private IEnumerator AnimateBreak(int index)
    {
        Image img = heartImages[index];
        if (img == null) yield break;

        img.gameObject.SetActive(true);

        // 프레임 재생
        if (breakFrames != null && breakFrames.Length > 0)
        {
            for (int f = 0; f < breakFrames.Length; f++)
            {
                if (breakFrames[f] != null)
                    img.sprite = breakFrames[f];
                yield return new WaitForSecondsRealtime(frameInterval);
            }
        }

        // 마지막 -> 빈 하트
        if (emptyHeartSprite != null) img.sprite = emptyHeartSprite;

        heartAnims[index] = null;
    }

    private IEnumerator AnimateHeal(int index)
    {
        Image img = heartImages[index];
        if (img == null) yield break;

        img.gameObject.SetActive(true);

        // 프레임 재생
        if (healFrames != null && healFrames.Length > 0)
        {
            for (int f = 0; f < healFrames.Length; f++)
            {
                if (healFrames[f] != null)
                    img.sprite = healFrames[f];
                yield return new WaitForSecondsRealtime(frameInterval);
            }
        }

        // 마지막 -> 꽉 찬 하트
        if (fullHeartSprite != null) img.sprite = fullHeartSprite;

        heartAnims[index] = null;
    }

    private void StopHeartAnim(int index)
    {
        if (heartAnims[index] != null)
        {
            StopCoroutine(heartAnims[index]);
            heartAnims[index] = null;
        }
    }

    // 전체 즉시 갱신
    public void RefreshAll()
    {
        if (playerHealth == null || heartImages == null) return;

        int hp = playerHealth.CurrentHp;
        int max = playerHealth.MaxHp;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            StopHeartAnim(i);

            if (i < hp)
            {
                heartImages[i].gameObject.SetActive(true);
                if (fullHeartSprite != null) heartImages[i].sprite = fullHeartSprite;
            }
            else if (i < max)
            {
                heartImages[i].gameObject.SetActive(true);
                if (emptyHeartSprite != null) heartImages[i].sprite = emptyHeartSprite;
            }
            else
            {
                heartImages[i].gameObject.SetActive(false);
            }
        }

        lastKnownHp = hp;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        RefreshAll();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}