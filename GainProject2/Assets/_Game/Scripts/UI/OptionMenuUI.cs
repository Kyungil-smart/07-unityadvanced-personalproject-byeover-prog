using UnityEngine;
using UnityEngine.UI;

public sealed class OptionMenuUI : MonoBehaviour
{
    [Header("옵션 패널")]
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject warningPanel;

    [Header("컨트롤 UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Dropdown difficultyDropdown;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Dropdown frameRateDropdown;

    [Header("버튼")]
    [SerializeField] private Button applyDifficultyButton;
    [SerializeField] private Button cancelDifficultyButton;

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameManager.Instance;

        optionPanel.SetActive(false);
        warningPanel.SetActive(false);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);

        applyDifficultyButton.onClick.AddListener(ApplyDifficultyAndRestart);
        cancelDifficultyButton.onClick.AddListener(CancelDifficultyChange);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptionMenu();
        }
    }

    public void ToggleOptionMenu()
    {
        if (warningPanel.activeSelf) return; 

        bool willShow = !optionPanel.activeSelf;
        optionPanel.SetActive(willShow);

        if (gameManager != null)
        {
            gameManager.SetPaused(willShow);
        }
    }

    private void OnVolumeChanged(float value) => AudioListener.volume = value;

    private void OnDifficultyChanged(int index) => warningPanel.SetActive(true);

    private void ApplyDifficultyAndRestart()
    {
        warningPanel.SetActive(false);
        optionPanel.SetActive(false);

        if (gameManager != null)
        {
            gameManager.RestartCurrentStage();
        }
    }

    private void CancelDifficultyChange() => warningPanel.SetActive(false);

    private void OnResolutionChanged(int index)
    {
        if (index == 0) Screen.SetResolution(1920, 1080, true);
        else if (index == 1) Screen.SetResolution(1280, 720, false);
    }

    private void OnFrameRateChanged(int index)
    {
        Application.targetFrameRate = index switch
        {
            0 => 60,
            1 => 120,
            2 => 144,
            _ => 60
        };
    }
}