using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public sealed class BackgroundScroller : MonoBehaviour
{
    [Header("스크롤 설정")]
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private Vector2 scrollDirection = Vector2.right;

    [Header("참조")]
    [SerializeField] private RawImage backgroundImage;

    private void Reset()
    {
        backgroundImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (backgroundImage == null) return;
        
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        Rect currentRect = backgroundImage.uvRect;
        currentRect.position += scrollDirection * scrollSpeed * Time.deltaTime;
        backgroundImage.uvRect = currentRect;
    }
}