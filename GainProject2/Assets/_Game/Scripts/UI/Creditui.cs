using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public sealed class CreditUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField, Tooltip("크레딧 텍스트 (RectTransform으로 스크롤)")]
    private RectTransform creditContent;

    [SerializeField, Tooltip("크레딧 텍스트 컴포넌트")]
    private TMP_Text creditText;

    [Header("스크롤 설정")]
    [SerializeField, Tooltip("스크롤 속도 (픽셀/초)")] private float scrollSpeed = 60f;
    [SerializeField, Tooltip("시작 전 대기 시간(초)")] private float startDelay = 1f;
    [SerializeField, Tooltip("끝난 후 대기 시간(초)")] private float endDelay = 2f;

    [Header("종료 동작")]
    [SerializeField, Tooltip("크레딧 종료 후 메인 메뉴 씬으로 이동할지")]
    private bool returnToMainMenuOnFinish = false;
    [SerializeField, Tooltip("메인 메뉴 씬 이름")]
    private string mainMenuSceneName = "MainMenu";

    [Header("크레딧 내용 (Inspector에서 직접 입력)")]
    [SerializeField, TextArea(10, 30)]
    private string creditString = @"
<size=48><b>그날이후</b></size>

<size=36>개발</size>
정승우

<size=36>프로그래밍</size>
정승우

<size=36>아트</size>
(아트 담당)

<size=36>음악</size>
(음악 담당)

<size=36>기획</size>
(기획 담당)

<size=36>Special Thanks</size>
(감사한 분들)


<size=24>Powered by Unity</size>

<size=20>© 2026 All Rights Reserved</size>
";

    private Coroutine scrollCoroutine;
    private float startY;
    private float targetY;

    private void OnEnable()
    {
        if (creditText != null && !string.IsNullOrWhiteSpace(creditString))
            creditText.text = creditString;

        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
        scrollCoroutine = StartCoroutine(ScrollCredit());
    }

    private void OnDisable()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            FinishCredit();
        }
    }

    private IEnumerator ScrollCredit()
    {
        if (creditContent == null) yield break;
        
        yield return null;

        startY = creditContent.anchoredPosition.y;
        float contentHeight = creditContent.rect.height;
        float parentHeight = 0f;

        var parent = creditContent.parent as RectTransform;
        if (parent != null) parentHeight = parent.rect.height;

        targetY = startY + contentHeight + parentHeight;

        creditContent.anchoredPosition = new Vector2(creditContent.anchoredPosition.x, startY);

        yield return new WaitForSecondsRealtime(startDelay);

        float currentY = startY;

        while (currentY < targetY)
        {
            currentY += scrollSpeed * Time.unscaledDeltaTime;
            creditContent.anchoredPosition = new Vector2(creditContent.anchoredPosition.x, currentY);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(endDelay);

        FinishCredit();
    }

    private void FinishCredit()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }

        if (returnToMainMenuOnFinish && !string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}