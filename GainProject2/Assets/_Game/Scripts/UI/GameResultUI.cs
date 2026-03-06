using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public sealed class GameResultUI : MonoBehaviour
{
    [Header("루트")]
    [SerializeField] private GameObject resultPanel;

    [Header("클리어")]
    [SerializeField] private GameObject clearGroup;
    [SerializeField] private TMP_Text clearText;

    [Header("패배")]
    [SerializeField] private GameObject failGroup;
    [SerializeField] private TMP_Text failText;

    [Header("버튼")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    [Header("메인메뉴 씬")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameManager gameManager;

    private void Awake()
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenu);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager != null && gameManager.Events != null)
        {
            gameManager.Events.GameStateChanged += OnGameStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null && gameManager.Events != null)
        {
            gameManager.Events.GameStateChanged -= OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.StageClear:
                ShowClear();
                break;
            case GameState.StageFailed:
                ShowFail();
                break;
        }
    }

    private void ShowClear()
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (clearGroup != null) clearGroup.SetActive(true);
        if (failGroup != null) failGroup.SetActive(false);
        if (clearText != null) clearText.text = "스테이지 클리어!";
    }

    private void ShowFail()
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (clearGroup != null) clearGroup.SetActive(false);
        if (failGroup != null) failGroup.SetActive(true);
        if (failText != null) failText.text = "패배";

        Time.timeScale = 0f;
    }

    private void OnRetry()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        Time.timeScale = 1f;

        if (gameManager != null)
            gameManager.RestartCurrentStage();
    }

    private void OnMenu()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}