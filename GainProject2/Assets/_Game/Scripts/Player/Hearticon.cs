using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개별 하트 아이콘. 깨짐/생성 4프레임 스프라이트 애니메이션.
/// 각 하트 Image에 붙여서 사용.
/// </summary>
[DisallowMultipleComponent]
public sealed class HeartIcon : MonoBehaviour
{
    [Header("스프라이트")]
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;

    [Header("깨짐 애니메이션 (4프레임)")]
    [SerializeField] private Sprite[] breakFrames;

    [Header("생성 애니메이션 (4프레임)")]
    [SerializeField] private Sprite[] createFrames;

    [Header("속도")]
    [SerializeField] private float frameInterval = 0.08f;

    private Image image;
    private Coroutine animCoroutine;
    private bool isFull;

    public bool IsFull => isFull;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    /// <summary>즉시 풀 하트로 (애니메이션 없이)</summary>
    public void SetFull()
    {
        StopAnim();
        isFull = true;
        if (image != null && fullSprite != null) image.sprite = fullSprite;
        gameObject.SetActive(true);
    }

    /// <summary>즉시 빈 하트로 (애니메이션 없이)</summary>
    public void SetEmpty()
    {
        StopAnim();
        isFull = false;
        if (image != null && emptySprite != null) image.sprite = emptySprite;
        gameObject.SetActive(true);
    }

    /// <summary>깨짐 애니메이션 → 빈 하트</summary>
    public void PlayBreak()
    {
        if (!isFull) return;
        StopAnim();
        animCoroutine = StartCoroutine(AnimateBreak());
    }

    /// <summary>생성 애니메이션 → 풀 하트</summary>
    public void PlayCreate()
    {
        if (isFull) return;
        StopAnim();
        animCoroutine = StartCoroutine(AnimateCreate());
    }

    /// <summary>즉시 비활성화</summary>
    public void Hide()
    {
        StopAnim();
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateBreak()
    {
        isFull = false;

        if (breakFrames != null && breakFrames.Length > 0)
        {
            for (int i = 0; i < breakFrames.Length; i++)
            {
                if (breakFrames[i] != null && image != null)
                    image.sprite = breakFrames[i];
                yield return new WaitForSecondsRealtime(frameInterval);
            }
        }

        if (image != null && emptySprite != null)
            image.sprite = emptySprite;

        animCoroutine = null;
    }

    private IEnumerator AnimateCreate()
    {
        isFull = true;
        gameObject.SetActive(true);

        if (createFrames != null && createFrames.Length > 0)
        {
            for (int i = 0; i < createFrames.Length; i++)
            {
                if (createFrames[i] != null && image != null)
                    image.sprite = createFrames[i];
                yield return new WaitForSecondsRealtime(frameInterval);
            }
        }

        if (image != null && fullSprite != null)
            image.sprite = fullSprite;

        animCoroutine = null;
    }

    private void StopAnim()
    {
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }
    }
}