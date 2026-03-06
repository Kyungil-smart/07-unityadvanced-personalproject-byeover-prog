using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class MainMenuUI : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button quitButton;

    [Header("옵션 패널")]
    [SerializeField] private GameObject optionPanel;

    [Header("씬 이름")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("세이브")]
    [SerializeField] private string saveFileName = "save.json";

    private void Awake()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (optionButton != null) optionButton.onClick.AddListener(OnOption);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        if (optionPanel != null) optionPanel.SetActive(false);

        // 이어하기 버튼: 세이브 파일 있을 때만 활성화
        if (continueButton != null)
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
            bool hasSave = System.IO.File.Exists(path);
            continueButton.interactable = hasSave;
        }

        Time.timeScale = 1f;
    }

    private void OnNewGame()
    {
        // 세이브 초기화
        string path = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnContinue()
    {
        // 세이브 유지한 채 게임 씬 로드
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(!optionPanel.activeSelf);
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}