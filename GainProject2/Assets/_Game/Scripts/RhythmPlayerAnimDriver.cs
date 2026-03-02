using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class RhythmPlayerAnimDriver : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("플레이어 Animator")]
        [SerializeField] private Animator playerAnimator;

        [Header("입력")]
        [Tooltip("리듬 입력 키(1키)")]
        [SerializeField] private KeyCode rhythmKey = KeyCode.Space;

        [Header("Animator 파라미터")]
        [Tooltip("Trigger 파라미터 이름(Animator Controller)")]
        [SerializeField] private string attackTriggerName = "Attack";

        private int _attackTriggerHash;
        private bool _hasAttackParam;

        private void Awake()
        {
            if (playerAnimator == null)
                playerAnimator = GetComponentInChildren<Animator>();

            _attackTriggerHash = Animator.StringToHash(attackTriggerName);
            _hasAttackParam = HasParameter(attackTriggerName);
        }

        private void Update()
        {
            if (Input.GetKeyDown(rhythmKey))
            {
                if (playerAnimator != null && _hasAttackParam)
                    playerAnimator.SetTrigger(_attackTriggerHash);
            }
        }

        private bool HasParameter(string paramName)
        {
            if (string.IsNullOrEmpty(paramName) || playerAnimator == null || playerAnimator.runtimeAnimatorController == null) 
                return false;

            foreach (AnimatorControllerParameter param in playerAnimator.parameters)
            {
                if (param.name == paramName) return true;
            }
            return false;
        }
    }
}