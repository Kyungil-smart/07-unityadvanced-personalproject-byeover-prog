using System;
using UnityEngine;
using TMPro;
using GnalIhu.Rhythm;
using System.Collections.Generic;

namespace _Game.Scripts.System
{
    [Serializable]
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

        private bool isPaused;

        private void OnEnable()
        {
            isPaused = false;
            ResetSteps();

            if (conductor != null)
                conductor.OnBeat += CheckTutorialStep;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
        }

        private void OnDisable()
        {
            if (conductor != null)
                conductor.OnBeat -= CheckTutorialStep;
        }

        private void ResetSteps()
        {
            if (steps == null) return;
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i] == null) continue;
                steps[i].isTriggered = false;
            }
        }

        private void CheckTutorialStep(int currentBeat)
        {
            if (isPaused) return;
            if (steps == null || steps.Count == 0) return;

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step == null) continue;

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

            if (conductor != null)
                conductor.PauseMusic();

            if (tutorialText != null)
                tutorialText.text = step.message;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);
        }

        private void Update()
        {
            if (!isPaused) return;

            if (Input.GetKeyDown(KeyCode.Space))
                ResumeGame();
        }

        private void ResumeGame()
        {
            isPaused = false;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            if (conductor != null)
                conductor.ResumeMusic();
        }
    }
}