using UnityEngine;
using UnityEngine.UI;

public sealed class OptionMenuUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject optionPanel;

    [Header("볼륨")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField, Tooltip("BGM 숫자 표시")] private TMPro.TMP_Text bgmValueText;
    [SerializeField, Tooltip("SFX 숫자 표시")] private TMPro.TMP_Text sfxValueText;
    [SerializeField, Tooltip("BGM AudioSource")] private AudioSource bgmSource;
    [SerializeField, Tooltip("SFX AudioSource")] private AudioSource sfxSource;

    [Header("히트라인(부적)")]
    [SerializeField, Tooltip("히트라인 오브젝트들")] private GameObject[] hitLineObjects;
    [SerializeField] private Toggle hitLineToggle;

    [Header("버튼")]
    [SerializeField] private Button hideButton;
    [SerializeField] private Button quitButton;

    private GameManager gameManager;
    private bool isOpen;

    private void Awake()
    {
        gameManager = GameManager.Instance;

        if (optionPanel != null) optionPanel.SetActive(false);
        isOpen = false;

        if (bgmSlider != null)
        {
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
            bgmSlider.onValueChanged.AddListener(OnBgmChanged);
            OnBgmChanged(bgmSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            OnSfxChanged(sfxSlider.value);
        }
        
        if (hitLineToggle != null)
        {
            hitLineToggle.isOn = true;
            hitLineToggle.onValueChanged.AddListener(OnHitLineToggled);
        }

        // 버튼
        if (hideButton != null) hideButton.onClick.AddListener(Close);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen) Close();
            else Open();
        }
    }

    public void Open()
    {
        if (optionPanel == null) return;
        isOpen = true;
        optionPanel.SetActive(true);

        if (gameManager != null) gameManager.SetPaused(true);
    }

    public void Close()
    {
        if (optionPanel == null) return;
        isOpen = false;
        optionPanel.SetActive(false);

        if (gameManager != null) gameManager.SetPaused(false);
    }

    private void OnBgmChanged(float value)
    {
        if (bgmSource != null) bgmSource.volume = value;
        if (bgmValueText != null) bgmValueText.text = Mathf.RoundToInt(value * 100).ToString();
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void OnSfxChanged(float value)
    {
        if (sfxSource != null) sfxSource.volume = value;
        if (sfxValueText != null) sfxValueText.text = Mathf.RoundToInt(value * 100).ToString();
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void OnHitLineToggled(bool visible)
    {
        if (hitLineObjects == null) return;
        foreach (var obj in hitLineObjects)
        {
            if (obj != null) obj.SetActive(visible);
        }
    }

    private void QuitGame()
    {
        if (gameManager != null) gameManager.QuitGame();
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}