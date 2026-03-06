using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public sealed class CreditUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RectTransform creditContent;
    [SerializeField] private TMP_Text creditText;

    [Header("설정")]
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private string returnSceneName = "MainMenu";

    [Header("크레딧 내용")]
    [SerializeField, TextArea(10, 30)] private string creditString =
        "리듬 도사\n\n\n" +
        "개발\n(Byeover)\n\n" +
        "기획\n(Byeover)\n\n" +
        "하율 일러스트\n(라리루(크몽))\n\n" +
        "배경 아트\n(Chat GPT+직선단순)\n\n" +
        "도트 아트\n(몬스터 : 리클(아트머그)\n\n" +
        "도트 아트\n(하율 : 식빵댕이(크몽)\n\n" +
        "UI/UX 아트\n(권스타)\n\n" +
        "음악\n(바이스원(Vaice))\n\n" +
        "프로그래밍\n(Byeover)\n\n\n" +
        "프로그래밍 도움 주신분\n(최완용)\n\n\n" +
        "감사합니다!";

    private float elapsed;
    private bool started;
    private float startY;

    private void Start()
    {
        if (creditText != null)
            creditText.text = creditString;

        if (creditContent != null)
        {
            startY = creditContent.anchoredPosition.y;
            // 화면 아래에서 시작
            creditContent.anchoredPosition = new Vector2(
                creditContent.anchoredPosition.x,
                startY - Screen.height
            );
        }
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;

        if (elapsed < startDelay) return;
        started = true;

        // 스크롤
        if (creditContent != null)
        {
            var pos = creditContent.anchoredPosition;
            pos.y += scrollSpeed * Time.unscaledDeltaTime;
            creditContent.anchoredPosition = pos;
        }
        
        if (Input.anyKeyDown)
        {
            ReturnToMenu();
        }
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(returnSceneName))
            SceneManager.LoadScene(returnSceneName);
    }
}