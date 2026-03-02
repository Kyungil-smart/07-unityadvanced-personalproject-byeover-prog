using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class GameResultUI : MonoBehaviour
{
    [Header("루트 오브젝트")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject clearTitle;
    [SerializeField] private GameObject failTitle;

    [Header("텍스트")]
    [SerializeField] private Text gradeText;

    [Header("버튼")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameManager.Instance;

        nextButton.onClick.AddListener(OnClickNext);
        retryButton.onClick.AddListener(OnClickRetry);
        menuButton.onClick.AddListener(OnClickMenu);

        resultPanel.SetActive(false);
    }

    public void ShowResult(bool isClear, float accuracy)
    {
        resultPanel.SetActive(true);
        clearTitle.SetActive(isClear);
        failTitle.SetActive(!isClear);
        nextButton.gameObject.SetActive(isClear);
        
        gradeText.text = CalculateGrade(accuracy);
    }

    private string CalculateGrade(float accuracy)
    {
        if (accuracy >= 0.95f) return "SS";
        if (accuracy >= 0.90f) return "S+";
        if (accuracy >= 0.80f) return "S";
        if (accuracy >= 0.70f) return "A";
        if (accuracy >= 0.50f) return "B";
        return "C";
    }

    private void OnClickNext()
    {
        resultPanel.SetActive(false);
        gameManager.Session.AdvanceToNextStage();
    }

    private void OnOnClickNext() => OnClickNext(); // 오타 방지용 브릿지

    private void OnClickRetry()
    {
        resultPanel.SetActive(false);
        gameManager.RestartCurrentStage();
    }

    private void OnClickMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); 
    }
}