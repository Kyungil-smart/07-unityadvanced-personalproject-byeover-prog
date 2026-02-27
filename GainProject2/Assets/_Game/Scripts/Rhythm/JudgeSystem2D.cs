using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    public enum JudgeResult
    {
        None,
        Perfect,
        Good,
        Miss
    }

    [DisallowMultipleComponent]
    public sealed class JudgeSystem2D : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("리듬 시간 기준(필수)")]
        [SerializeField] private RhythmConductor2D conductor;

        [Header("입력")]
        [Tooltip("리듬 입력 키(1키)")]
        [SerializeField] private KeyCode rhythmKey = KeyCode.Space;

        [Header("판정 윈도우")]
        [Tooltip("Perfect 판정(ms)")]
        [Min(1)]
        [SerializeField] private float perfectWindowMs = 50f;

        [Tooltip("Good 판정(ms)")]
        [Min(1)]
        [SerializeField] private float goodWindowMs = 100f;

        [Header("옵션")]
        [Tooltip("타이밍 밖 입력도 Miss 판정으로 처리할지(데미지 없음)")]
        [SerializeField] private bool punishBadInput = false;

        [Header("디버그")]
        [Tooltip("히트라인 미스 발생 시 이벤트를 보낼지")]
        [SerializeField] private bool emitMissOnTimeout = true;

        public event Action<JudgeResult> OnJudged;

        private readonly List<NoteMonster2D> _activeNotes = new List<NoteMonster2D>(128);
        private int _headIndex;

        public void Register(NoteMonster2D note)
        {
            if (note == null) return;
            _activeNotes.Add(note);
        }

        private void Awake()
        {
            if (conductor == null) conductor = FindFirstObjectByType<RhythmConductor2D>();
        }

        private void Update()
        {
            if (conductor == null || !conductor.IsPlaying) return;

            double now = conductor.SongTimeSeconds;

            ProcessTimeoutAtHitLine(now);

            if (Input.GetKeyDown(rhythmKey))
                ProcessInput(now);
        }

        private void ProcessTimeoutAtHitLine(double now)
        {
            float missThreshold = goodWindowMs * 0.001f;

            for (; _headIndex < _activeNotes.Count; _headIndex++)
            {
                var note = _activeNotes[_headIndex];
                if (note == null) continue;

                if (!note.gameObject.activeSelf)
                    continue;

                float diff = (float)(now - note.TargetHitTime);

                if (diff <= missThreshold)
                    break;

                if (emitMissOnTimeout)
                    OnJudged?.Invoke(JudgeResult.Miss);
            }

            if (_headIndex > 64)
            {
                _activeNotes.RemoveRange(0, _headIndex);
                _headIndex = 0;
            }
        }

        private void ProcessInput(double now)
        {
            NoteMonster2D best = null;
            float bestAbs = float.MaxValue;

            for (int i = _headIndex; i < _activeNotes.Count; i++)
            {
                var note = _activeNotes[i];
                if (note == null) continue;
                if (!note.gameObject.activeSelf) continue;

                float diff = (float)(now - note.TargetHitTime);
                float abs = Mathf.Abs(diff);

                if (abs < bestAbs)
                {
                    bestAbs = abs;
                    best = note;
                }

                if (diff < -goodWindowMs * 0.001f)
                    break;
            }

            float perfect = perfectWindowMs * 0.001f;
            float good = goodWindowMs * 0.001f;

            if (best == null)
            {
                if (punishBadInput)
                    OnJudged?.Invoke(JudgeResult.Miss);
                return;
            }

            JudgeResult result;
            if (bestAbs <= perfect) result = JudgeResult.Perfect;
            else if (bestAbs <= good) result = JudgeResult.Good;
            else result = JudgeResult.None;

            if (result == JudgeResult.Perfect || result == JudgeResult.Good)
            {
                best.Despawn();
                OnJudged?.Invoke(result);
                return;
            }

            if (punishBadInput)
                OnJudged?.Invoke(JudgeResult.Miss);
        }
    }
}