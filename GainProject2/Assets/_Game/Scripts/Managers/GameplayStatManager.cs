using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Rhythm;

[DisallowMultipleComponent]
public sealed class GameplayStatManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private JudgeSystem judgeSystem;
    [SerializeField] private BossAndFeverUI feverUI;
    [SerializeField] private GameResultUI resultUI;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("UI 텍스트")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text comboText;

    [Header("피버 설정")]
    [SerializeField] private float feverMax = 100f;
    [SerializeField] private float feverChargePerPerfect = 5f;
    [SerializeField] private float feverChargePerGood = 2f;
    [SerializeField] private float feverDuration = 10f;
    [SerializeField] private int feverScoreMultiplier = 2;

    private int currentScore;
    private int currentCombo;
    private int maxCombo;

    private int perfectCount;
    private int goodCount;
    private int missCount;

    private float currentFever;
    private bool isFeverActive;
    private float feverTimer;

    private int feverPerfectCombo;
    private int feverBonusGiven;

    public float FeverBonusProgress { get; private set; }

    private void OnEnable()
    {
        if (judgeSystem != null) judgeSystem.OnJudged += HandleJudged;
        if (GameManager.Instance != null && GameManager.Instance.Events != null)
        {
            GameManager.Instance.Events.StageStarted += HandleStageStarted;
            GameManager.Instance.Events.SongEnded += HandleSongEnded;
        }
    }

    private void OnDisable()
    {
        if (judgeSystem != null) judgeSystem.OnJudged -= HandleJudged;
        if (GameManager.Instance != null && GameManager.Instance.Events != null)
        {
            GameManager.Instance.Events.StageStarted -= HandleStageStarted;
            GameManager.Instance.Events.SongEnded -= HandleSongEnded;
        }
    }

    private void HandleStageStarted(int stageIndex)
    {
        currentScore = 0;
        currentCombo = 0;
        maxCombo = 0;
        perfectCount = 0;
        goodCount = 0;
        missCount = 0;
        currentFever = 0f;
        isFeverActive = false;
        FeverBonusProgress = 0f;
        feverPerfectCombo = 0;
        feverBonusGiven = 0;
        UpdateUI();
    }

    private void Update()
    {
        if (isFeverActive)
        {
            feverTimer -= Time.deltaTime;
            if (feverTimer <= 0f)
            {
                StopFever();
            }

            if (GameManager.Instance != null && GameManager.Instance.Audio != null)
            {
                float len = Mathf.Max(0.01f, GameManager.Instance.Audio.CurrentSongLength);
                FeverBonusProgress += Time.deltaTime / len; 
            }
        }
    }

    private void HandleJudged(JudgeResult result)
    {
        if (result == JudgeResult.Perfect)
        {
            perfectCount++;
            AddScore(100);
            AddFever(feverChargePerPerfect);
            
            if (isFeverActive)
            {
                playerHealth.Heal(1, false);
                feverPerfectCombo++;
                if (feverPerfectCombo % 20 == 0 && feverBonusGiven < 4)
                {
                    playerHealth.Heal(1, true);
                    feverBonusGiven++;
                }
            }
            
            AddCombo();
        }
        else if (result == JudgeResult.Good)
        {
            goodCount++;
            AddScore(50);
            AddFever(feverChargePerGood);

            if (isFeverActive)
            {
                playerHealth.Heal(1, false);
                feverPerfectCombo = 0;
            }
            
            AddCombo();
        }
        else if (result == JudgeResult.Miss)
        {
            missCount++;
            ResetCombo();
            feverPerfectCombo = 0;
        }
    }

    private void AddCombo()
    {
        currentCombo++;
        if (currentCombo > maxCombo) maxCombo = currentCombo;

        if (!isFeverActive && currentCombo % 10 == 0)
        {
            playerHealth.Heal(1, false);
        }

        UpdateUI();
    }

    private void ResetCombo()
    {
        currentCombo = 0;
        UpdateUI();
    }

    private void AddScore(int baseScore)
    {
        int multiplier = isFeverActive ? feverScoreMultiplier : 1;
        int comboBonus = currentCombo / 10;
        currentScore += (baseScore + comboBonus) * multiplier;
        UpdateUI();
    }

    private void AddFever(float amount)
    {
        if (isFeverActive) return;

        currentFever += amount;
        if (currentFever >= feverMax)
        {
            StartFever();
        }
    }

    private void StartFever()
    {
        currentFever = feverMax;
        isFeverActive = true;
        feverTimer = feverDuration;
        feverPerfectCombo = 0;
        feverBonusGiven = 0;
        
        if (feverUI != null) feverUI.SetFeverState(true);
    }

    private void StopFever()
    {
        currentFever = 0f;
        isFeverActive = false;
        feverPerfectCombo = 0;
        feverBonusGiven = 0;

        if (feverUI != null) feverUI.SetFeverState(false);
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = currentScore.ToString("D6");
        if (comboText != null) comboText.text = currentCombo > 0 ? currentCombo.ToString() : string.Empty;
    }

    private void HandleSongEnded()
    {
        float accuracy = CalculateAccuracy();
        if (resultUI != null) resultUI.ShowResult(true, accuracy);
    }

    private float CalculateAccuracy()
    {
        int totalNotes = perfectCount + goodCount + missCount;
        if (totalNotes == 0) return 0f;

        float score = (perfectCount * 1f) + (goodCount * 0.5f);
        return score / totalNotes;
    }
}