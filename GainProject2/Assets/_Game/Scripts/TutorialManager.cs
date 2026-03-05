using System; // <- 추가된 부분
using UnityEngine;
using TMPro;
using GnalIhu.Rhythm;
using System.Collections.Generic;

namespace _Game.Scripts.System
{
    [Serializable] // <- System. 을 빼고 직접 호출하도록 수정
    public class TutorialStep
    {
        [Tooltip("정지할 박자 (예: 4번째 박자에 멈춤 = 4)")]
        public int targetBeat;
        
        [Tooltip("출력할 설명 텍스트"), TextArea(3, 5)]
        public string message;
        
        [HideInInspector]
        public bool isTriggered;
    }

    [DisallowMultipleComponent]
    public sealed class TutorialManager : MonoBehaviour
    {
        [Header("시스템 연결")]
        [SerializeField] private RhythmConductor conductor;
        
        [Header("UI 연결")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI tutorialText;
        
        [Header("튜토리얼 단계 설정")]
        [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

        private bool isPaused = false;

        private void OnEnable()
        {
            if (conductor != null)
            {
                conductor.OnBeat += CheckTutorialStep;
            }
        }

        private void OnDisable()
        {
            if (conductor != null)
            {
                conductor.OnBeat -= CheckTutorialStep;
            }
        }

        private void CheckTutorialStep(int currentBeat)
        {
            if (isPaused) return;

            foreach (var step in steps)
            {
                if (!step.isTriggered && currentBeat == step.targetBeat)
                {
                    TriggerStep(step);
                    break;
                }
            }
        }

        private void TriggerStep(TutorialStep step)
        {
            step.isTriggered = true;
            isPaused = true;
            
            conductor.PauseMusic();
            Time.timeScale = 0f; 
            
            tutorialText.text = step.message;
            tutorialPanel.SetActive(true);
        }

        private void Update()
        {
            if (isPaused && Input.GetKeyDown(KeyCode.Space))
            {
                ResumeGame();
            }
        }

        private void ResumeGame()
        {
            isPaused = false;
            tutorialPanel.SetActive(false);
            
            Time.timeScale = 1f;
            conductor.ResumeMusic();
        }
    }
}