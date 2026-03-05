using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class OptionMenuUI : MonoBehaviour
{
    [Header("옵션 패널")]
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject warningPanel;

    [Header("볼륨 컨트롤")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField, Tooltip("BGM 전용 볼륨 (선택, 비어있으면 마스터만 사용)")]
    private Slider bgmVolumeSlider;
    [SerializeField, Tooltip("SFX 전용 볼륨 (선택)")]
    private Slider sfxVolumeSlider;

    [Header("게임 설정")]
    [SerializeField] private Dropdown difficultyDropdown;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Dropdown frameRateDropdown;

    [Header("글자 속도")]
    [SerializeField, Tooltip("텍스트 출력 속도 슬라이더 (1~100)")]
    private Slider textSpeedSlider;
    [SerializeField, Tooltip("현재 속도 표시 텍스트 (선택)")]
    private Text textSpeedLabel;

    [Header("버튼")]
    [SerializeField] private Button applyDifficultyButton;
    [SerializeField] private Button cancelDifficultyButton;
    [SerializeField, Tooltip("크레딧 버튼 (선택)")]
    private Button creditButton;
    [SerializeField, Tooltip("크레딧 패널 (선택)")]
    private GameObject creditPanel;

    [Header("전체 화면")]
    [SerializeField, Tooltip("전체 화면 토글 (선택)")]
    private Toggle fullscreenToggle;

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameManager.Instance;

        if (optionPanel != null) optionPanel.SetActive(false);
        if (warningPanel != null) warningPanel.SetActive(false);
        if (creditPanel != null) creditPanel.SetActive(false);

        // 볼륨
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        // 게임 설정
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (frameRateDropdown != null)
            frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);

        // 버튼
        if (applyDifficultyButton != null)
            applyDifficultyButton.onClick.AddListener(ApplyDifficultyAndRestart);
        if (cancelDifficultyButton != null)
            cancelDifficultyButton.onClick.AddListener(CancelDifficultyChange);

        // 글자 속도 슬라이더
        if (textSpeedSlider != null)
        {
            textSpeedSlider.minValue = 5f;
            textSpeedSlider.maxValue = 100f;
            textSpeedSlider.value = 40f;
            textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
        }

        // 크레딧 버튼
        if (creditButton != null)
            creditButton.onClick.AddListener(ToggleCredit);

        // 전체 화면 토글
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 크레딧이 열려있으면 크레딧 닫기
            if (creditPanel != null && creditPanel.activeSelf)
            {
                creditPanel.SetActive(false);
                return;
            }

            ToggleOptionMenu();
        }
    }

    public void ToggleOptionMenu()
    {
        if (warningPanel != null && warningPanel.activeSelf) return;

        bool willShow = !optionPanel.activeSelf;
        optionPanel.SetActive(willShow);

        if (gameManager != null)
        {
            gameManager.SetPaused(willShow);
        }
    }

    // =============== 볼륨 ===============

    private void OnVolumeChanged(float value) => AudioListener.volume = value;

    private void OnBgmVolumeChanged(float value)
    {
        // AudioManager에 BGM 볼륨 연결 (필요시 확장)
        // 현재는 마스터 볼륨만 사용
    }

    private void OnSfxVolumeChanged(float value)
    {
        // AudioManager에 SFX 볼륨 연결 (필요시 확장)
    }

    // =============== 게임 설정 ===============

    private void OnDifficultyChanged(int index)
    {
        if (warningPanel != null) warningPanel.SetActive(true);
    }

    private void ApplyDifficultyAndRestart()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);

        if (gameManager != null)
        {
            gameManager.RestartCurrentStage();
        }
    }

    private void CancelDifficultyChange()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
    }

    private void OnResolutionChanged(int index)
    {
        switch (index)
        {
            case 0: Screen.SetResolution(1920, 1080, Screen.fullScreen); break;
            case 1: Screen.SetResolution(1280, 720, Screen.fullScreen); break;
            case 2: Screen.SetResolution(1600, 900, Screen.fullScreen); break;
        }
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

    // =============== 글자 속도 ===============

    private void OnTextSpeedChanged(float value)
    {
        // StoryManager가 현재 씬에 있으면 즉시 적용
        var storyManager = FindFirstObjectByType<StoryManager>();
        if (storyManager != null)
        {
            storyManager.CharactersPerSecond = value;
        }

        // 라벨 갱신
        if (textSpeedLabel != null)
        {
            textSpeedLabel.text = $"글자 속도: {value:0}";
        }

        // PlayerPrefs에 저장 (다음 씬에서도 유지)
        PlayerPrefs.SetFloat("TextSpeed", value);
    }

    // =============== 크레딧 ===============

    private void ToggleCredit()
    {
        if (creditPanel == null) return;
        creditPanel.SetActive(!creditPanel.activeSelf);
    }

    // =============== 전체 화면 ===============

    private void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}